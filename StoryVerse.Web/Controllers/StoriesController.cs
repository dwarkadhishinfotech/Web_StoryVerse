using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryVerse.Core.Entities;
using StoryVerse.Core.Entities.Identity;
using StoryVerse.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace StoryVerse.Web.Controllers
{
    [Authorize]
    public class StoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly StoryVerse.Web.Services.IDropdownService _dropdownService;
        private readonly StoryVerse.Web.Services.IActiveStoryService _activeStoryService;

        public StoriesController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            StoryVerse.Web.Services.IDropdownService dropdownService,
            StoryVerse.Web.Services.IActiveStoryService activeStoryService)
        {
            _context = context;
            _userManager = userManager;
            _dropdownService = dropdownService;
            _activeStoryService = activeStoryService;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private async Task PopulateViewBagAsync()
        {
            ViewBag.StoryTypes      = await _dropdownService.GetOptionsByCategoryAsync("StoryType");
            ViewBag.ProjectStatuses = await _dropdownService.GetOptionsByCategoryAsync("ProjectStatus");
            ViewBag.TargetAudiences = await _dropdownService.GetOptionsByCategoryAsync("TargetAudience");
            ViewBag.Languages       = await _dropdownService.GetOptionsByCategoryAsync("Language");
            ViewBag.WritingStyles   = await _dropdownService.GetOptionsByCategoryAsync("WritingStyle");
            ViewBag.PointsOfView    = await _dropdownService.GetOptionsByCategoryAsync("PointOfView");
            ViewBag.Tenses          = await _dropdownService.GetOptionsByCategoryAsync("Tense");
            // Genre master list — ordered by SortOrder, only active
            ViewBag.Genres          = await _context.Genres
                                        .Where(g => g.IsActive)
                                        .OrderBy(g => g.SortOrder)
                                        .ToListAsync();
        }

        // ── Index ─────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var stories = await _context.Stories
                .AsNoTracking()
                .AsSplitQuery()
                .Where(s => s.UserId == user.Id)
                .Include(s => s.Chapters)
                .Include(s => s.Characters)
                .Include(s => s.Locations)
                .Include(s => s.StoryGenres).ThenInclude(sg => sg.Genre)
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();

            // Calculate logged-in user initials
            string userInitials = "A";
            if (!string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(user.LastName))
            {
                userInitials = $"{user.FirstName[0]}{user.LastName[0]}".ToUpper();
            }
            else if (!string.IsNullOrWhiteSpace(user.DisplayName))
            {
                var parts = user.DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                userInitials = parts.Length > 1 
                    ? $"{parts[0][0]}{parts[1][0]}".ToUpper() 
                    : (parts.Length > 0 ? parts[0][0].ToString().ToUpper() : "A");
            }
            else if (!string.IsNullOrWhiteSpace(user.FirstName))
            {
                userInitials = user.FirstName[0].ToString().ToUpper();
            }
            else if (!string.IsNullOrWhiteSpace(user.UserName))
            {
                userInitials = user.UserName[0].ToString().ToUpper();
            }

            ViewBag.UserInitials = userInitials;

            // Fetch UserGoal for dynamic writing goals card
            UserGoal? userGoal = null;
            try
            {
                userGoal = await _context.UserGoals
                    .FirstOrDefaultAsync(g => g.UserId == user.Id);
            }
            catch
            {
                userGoal = null;
            }

            if (userGoal == null)
            {
                userGoal = new UserGoal
                {
                    UserId = user.Id,
                    DailyWordCountGoal = 1000,
                    MonthlyWordCountGoal = 50000,
                    LastUpdated = DateTime.UtcNow
                };
            }

            int totalWordsWritten = stories.Sum(s => s.CurrentWordCount);
            int monthlyGoal = userGoal.MonthlyWordCountGoal > 0 ? userGoal.MonthlyWordCountGoal : 50000;
            int goalPercentage = (int)Math.Clamp(((double)totalWordsWritten / monthlyGoal) * 100, 0, 100);
            int currentDayOfMonth = Math.Max(1, DateTime.UtcNow.Day);
            int dailyAverage = (int)Math.Round((double)totalWordsWritten / currentDayOfMonth);

            ViewBag.UserGoal = userGoal;
            ViewBag.MonthlyGoal = monthlyGoal;
            ViewBag.WordsWrittenThisMonth = totalWordsWritten;
            ViewBag.GoalPercentage = goalPercentage;
            ViewBag.DailyAverage = dailyAverage;

            return View(stories);
        }

        // ── Details ───────────────────────────────────────────────────────────

        public async Task<IActionResult> Details(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            _context.Database.SetCommandTimeout(120);

            var story = await _context.Stories
                .AsSplitQuery()
                .Include(s => s.StoryParts.OrderBy(p => p.Order))
                    .ThenInclude(p => p.Chapters.OrderBy(c => c.Order))
                .Include(s => s.Chapters.OrderBy(c => c.Order))
                .Include(s => s.Characters)
                .Include(s => s.Locations)
                .Include(s => s.TimelineEvents)
                .Include(s => s.StoryArcs)
                .Include(s => s.ResearchNotes)
                .Include(s => s.Assets)
                .Include(s => s.StoryGenres).ThenInclude(sg => sg.Genre)
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.Id);

            if (story == null) return NotFound();

            // Set active story in session
            _activeStoryService.SetActiveStoryId(HttpContext, story.Id);

            // Fetch UserGoal for dynamic writing goals
            UserGoal? userGoal = await _context.UserGoals.FirstOrDefaultAsync(g => g.UserId == user.Id);
            if (userGoal == null)
            {
                userGoal = new UserGoal
                {
                    UserId = user.Id,
                    DailyWordCountGoal = 1000,
                    WeeklyWordCountGoal = 5000,
                    MonthlyWordCountGoal = 20000,
                    WordsWrittenToday = 0,
                    WordsWrittenThisWeek = 0,
                    WordsWrittenThisMonth = 0,
                    CurrentStreakDays = 0,
                    LastUpdated = DateTime.UtcNow
                };
                _context.UserGoals.Add(userGoal);
                await _context.SaveChangesAsync();
            }

            ViewBag.UserGoal = userGoal;

            // Logged-in user information
            string userName = !string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(user.LastName)
                ? $"{user.FirstName} {user.LastName}"
                : (!string.IsNullOrWhiteSpace(user.DisplayName) ? user.DisplayName : (user.UserName ?? "Rakshesh kumar"));
            
            string userInitials = "RP";
            if (!string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(user.LastName))
            {
                userInitials = $"{user.FirstName[0]}{user.LastName[0]}".ToUpper();
            }

            ViewBag.UserName = userName;
            ViewBag.UserInitials = userInitials;

            return View(story);
        }

        // ── GET: Stories/Create ───────────────────────────────────────────────

        public async Task<IActionResult> Create()
        {
            await PopulateViewBagAsync();
            return View(new Story());
        }

        // ── POST: Stories/Create ──────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Title,TargetWordCount,Status,Tagline,Synopsis,StoryType,TargetAudience,Language,Tone,PointOfView,TimePeriod,Themes")] Story story,
            IFormFile? CoverFile,
            IFormFile? HeroBannerFile,
            List<int> SelectedGenreIds)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            ModelState.Remove("User");
            ModelState.Remove("UserId");
            ModelState.Remove("CoverImageUrl");
            ModelState.Remove("HeroBannerImageUrl");
            ModelState.Remove("Genre");

            if (string.IsNullOrWhiteSpace(story.Status))
            {
                story.Status = "Draft";
            }

            if (ModelState.IsValid)
            {
                story.Id             = Guid.NewGuid();
                story.UserId         = user.Id;
                story.CreatedAt      = DateTime.UtcNow;
                story.UpdatedAt      = DateTime.UtcNow;
                story.CurrentWordCount = 0;

#pragma warning disable CS0618 // Keep Genre string in sync for any legacy code still reading it
                if (SelectedGenreIds != null && SelectedGenreIds.Count > 0)
                {
                    var selectedGenres = await _context.Genres
                        .Where(g => SelectedGenreIds.Contains(g.Id))
                        .ToListAsync();
                    story.Genre = string.Join(", ", selectedGenres.Select(g => g.Name));
                }
#pragma warning restore CS0618

                // Handle cover image
                if (CoverFile != null && CoverFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "covers");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(CoverFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                        await CoverFile.CopyToAsync(fileStream);

                    story.CoverImageUrl = "/covers/" + uniqueFileName;
                }
                else
                {
                    story.CoverImageUrl = "/images/empty-states/live_preview_book.png";
                }

                // Handle hero banner background image if uploaded
                if (HeroBannerFile != null && HeroBannerFile.Length > 0)
                {
                    var bannersFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "banners");
                    if (!Directory.Exists(bannersFolder))
                        Directory.CreateDirectory(bannersFolder);

                    var uniqueBannerName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(HeroBannerFile.FileName);
                    var bannerPath = Path.Combine(bannersFolder, uniqueBannerName);
                    using (var bannerStream = new FileStream(bannerPath, FileMode.Create))
                        await HeroBannerFile.CopyToAsync(bannerStream);

                    story.HeroBannerImageUrl = "/banners/" + uniqueBannerName;
                }

                _context.Add(story);

                // Insert join rows — ordered by the user's selection sequence
                if (SelectedGenreIds != null)
                {
                    for (int i = 0; i < SelectedGenreIds.Count; i++)
                    {
                        _context.StoryGenres.Add(new StoryGenre
                        {
                            StoryId   = story.Id,
                            GenreId   = SelectedGenreIds[i],
                            IsPrimary = i == 0,   // first selected is primary
                            SortOrder = i,
                            AddedAt   = DateTime.UtcNow
                        });
                    }
                }

                _context.ActivityLogs.Add(new ActivityLog
                {
                    UserId            = user.Id,
                    ActionType        = "Story",
                    Description       = $"Created new story '{story.Title}'",
                    RelatedEntityName = story.Title,
                    Timestamp         = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                _activeStoryService.SetActiveStoryId(HttpContext, story.Id);
                return RedirectToAction(nameof(Index));
            }

            await PopulateViewBagAsync();
            return View(story);
        }

        // ── POST: Stories/SetGlobalStory ─────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> SetGlobalStory(Guid storyId, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var storyExists = await _context.Stories.AnyAsync(s => s.Id == storyId && s.UserId == user.Id);
            if (storyExists)
            {
                _activeStoryService.SetActiveStoryId(HttpContext, storyId);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        // ── GET: Stories/Edit/5 ───────────────────────────────────────────────

        public async Task<IActionResult> Edit(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var story = await _context.Stories
                .Include(s => s.StoryGenres)
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.Id);

            if (story == null) return NotFound();

            await PopulateViewBagAsync();

            // Pass currently selected genre IDs to the view
            ViewBag.CurrentGenreIds = story.StoryGenres.Select(sg => sg.GenreId).ToList();

            return View(story);
        }

        // ── POST: Stories/Edit/5 ──────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Guid id,
            [Bind("Id,Title,TargetWordCount,Status,CoverImageUrl,HeroBannerImageUrl,CurrentWordCount,CreatedAt,Tagline,Synopsis,PointOfView,TimePeriod,Language,TargetAudience,Themes,Tone,StoryType")] Story story,
            List<int> SelectedGenreIds,
            IFormFile? CoverFile,
            IFormFile? HeroBannerFile,
            List<string>? Themes)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (id != story.Id) return NotFound();

            var existingStory = await _context.Stories
                .Include(s => s.StoryGenres)
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.Id);

            if (existingStory == null) return NotFound();

            ModelState.Remove("User");
            ModelState.Remove("UserId");
            ModelState.Remove("CoverImageUrl");
            ModelState.Remove("HeroBannerImageUrl");
            ModelState.Remove("Genre");

            if (ModelState.IsValid)
            {
                existingStory.Title           = story.Title;
                existingStory.TargetWordCount = story.TargetWordCount;
                existingStory.Status          = story.Status;
                existingStory.CurrentWordCount = story.CurrentWordCount;
                existingStory.Tagline         = story.Tagline;
                existingStory.Synopsis        = story.Synopsis;
                existingStory.PointOfView     = story.PointOfView;
                existingStory.TimePeriod      = story.TimePeriod;
                existingStory.Language        = story.Language;
                existingStory.TargetAudience  = story.TargetAudience;
                existingStory.Tone            = story.Tone;
                existingStory.StoryType       = story.StoryType ?? existingStory.StoryType;
                existingStory.UpdatedAt       = DateTime.UtcNow;

                if (Themes != null && Themes.Count > 0)
                {
                    existingStory.Themes = string.Join(", ", Themes);
                }
                else if (!string.IsNullOrWhiteSpace(story.Themes))
                {
                    existingStory.Themes = story.Themes;
                }

                // Handle cover file upload if provided
                if (CoverFile != null && CoverFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "covers");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(CoverFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                        await CoverFile.CopyToAsync(fileStream);

                    existingStory.CoverImageUrl = "/covers/" + uniqueFileName;
                }
                else if (!string.IsNullOrWhiteSpace(story.CoverImageUrl))
                {
                    existingStory.CoverImageUrl = story.CoverImageUrl;
                }

                // Handle hero banner background image upload if provided
                if (HeroBannerFile != null && HeroBannerFile.Length > 0)
                {
                    var bannersFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "banners");
                    if (!Directory.Exists(bannersFolder))
                        Directory.CreateDirectory(bannersFolder);

                    var uniqueBannerName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(HeroBannerFile.FileName);
                    var bannerPath = Path.Combine(bannersFolder, uniqueBannerName);
                    using (var bannerStream = new FileStream(bannerPath, FileMode.Create))
                        await HeroBannerFile.CopyToAsync(bannerStream);

                    existingStory.HeroBannerImageUrl = "/banners/" + uniqueBannerName;
                }
                else if (story.HeroBannerImageUrl != null)
                {
                    existingStory.HeroBannerImageUrl = story.HeroBannerImageUrl;
                }

                // Remove old genre associations and re-add
                _context.StoryGenres.RemoveRange(existingStory.StoryGenres);

                if (SelectedGenreIds != null)
                {
                    for (int i = 0; i < SelectedGenreIds.Count; i++)
                    {
                        _context.StoryGenres.Add(new StoryGenre
                        {
                            StoryId   = existingStory.Id,
                            GenreId   = SelectedGenreIds[i],
                            IsPrimary = i == 0,
                            SortOrder = i,
                            AddedAt   = DateTime.UtcNow
                        });
                    }
                }

#pragma warning disable CS0618
                var selectedGenres = SelectedGenreIds != null
                    ? await _context.Genres.Where(g => SelectedGenreIds.Contains(g.Id)).ToListAsync()
                    : new List<Genre>();
                existingStory.Genre = string.Join(", ", selectedGenres.Select(g => g.Name));
#pragma warning restore CS0618

                _context.ActivityLogs.Add(new ActivityLog
                {
                    UserId            = user.Id,
                    ActionType        = "Story",
                    Description       = $"Updated story '{existingStory.Title}'",
                    RelatedEntityName = existingStory.Title,
                    Timestamp         = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateViewBagAsync();
            ViewBag.CurrentGenreIds = SelectedGenreIds ?? new List<int>();
            return View(story);
        }

        // ── POST: Stories/Delete/5 ────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var story = await _context.Stories.FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.Id);
            if (story == null) return NotFound();

            _context.Stories.Remove(story);
            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId            = user.Id,
                ActionType        = "Story",
                Description       = $"Deleted story '{story.Title}'",
                RelatedEntityName = story.Title,
                Timestamp         = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ── POST: Stories/UpdateGoal ──────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGoal(int monthlyWordCountGoal, int dailyWordCountGoal)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            try
            {
                var userGoal = await _context.UserGoals.FirstOrDefaultAsync(g => g.UserId == user.Id);
                if (userGoal == null)
                {
                    userGoal = new UserGoal
                    {
                        UserId = user.Id,
                        MonthlyWordCountGoal = monthlyWordCountGoal > 0 ? monthlyWordCountGoal : 50000,
                        DailyWordCountGoal = dailyWordCountGoal > 0 ? dailyWordCountGoal : 1000,
                        LastUpdated = DateTime.UtcNow
                    };
                    _context.UserGoals.Add(userGoal);
                }
                else
                {
                    if (monthlyWordCountGoal > 0) userGoal.MonthlyWordCountGoal = monthlyWordCountGoal;
                    if (dailyWordCountGoal > 0) userGoal.DailyWordCountGoal = dailyWordCountGoal;
                    userGoal.LastUpdated = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Fallback log error
            }

            return RedirectToAction(nameof(Index));
        }

        // ── GET: Stories/CheckTitleAvailability ──────────────────────────────
        [HttpGet]
        public async Task<IActionResult> CheckTitleAvailability(string title, Guid? currentStoryId = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { isAvailable = true, suggestions = new string[0] });

            if (string.IsNullOrWhiteSpace(title))
            {
                return Json(new { isAvailable = true, suggestions = new string[0] });
            }

            var trimmedTitle = title.Trim();
            bool exists = await _context.Stories
                .AsNoTracking()
                .AnyAsync(s => s.UserId == user.Id &&
                               (currentStoryId == null || s.Id != currentStoryId.Value) &&
                               s.Title.ToLower() == trimmedTitle.ToLower());

            if (!exists)
            {
                return Json(new { isAvailable = true, suggestions = new string[0] });
            }

            var rawSuggestions = new List<string>
            {
                $"{trimmedTitle}: Volume 1",
                $"The Chronicles of {trimmedTitle}",
                $"{trimmedTitle} II",
                $"Tales of {trimmedTitle}",
                $"{trimmedTitle} (2026)",
                $"{trimmedTitle}: Part I"
            };

            var existingTitles = await _context.Stories
                .AsNoTracking()
                .Where(s => s.UserId == user.Id)
                .Select(s => s.Title.ToLower())
                .ToListAsync();

            var availableSuggestions = rawSuggestions
                .Where(s => !existingTitles.Contains(s.ToLower()))
                .Take(4)
                .ToList();

            return Json(new {
                isAvailable = false,
                message = "Name is not available",
                suggestions = availableSuggestions
            });
        }
    }
}
