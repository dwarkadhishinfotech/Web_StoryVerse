/**
 * StoryVerse - Intelligent Writing Assistant Engine
 * Context-Aware Story Entity Intelligence Pipeline V5
 * 
 * CORE PRINCIPLE: "HOW IS THIS TERM BEING USED IN THIS STORY?"
 * NOT: "DO I RECOGNIZE THIS WORD?"
 */

window.StoryIntelligence = (function () {

    // 1. SMALL LINGUISTIC LANGUAGE FILTER (Grammatical closed-class words & structural meta text ONLY)
    // IMPORTANT: This is a language guardrail, NOT a name dictionary or blacklist.
    // It must NEVER be used to judge whether an unknown fictional word (e.g. Zayrith, Kaelith) is valid.
    const GRAMMATICAL_WORDS = new Set([
        "both", "all", "each", "every", "either", "neither", "some", "any", "many",
        "few", "several", "another", "other", "one", "two", "three", "four", "five",
        "then", "finally", "meanwhile", "however", "and", "but", "so", "yet", "this",
        "that", "these", "those", "someone", "everyone", "nobody", "something",
        "anything", "nothing", "somebody", "everybody", "no one", "who", "whom",
        "whose", "which", "what", "where", "when", "why", "how", "here", "there",
        "shall", "will", "would", "should", "could", "can", "may", "might", "must",
        "hi", "hello", "hey", "reporter", "anchor", "narrator"
    ]);

    const PRONOUNS = new Set([
        "i", "me", "my", "mine", "myself",
        "you", "your", "yours", "yourself", "yourselves",
        "he", "him", "his", "himself",
        "she", "her", "hers", "herself",
        "it", "its", "itself",
        "we", "us", "our", "ours", "ourselves",
        "they", "them", "their", "theirs", "themselves"
    ]);

    const DISCOURSE_CONNECTORS = new Set([
        "then", "and", "but", "so", "finally", "meanwhile", "suddenly", "however",
        "therefore", "also", "still", "yet", "now", "well", "hey", "hi", "hello",
        "yes", "no", "haan", "nahi", "achha", "acha", "arre", "oye", "oh", "ah",
        "besides", "instead", "otherwise", "next", "later", "soon", "first", "second",
        "third", "lastly", "furthermore", "moreover", "the"
    ]);

    const META_TEXT = new Set([
        "to be continued", "the end", "end", "fin", "continued", "continue",
        "next chapter", "chapter one", "chapter two", "chapter three",
        "chapter four", "chapter five", "part one", "part two", "part three",
        "part four", "part five", "epilogue", "prologue"
    ]);

    // 2. CONTEXTUAL INDICATORS & SIGNALS (Used ONLY for evidence scoring, not for filtering)
    const HUMAN_ACTION_VERBS = new Set([
        "entered", "enters", "walked", "walks", "ran", "runs", "stood", "stands",
        "sat", "sits", "smiled", "smiles", "laughed", "laughs", "cried", "cries",
        "spoke", "speaks", "said", "says", "asked", "asks", "replied", "replies",
        "whispered", "whispers", "shouted", "shouts", "looked", "looks", "stared",
        "stares", "turned", "turns", "nodded", "nods", "sighed", "sighs", "thought",
        "thinks", "wondered", "wonders", "remembered", "remembers", "decided", "decides",
        "promised", "promises", "refused", "refuses", "agreed", "agrees", "followed",
        "follows", "arrived", "arrives", "left", "leaves", "returned", "returns",
        "held", "holds", "touched", "touches", "opened", "opens", "closed", "closes",
        "screamed", "screams", "muttered", "mutters", "exclaimed", "exclaims", "grinned",
        "grins", "glanced", "glances", "smirked", "smirks", "gasped", "gasps", "knew", "knows"
    ]);

    const COLLECTIVE_ENTITY_VERBS = new Set([
        "awakened", "awakens", "gathered", "gathers", "announced", "announces",
        "controls", "controlled", "ruled", "rules", "powered", "powers",
        "marched", "marches", "rose", "rises", "governed", "governs", "assembled",
        "established", "threatened", "protected"
    ]);

    const LOCATION_PREPOSITIONS = new Set([
        "in", "at", "from", "to", "near", "inside", "outside", "into", "onto",
        "towards", "toward", "beyond", "across", "within", "around", "above", "below"
    ]);

    const LOCATION_NOUNS = new Set([
        "city", "town", "village", "kingdom", "country", "palace", "castle", "temple",
        "fortress", "station", "island", "planet", "district", "street", "house",
        "mansion", "academy", "university", "forest", "mountain", "mountains", "river",
        "lake", "ocean", "sea", "realm", "haven", "bay", "port", "harbor", "valley",
        "peak", "brook", "falls", "bridge", "fort", "manor", "capital", "land"
    ]);

    const WORLD_ENTITY_KEYWORDS = new Set([
        "council", "order", "guild", "clan", "empire", "alliance", "syndicate",
        "society", "brotherhood", "legion", "assembly", "ministry", "coven", "guard",
        "sect", "federation", "union", "coalition", "faction", "house", "dynasty",
        "core", "system", "force", "cult", "covenant", "pact", "bloodline", "spire"
    ]);

    const EVENT_KEYWORDS = new Set([
        "explosion", "battle", "siege", "meeting", "incident", "coronation",
        "massacre", "invasion", "trial", "rebellion", "raid", "execution", "treaty",
        "fall", "rising", "disappearance", "murder", "war", "uprising", "attack",
        "accident", "ambush", "betrayal", "conflict", "conspiracy", "revolt"
    ]);

    const EVENT_ACTION_VERBS = new Set([
        "occurred", "occurs", "happened", "happens", "began", "begins", "started",
        "starts", "destroyed", "destroys", "unfolded", "unfolds", "changed", "changes",
        "took place", "erupted", "erupts", "broke out", "shook", "shakes"
    ]);

    const HONORIFICS = new Set([
        "mr", "mrs", "ms", "miss", "dr", "sir", "madam", "lord", "lady",
        "captain", "professor", "officer", "king", "queen", "prince", "princess",
        "judge", "inspector", "colonel", "general", "major", "commander", "detective",
        "agent", "chief", "master", "mistress", "emperor", "empress", "duke", "duchess"
    ]);

    function normalize(str) {
        if (!str) return "";
        return str.trim().toLowerCase().replace(/['’]s$/i, "");
    }

    function escapeRegExp(string) {
        return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }

    /**
     * Clean tokens: strip leading discourse connectors, determiners & pronouns from candidate start
     * e.g., ["Then", "Zayrith"] -> cleanName: "Zayrith", strippedPrefix: "Then"
     * e.g., ["The", "Eldorian", "Council"] -> cleanName: "Eldorian Council", strippedPrefix: "The"
     * e.g., ["Haan", "Arjun"] -> cleanName: "Arjun", strippedPrefix: "Haan"
     */
    function cleanCandidateTokens(initialWords) {
        let words = [...initialWords];
        let strippedPrefixes = [];
        let honorific = null;

        // Strip leading discourse connectors/determiners if multi-word
        while (words.length > 1 && DISCOURSE_CONNECTORS.has(words[0].toLowerCase())) {
            strippedPrefixes.push(words.shift());
        }

        // Strip leading pronouns if multi-word (e.g. "Her Zayrith" -> "Zayrith")
        while (words.length > 1 && PRONOUNS.has(words[0].toLowerCase())) {
            strippedPrefixes.push(words.shift());
        }

        // Handle Honorifics (e.g. "Lord Zayrith" -> honorific: "Lord", cleanWords: ["Zayrith"])
        if (words.length > 1 && HONORIFICS.has(words[0].toLowerCase())) {
            honorific = words.shift();
        }

        return {
            cleanWords: words,
            strippedPrefix: strippedPrefixes.join(" "),
            honorific: honorific
        };
    }

    /**
     * MAIN UNIFIED PIPELINE: detectEntities(text, cache, ignoredCandidatesSet)
     */
    function detectEntities(text, cache, ignoredCandidates = new Set()) {
        if (!text || text.trim().length < 2) {
            return { existingMatches: [], candidates: [], ambiguousCandidates: [] };
        }

        const rawText = text;

        // =========================================================================
        // PIPELINE STAGE 1: EXISTING ENTITY PRIORITY RESOLUTION (Confidence = 1.00)
        // =========================================================================
        const existingMatches = [];
        const existingMatchedRanges = [];
        const allCacheEntities = [];

        if (cache && cache.characters) {
            cache.characters.forEach(c => {
                allCacheEntities.push({ entity: c, type: "character", name: c.name });
                if (c.nicknames) {
                    c.nicknames.forEach(n => {
                        if (n && n.trim()) allCacheEntities.push({ entity: c, type: "character", name: n.trim() });
                    });
                }
            });
        }
        if (cache && cache.locations) {
            cache.locations.forEach(l => allCacheEntities.push({ entity: l, type: "location", name: l.name }));
        }
        if (cache && cache.worldEntities) {
            cache.worldEntities.forEach(w => allCacheEntities.push({ entity: w, type: "world", name: w.name }));
        }
        if (cache && cache.timelineEvents) {
            cache.timelineEvents.forEach(t => allCacheEntities.push({ entity: t, type: "timeline", name: t.title }));
        }

        // Sort longest entity names first to prevent partial substring overlaps
        allCacheEntities.sort((a, b) => b.name.length - a.name.length);

        const foundEntityIds = new Set();

        allCacheEntities.forEach(item => {
            const norm = normalize(item.name);
            if (!norm || norm.length < 2) return;

            const regex = new RegExp(`\\b${escapeRegExp(norm)}(?:['’]s)?\\b`, 'gi');
            let match;
            let count = 0;
            while ((match = regex.exec(rawText)) !== null) {
                count++;
                existingMatchedRanges.push({ start: match.index, end: match.index + match[0].length });
            }

            if (count > 0 && !foundEntityIds.has(item.type + "_" + item.entity.id)) {
                foundEntityIds.add(item.type + "_" + item.entity.id);
                existingMatches.push({
                    entity: item.entity,
                    type: item.type,
                    occurrences: count,
                    confidence: 1.00
                });
            }
        });

        // =========================================================================
        // PIPELINE STAGE 2: CANDIDATE EXTRACTION & TEXT SEGMENTATION
        // =========================================================================
        const rawCandidatesMap = new Map(); // normName -> data object

        function processCandidate(phrase, isDialogueSpeaker, sentenceContext, isPossessive) {
            if (!phrase || phrase.trim().length < 2) return;

            const originalPhrase = phrase.trim();
            const initialWords = originalPhrase.split(/\s+/);

            const { cleanWords, strippedPrefix, honorific } = cleanCandidateTokens(initialWords);
            if (cleanWords.length === 0) return;

            const cleanPhrase = cleanWords.join(" ").replace(/['’]s$/i, "");
            const normPhrase = normalize(cleanPhrase);

            if (!normPhrase || normPhrase.length < 2) return;

            // Session ignore check
            if (ignoredCandidates && (ignoredCandidates.has(normPhrase) || ignoredCandidates.has(originalPhrase.toLowerCase()))) {
                return;
            }

            if (!rawCandidatesMap.has(normPhrase)) {
                rawCandidatesMap.set(normPhrase, {
                    originalText: originalPhrase,
                    name: cleanPhrase,
                    normalizedName: normPhrase,
                    words: cleanWords,
                    strippedPrefix: strippedPrefix,
                    honorific: honorific,
                    isDialogueSpeaker: isDialogueSpeaker,
                    dialogueSpeakerCount: isDialogueSpeaker ? 1 : 0,
                    isPossessive: isPossessive || false,
                    occurrences: [sentenceContext].filter(Boolean)
                });
            } else {
                const existing = rawCandidatesMap.get(normPhrase);
                if (isDialogueSpeaker) {
                    existing.dialogueSpeakerCount += 1;
                    existing.isDialogueSpeaker = true;
                }
                if (sentenceContext) {
                    existing.occurrences.push(sentenceContext);
                }
                if (honorific && !existing.honorific) existing.honorific = honorific;
                if (strippedPrefix && !existing.strippedPrefix) existing.strippedPrefix = strippedPrefix;
                if (isPossessive) existing.isPossessive = true;
            }
        }

        // A. Extract Dialogue Speakers e.g. "Zayrith: We need to leave.", "Arjun Rawte:"
        const dialogueSpeakerRegex = /(?:^|\n|\.\s+)([A-Z][a-zA-Z0-9']*(?:\s+[A-Z][a-zA-Z0-9']*){0,2})\s*:/g;
        let dsMatch;
        while ((dsMatch = dialogueSpeakerRegex.exec(rawText)) !== null) {
            const speaker = dsMatch[1].trim();
            const startPos = dsMatch.index;
            const endPos = startPos + speaker.length;
            const overlapsExisting = existingMatchedRanges.some(r =>
                (startPos >= r.start && startPos < r.end) || (endPos > r.start && endPos <= r.end)
            );
            if (!overlapsExisting) {
                processCandidate(speaker, true, null, false);
            }
        }

        // B. Extract Proper Noun Phrases from Prose Sentences
        const sentenceRegex = /[^.!?\n]+[.!?\n]*/g;
        let sMatch;
        const sentences = [];
        let sIdx = 0;

        while ((sMatch = sentenceRegex.exec(rawText)) !== null) {
            sentences.push({ text: sMatch[0], index: sMatch.index, sIdx: sIdx++ });
        }

        sentences.forEach((sentObj) => {
            const sentence = sentObj.text;
            const sStart = sentObj.index;
            const nextSentence = sentences[sentObj.sIdx + 1] ? sentences[sentObj.sIdx + 1].text : "";

            // Phrase Extractor: 1 to 4 capitalized words
            const phraseRegex = /\b([A-Z][a-zA-Z0-9']*(?:\s+(?:of|the|and|in)\s+[A-Z][a-zA-Z0-9']*|\s+[A-Z][a-zA-Z0-9']*){0,3})\b/g;
            let pMatch;

            while ((pMatch = phraseRegex.exec(sentence)) !== null) {
                const phrase = pMatch[1].trim();
                const pStart = sStart + pMatch.index;
                const pEnd = pStart + phrase.length;

                const overlapsExisting = existingMatchedRanges.some(r =>
                    (pStart >= r.start && pStart < r.end) || (pEnd > r.start && pEnd <= r.end)
                );
                if (overlapsExisting) continue;

                const preContext = sentence.substring(Math.max(0, pMatch.index - 60), pMatch.index);
                const postContext = sentence.substring(pMatch.index + pMatch[0].length, Math.min(sentence.length, pMatch.index + pMatch[0].length + 60));
                const isSentenceStart = (pMatch.index === 0 || sentence.substring(0, pMatch.index).trim().length === 0);
                const isPossessive = /['’]s$/i.test(phrase);

                processCandidate(phrase, false, {
                    sentence: sentence.trim(),
                    nextSentence: nextSentence.trim(),
                    isSentenceStart,
                    preContext,
                    postContext
                }, isPossessive);
            }
        });

        // =========================================================================
        // PIPELINE STAGE 3: CONTEXT ANALYSIS & COMPETITIVE CLASSIFICATION
        // =========================================================================
        const highConfidenceCandidates = [];
        const ambiguousCandidates = [];

        rawCandidatesMap.forEach((cand) => {
            const normName = cand.normalizedName;
            const name = cand.name;
            const occurrencesCount = Math.max(cand.dialogueSpeakerCount, cand.occurrences.length);
            const words = cand.words;
            const isMultiWord = words.length >= 2;

            // ---------------------------------------------------------------------
            // STAGE 3A: SMALL LINGUISTIC LANGUAGE FILTER (Closed-class words & Meta)
            // ---------------------------------------------------------------------
            if (META_TEXT.has(normName)) return;

            if (!isMultiWord) {
                if (GRAMMATICAL_WORDS.has(normName) || PRONOUNS.has(normName) || /^\d+$/.test(normName)) {
                    return;
                }
            } else {
                const allWordsGrammatical = words.every(w => GRAMMATICAL_WORDS.has(w.toLowerCase()) || PRONOUNS.has(w.toLowerCase()));
                if (allWordsGrammatical) return;
            }

            // ---------------------------------------------------------------------
            // STAGE 3B: CONTEXTUAL EVIDENCE EVALUATION & COMPETITIVE SCORING
            // ---------------------------------------------------------------------
            let charScore = 0.0;
            let locScore = 0.0;
            let worldScore = 0.0;
            let timelineScore = 0.0;
            const evidence = [];

            // Context Signal 1: Possessive Context ("Zayrith's sword", "sword of Zayrith")
            if (cand.isPossessive) {
                charScore += 0.35;
                worldScore += 0.15;
                evidence.push("Possessive syntax");
            }

            // Context Signal 2: Honorific Title Prefix ("Lord Zayrith", "Captain Dravak")
            if (cand.honorific) {
                charScore += 0.45;
                evidence.push(`Title/Honorific (${cand.honorific})`);
            }

            // Context Signal 3: Explicit Dialogue Speaker ("Zayrith: We need...")
            if (cand.dialogueSpeakerCount > 0) {
                if (normName === "reporter" || normName === "anchor" || normName === "narrator") {
                    charScore += 0.20;
                } else {
                    charScore += 0.55;
                    evidence.push(`Dialogue speaker (${cand.dialogueSpeakerCount}x)`);
                }
            }

            // Context Signal 4: Phrase Keywords (World / Location / Timeline)
            words.forEach(w => {
                const lw = w.toLowerCase();
                if (LOCATION_NOUNS.has(lw)) {
                    locScore += 0.65;
                    evidence.push(`Location noun "${w}"`);
                }
                if (WORLD_ENTITY_KEYWORDS.has(lw)) {
                    worldScore += 0.65;
                    evidence.push(`World entity keyword "${w}"`);
                }
                if (EVENT_KEYWORDS.has(lw)) {
                    timelineScore += 0.65;
                    evidence.push(`Timeline event keyword "${w}"`);
                }
            });

            // Context Signal 5: Determiner + Capitalized Phrase ("The Order of Ash", "The Veylora")
            if (cand.strippedPrefix && cand.strippedPrefix.toLowerCase() === "the") {
                worldScore += 0.30;
                timelineScore += 0.20;
            }

            // Context Signal 6: Prose Occurrence Analysis (Surrounding Verbs, Prepositions, Pronouns)
            cand.occurrences.forEach(occ => {
                const preLower = occ.preContext.toLowerCase();
                const postLower = occ.postContext.toLowerCase();

                const postWords = postLower.replace(/^[^a-zA-Z0-9]+/, "").split(/\s+/);
                const preWords = preLower.replace(/[^a-zA-Z0-9]+$/, "").split(/\s+/);

                const nextWord = (postWords[0] || "").replace(/[^a-zA-Z0-9]/g, "");
                const secondNextWord = (postWords[1] || "").replace(/[^a-zA-Z0-9]/g, "");
                const prevWord = (preWords[preWords.length - 1] || "").replace(/[^a-zA-Z0-9]/g, "");

                // A. Human Action Verbs ("Zayrith entered...", "Kaelith smiled...", "said Dravak")
                if (HUMAN_ACTION_VERBS.has(nextWord)) {
                    charScore += 0.55;
                    if (occ.isSentenceStart || cand.strippedPrefix) charScore += 0.10;
                    evidence.push(`Human action verb "${nextWord}"`);
                } else if (HUMAN_ACTION_VERBS.has(secondNextWord)) {
                    charScore += 0.40;
                    evidence.push(`Action verb "${secondNextWord}"`);
                }
                if (HUMAN_ACTION_VERBS.has(prevWord)) {
                    charScore += 0.45;
                    evidence.push(`Preceded by action verb "${prevWord}"`);
                }

                // B. Collective / Entity Verbs ("The Veylora awakened", "The Eldorian Council gathered")
                if (COLLECTIVE_ENTITY_VERBS.has(nextWord)) {
                    worldScore += 0.45;
                    charScore += 0.35; // competing signal
                    evidence.push(`Collective verb "${nextWord}"`);
                }

                // C. Personal Preposition Context ("smiled at him", "looked at Zayrith", "said to Kaelith")
                if (/\b(?:at|to|with|saw|towards|beside|facing)\s*$/i.test(preLower) && (HUMAN_ACTION_VERBS.has(prevWord) || preLower.includes("smiled") || preLower.includes("looked"))) {
                    charScore += 0.40;
                    evidence.push("Personal preposition context");
                }

                // D. Pronoun Continuity ("Zayrith entered... He looked around...")
                if (occ.nextSentence) {
                    const firstNextWord = occ.nextSentence.trim().split(/\s+/)[0].toLowerCase().replace(/[^a-z]/g, "");
                    if (["he", "she", "his", "her", "him", "they"].includes(firstNextWord)) {
                        charScore += 0.35;
                        evidence.push(`Pronoun continuity ("${firstNextWord}")`);
                    }
                }

                // E. Location Context & Prepositions ("in Zayra", "arrived in Veyra", "returned to Elarion", "was a city")
                if (LOCATION_PREPOSITIONS.has(prevWord)) {
                    locScore += 0.55;
                    evidence.push(`Spatial preposition "${prevWord}"`);
                }

                if (/\b(?:arrived\s+in|returned\s+to|travelled\s+to|heading\s+to|city\s+of|kingdom\s+of|forest\s+of|mountains\s+of|located\s+in|near|was\s+a\s+city|was\s+a\s+town|was\s+a\s+kingdom)\s*$/i.test(preLower) || /\b(?:was\s+a\s+(?:city|town|village|kingdom|country|palace|castle|fortress|place|realm|location)|is\s+a\s+(?:city|town|village|kingdom|country|palace|castle|fortress|place|realm|location))\b/i.test(postLower)) {
                    locScore += 0.55;
                    evidence.push("Location context phrase");
                }

                // F. Event Occurrence Verbs ("The Great Betrayal occurred", "The Palace Explosion changed...")
                if (EVENT_ACTION_VERBS.has(nextWord)) {
                    timelineScore += 0.55;
                    evidence.push(`Event verb "${nextWord}"`);
                } else if (EVENT_ACTION_VERBS.has(secondNextWord)) {
                    timelineScore += 0.40;
                    evidence.push(`Event action verb "${secondNextWord}"`);
                }
            });

            // Context Signal 7: Name Token Structure (Fictional Single or Multi-word capitalization)
            if (isMultiWord) {
                charScore += 0.25;
            } else {
                charScore += 0.25;
            }

            // Context Signal 8: Mention Frequency
            if (occurrencesCount >= 2) {
                charScore += 0.10;
                locScore += 0.05;
                worldScore += 0.05;
                evidence.push(`Repeated reference (${occurrencesCount}x)`);
            }

            // Normalize Scores
            charScore = parseFloat(Math.min(0.98, charScore).toFixed(2));
            locScore = parseFloat(Math.min(0.98, locScore).toFixed(2));
            worldScore = parseFloat(Math.min(0.98, worldScore).toFixed(2));
            timelineScore = parseFloat(Math.min(0.98, timelineScore).toFixed(2));

            // =========================================================================
            // PIPELINE STAGE 4: COMPETITIVE DECISION & AMBIGUITY EVALUATION
            // =========================================================================
            const scoresMap = [
                { type: "character", score: charScore, label: "Character" },
                { type: "location", score: locScore, label: "Location" },
                { type: "world", score: worldScore, label: "World Entity" },
                { type: "timeline", score: timelineScore, label: "Timeline Event" }
            ];

            scoresMap.sort((a, b) => b.score - a.score);

            const winner = scoresMap[0];
            const runnerUp = scoresMap[1];

            const uniqueEvidence = Array.from(new Set(evidence)).join(", ");

            // AMBIGUITY DECISION RULE:
            // Candidates are AMBIGUOUS if:
            // 1. Two top categories have competing non-zero scores with small delta (|winner - runnerUp| <= 0.18) AND runnerUp score >= 0.35.
            // 2. OR top score is moderate (0.50 - 0.70) AND runnerUp score >= 0.25.
            const isAmbiguous = (winner.score >= 0.50 && runnerUp.score >= 0.35 && (winner.score - runnerUp.score <= 0.18)) ||
                                (winner.score >= 0.50 && winner.score <= 0.70 && runnerUp.score >= 0.25);

            if (isAmbiguous) {
                ambiguousCandidates.push({
                    name: name,
                    normalizedName: normName,
                    type: "ambiguous",
                    winningType: winner.type,
                    topScores: [winner, runnerUp],
                    confidence: winner.score,
                    occurrences: occurrencesCount,
                    evidence: uniqueEvidence
                });
            } else if (winner.score >= 0.75) {
                // High / Strong Confidence Candidate (0.75+)
                highConfidenceCandidates.push({
                    name: name,
                    normalizedName: normName,
                    type: winner.type,
                    confidence: winner.score,
                    occurrences: occurrencesCount,
                    evidence: uniqueEvidence
                });
            } else if (winner.score >= 0.50) {
                // Moderate score -> ambiguous suggestion
                ambiguousCandidates.push({
                    name: name,
                    normalizedName: normName,
                    type: "ambiguous",
                    winningType: winner.type,
                    topScores: [winner, runnerUp],
                    confidence: winner.score,
                    occurrences: occurrencesCount,
                    evidence: uniqueEvidence
                });
            }
        });

        // Deduplicate & Sort candidates by confidence descending
        highConfidenceCandidates.sort((a, b) => b.confidence - a.confidence || b.occurrences - a.occurrences);
        ambiguousCandidates.sort((a, b) => b.confidence - a.confidence || b.occurrences - a.occurrences);

        return {
            existingMatches,
            candidates: highConfidenceCandidates,
            ambiguousCandidates
        };
    }

    return {
        detectEntities,
        GRAMMATICAL_WORDS,
        PRONOUNS,
        DISCOURSE_CONNECTORS,
        HUMAN_ACTION_VERBS,
        LOCATION_PREPOSITIONS,
        LOCATION_NOUNS,
        WORLD_ENTITY_KEYWORDS,
        EVENT_KEYWORDS
    };

})();
