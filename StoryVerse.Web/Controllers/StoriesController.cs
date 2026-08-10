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

        public StoriesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, StoryVerse.Web.Services.IDropdownService dropdownService)
        {
            _context = context;
            _userManager = userManager;
            _dropdownService = dropdownService;
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
                .Where(s => s.UserId == user.Id)
                .Include(s => s.Chapters)
                .Include(s => s.Characters)
                .Include(s => s.Locations)
                .Include(s => s.StoryGenres).ThenInclude(sg => sg.Genre)
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();

            return View(stories);
        }

        // ── Details ───────────────────────────────────────────────────────────

        public async Task<IActionResult> Details(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var story = await _context.Stories
                .Include(s => s.Chapters)
                .Include(s => s.Characters)
                .Include(s => s.Locations)
                .Include(s => s.StoryGenres).ThenInclude(sg => sg.Genre)
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.Id);

            if (story == null) return NotFound();

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
            [Bind("Title,TargetWordCount,Status")] Story story,
            IFormFile CoverFile,
            List<int> SelectedGenreIds)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            ModelState.Remove("User");
            ModelState.Remove("UserId");
            ModelState.Remove("CoverImageUrl");
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
                return RedirectToAction(nameof(Index));
            }

            await PopulateViewBagAsync();
            return View(story);
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
            [Bind("Id,Title,TargetWordCount,Status,CoverImageUrl,CurrentWordCount,CreatedAt")] Story story,
            List<int> SelectedGenreIds)
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
            ModelState.Remove("Genre");

            if (ModelState.IsValid)
            {
                existingStory.Title           = story.Title;
                existingStory.TargetWordCount = story.TargetWordCount;
                existingStory.Status          = story.Status;
                existingStory.CoverImageUrl   = story.CoverImageUrl;
                existingStory.CurrentWordCount = story.CurrentWordCount;
                existingStory.UpdatedAt       = DateTime.UtcNow;

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
    }
}
