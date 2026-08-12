/**
 * StoryVerse Theme-wise Loader Engine
 * Handles page changes, form actions, link navigations, and API calls (Fetch/XHR/jQuery AJAX)
 */
(function (window, document) {
    'use strict';

    let overlayEl = null;
    let textEl = null;
    let progressBarEl = null;
    let safetyTimer = null;
    let activeRequestCount = 0;
    let progressTimer = null;
    let currentProgress = 0;

    const StoryVerseLoader = {
        /**
         * Initialize DOM references and attach global event handlers
         */
        init: function () {
            overlayEl = document.getElementById('svLoaderOverlay');
            textEl = document.getElementById('svLoaderText');
            progressBarEl = document.getElementById('svTopProgressBar');

            // Fade out overlay when initial DOM & resources load
            this.hide();

            // Intercept Page Navigations and Actions
            this._setupNavigationListeners();
            this._setupFormListeners();
            this._setupBfCacheListener();
            this._setupApiInterceptors();
        },

        /**
         * Show the full-screen glass loader overlay
         * @param {Object|string} options
         */
        show: function (options) {
            const text = (typeof options === 'string' ? options : (options && options.text)) || 'Loading...';
            
            if (!overlayEl) {
                overlayEl = document.getElementById('svLoaderOverlay');
                textEl = document.getElementById('svLoaderText');
            }

            if (textEl) {
                textEl.textContent = text;
            }

            if (overlayEl) {
                overlayEl.classList.add('active');
            }

            // Safety timeout guard (auto-dismiss after 10 seconds to prevent frozen overlay)
            clearTimeout(safetyTimer);
            safetyTimer = setTimeout(() => {
                this.hide();
            }, 10000);
        },

        /**
         * Hide the full-screen loader overlay
         */
        hide: function () {
            clearTimeout(safetyTimer);
            if (!overlayEl) {
                overlayEl = document.getElementById('svLoaderOverlay');
            }
            if (overlayEl) {
                overlayEl.classList.remove('active');
            }
        },

        /**
         * Start or advance top glowing peacock progress bar
         */
        startProgress: function () {
            if (!progressBarEl) {
                progressBarEl = document.getElementById('svTopProgressBar');
            }
            if (!progressBarEl) return;

            progressBarEl.classList.add('active');
            if (currentProgress === 0) {
                currentProgress = 15;
                progressBarEl.style.width = currentProgress + '%';
            }

            clearInterval(progressTimer);
            progressTimer = setInterval(() => {
                if (currentProgress < 85) {
                    currentProgress += Math.random() * 10;
                    progressBarEl.style.width = currentProgress + '%';
                }
            }, 300);
        },

        /**
         * Finish and hide top glowing peacock progress bar
         */
        endProgress: function () {
            if (!progressBarEl) {
                progressBarEl = document.getElementById('svTopProgressBar');
            }
            clearInterval(progressTimer);
            if (!progressBarEl) return;

            currentProgress = 100;
            progressBarEl.style.width = '100%';

            setTimeout(() => {
                progressBarEl.classList.remove('active');
                setTimeout(() => {
                    progressBarEl.style.width = '0%';
                    currentProgress = 0;
                }, 300);
            }, 200);
        },

        /**
         * Intercept link navigation clicks
         */
        _setupNavigationListeners: function () {
            const self = this;
            document.addEventListener('click', function (e) {
                // Find closest <a> tag
                const link = e.target.closest('a');
                if (!link) return;

                const href = link.getAttribute('href');
                if (!href) return;

                // Ignore special links
                if (
                    href.startsWith('#') ||
                    href.startsWith('javascript:') ||
                    href.startsWith('mailto:') ||
                    href.startsWith('tel:') ||
                    link.getAttribute('target') === '_blank' ||
                    link.hasAttribute('download') ||
                    link.getAttribute('data-no-loader') === 'true' ||
                    link.hasAttribute('data-bs-toggle') ||
                    link.hasAttribute('data-bs-dismiss') ||
                    e.ctrlKey || e.metaKey || e.shiftKey || e.altKey
                ) {
                    return;
                }

                // If explicit overlay requested via data attribute, show full glass overlay
                if (link.getAttribute('data-loader-overlay') === 'true' || link.hasAttribute('data-loader-text')) {
                    const text = link.getAttribute('data-loader-text') || 'Loading page...';
                    self.show({ text: text });
                } else {
                    // Fast lightweight top progress bar for smooth navigation
                    self.startProgress();
                }
            }, true);
        },

        /**
         * Intercept Form submissions
         */
        _setupFormListeners: function () {
            const self = this;
            document.addEventListener('submit', function (e) {
                const form = e.target;
                if (!form || form.getAttribute('data-no-loader') === 'true') {
                    return;
                }

                // Check HTML5 validity
                if (typeof form.checkValidity === 'function' && !form.checkValidity()) {
                    return;
                }

                // Check jQuery unobtrusive validation if present
                if (window.jQuery && window.jQuery(form).data('validator')) {
                    if (!window.jQuery(form).valid()) {
                        return;
                    }
                }

                const customText = form.getAttribute('data-loader-text') || 'Processing...';
                self.show({ text: customText });

                // Add button spinner state to submit button
                const submitBtn = form.querySelector('button[type="submit"], input[type="submit"]');
                if (submitBtn && !submitBtn.classList.contains('btn-sv-loading')) {
                    submitBtn.classList.add('btn-sv-loading');
                }
            }, true);
        },

        /**
         * Handle browser Back/Forward restore (bfcache)
         */
        _setupBfCacheListener: function () {
            const self = this;
            window.addEventListener('pageshow', function (event) {
                if (event.persisted) {
                    self.hide();
                    self.endProgress();
                    // Remove loading state from any submit buttons
                    document.querySelectorAll('.btn-sv-loading').forEach(el => el.classList.remove('btn-sv-loading'));
                }
            });
        },

        /**
         * Intercept native Fetch, XMLHttpRequest, and jQuery AJAX calls
         */
        _setupApiInterceptors: function () {
            const self = this;

            function isAutoSaveRequest(url, options) {
                const urlStr = (typeof url === 'string' ? url : (url && url.url ? url.url : '')).toLowerCase();
                if (options && (options.skipLoader === true || options.showLoader === false)) return true;
                if (options && options.headers && (options.headers['X-Skip-Loader'] === 'true' || options.headers['x-skip-loader'] === 'true')) return true;
                if (urlStr.includes('/chapters/savedraft') || urlStr.includes('/chapters/savecontent') || urlStr.includes('autosave') || urlStr.includes('savedraft')) return true;
                return false;
            }

            // 1. Intercept native fetch
            if (window.fetch) {
                const originalFetch = window.fetch;
                window.fetch = function () {
                    const args = arguments;
                    const url = args[0];
                    const options = args[1] || {};

                    if (isAutoSaveRequest(url, options)) {
                        return originalFetch.apply(this, args);
                    }

                    const isExplicitOverlay = options.showLoaderOverlay === true;
                    const message = options.loaderText || 'Loading...';

                    onRequestStart(isExplicitOverlay, message);

                    return originalFetch.apply(this, args)
                        .then(response => {
                            onRequestEnd(isExplicitOverlay);
                            return response;
                        })
                        .catch(error => {
                            onRequestEnd(isExplicitOverlay);
                            throw error;
                        });
                };
            }

            // 2. Intercept native XMLHttpRequest
            if (window.XMLHttpRequest) {
                const originalOpen = XMLHttpRequest.prototype.open;
                const originalSend = XMLHttpRequest.prototype.send;

                XMLHttpRequest.prototype.open = function () {
                    this._svUrl = arguments[1];
                    return originalOpen.apply(this, arguments);
                };

                XMLHttpRequest.prototype.send = function () {
                    const xhr = this;
                    if (isAutoSaveRequest(xhr._svUrl)) {
                        return originalSend.apply(this, arguments);
                    }

                    const isExplicitOverlay = xhr._svShowOverlay === true;

                    onRequestStart(isExplicitOverlay, xhr._svLoaderText);

                    xhr.addEventListener('loadend', function () {
                        onRequestEnd(isExplicitOverlay);
                    });

                    return originalSend.apply(this, arguments);
                };
            }

            // 3. Intercept jQuery AJAX if present
            if (window.jQuery) {
                window.jQuery(document).on('ajaxStart.svLoader', function (e, xhr, settings) {
                    const url = settings ? settings.url : '';
                    if (isAutoSaveRequest(url, settings)) return;
                    if (activeRequestCount === 0) {
                        self.startProgress();
                    }
                });

                window.jQuery(document).on('ajaxStop.svLoader ajaxError.svLoader', function () {
                    self.endProgress();
                });
            }
        }
    };

    // Attach to global window object
    window.StoryVerseLoader = StoryVerseLoader;

    // Auto-init on DOMReady
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            StoryVerseLoader.init();
            StoryVerseLoader.endProgress();
        });
    } else {
        StoryVerseLoader.init();
        StoryVerseLoader.endProgress();
    }

    // Ensure loader hides once full window resources load
    window.addEventListener('load', function () {
        StoryVerseLoader.hide();
        StoryVerseLoader.endProgress();
    });

})(window, document);
