using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryVerse.Core.Entities;
using StoryVerse.Core.Entities.Identity;
using StoryVerse.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StoryVerse.Web.Controllers
{
    [Authorize]
    public class ChaptersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChaptersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Chapters?storyId=...
        public async Task<IActionResult> Index(Guid? storyId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (storyId.HasValue && storyId.Value != Guid.Empty)
            {
                var story = await _context.Stories
                    .Include(s => s.Chapters)
                    .FirstOrDefaultAsync(s => s.Id == storyId.Value && s.UserId == user.Id);

                if (story == null) return NotFound();

                ViewBag.Story = story;
                var chapters = story.Chapters.OrderBy(c => c.Order).ToList();
                return View(chapters);
            }
            else
            {
                ViewBag.Story = null;
                var allChapters = await _context.Chapters
                    .Include(c => c.Story)
                    .Where(c => c.Story.UserId == user.Id)
                    .OrderBy(c => c.Story.Title)
                    .ThenBy(c => c.Order)
                    .ToListAsync();
                return View(allChapters);
            }
        }

        // GET: Chapters/Editor?storyId=...&chapterId=...
        public async Task<IActionResult> Editor(Guid storyId, Guid chapterId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var story = await _context.Stories
                .FirstOrDefaultAsync(s => s.Id == storyId && s.UserId == user.Id);

            if (story == null) return NotFound();

            var chapter = await _context.Chapters
                .FirstOrDefaultAsync(c => c.Id == chapterId && c.StoryId == storyId);

            if (chapter == null) return NotFound();

            ViewBag.Story = story;
            return View(chapter);
        }

        // POST: Chapters/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid storyId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var story = await _context.Stories
                .Include(s => s.Chapters)
                .FirstOrDefaultAsync(s => s.Id == storyId && s.UserId == user.Id);

            if (story == null) return NotFound();

            var nextNo = story.Chapters.Any() ? story.Chapters.Max(c => c.Order) + 1 : 1;

            var newChapter = new Chapter
            {
                Id = Guid.NewGuid(),
                StoryId = storyId,
                Title = $"Chapter {nextNo}",
                Order = nextNo,
                WordCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Chapters.Add(newChapter);

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ActionType = "Chapter",
                Description = $"Created Chapter {nextNo} for story: {story.Title}",
                RelatedEntityName = newChapter.Title,
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Editor), new { storyId = storyId, chapterId = newChapter.Id });
        }

        // POST: Chapters/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var chapter = await _context.Chapters
                .Include(c => c.Story)
                .FirstOrDefaultAsync(c => c.Id == id && c.Story.UserId == user.Id);

            if (chapter == null) return NotFound();

            var storyId = chapter.StoryId;
            _context.Chapters.Remove(chapter);

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ActionType = "Chapter",
                Description = $"Deleted chapter: {chapter.Title}",
                RelatedEntityName = chapter.Title,
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { storyId = storyId });
        }
    }
}

