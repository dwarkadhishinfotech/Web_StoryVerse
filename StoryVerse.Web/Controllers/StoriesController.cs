using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryVerse.Core.Entities;
using StoryVerse.Core.Entities.Identity;
using StoryVerse.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;

namespace StoryVerse.Web.Controllers
{
    [Authorize]
    public class StoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StoriesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var stories = await _context.Stories
                .Where(s => s.UserId == user.Id)
                .Include(s => s.Chapters)
                .Include(s => s.Characters)
                .Include(s => s.Locations)
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();

            return View(stories);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var story = await _context.Stories
                .Include(s => s.Chapters)
                .Include(s => s.Characters)
                .Include(s => s.Locations)
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.Id);

            if (story == null) return NotFound();

            return View(story);
        }

        // GET: Stories/Create
        public IActionResult Create()
        {
            return View(new Story());
        }

        // POST: Stories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Genre,TargetWordCount,Status,CoverImageUrl")] Story story)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (ModelState.IsValid)
            {
                story.Id = Guid.NewGuid();
                story.UserId = user.Id;
                story.CreatedAt = System.DateTime.UtcNow;
                story.UpdatedAt = System.DateTime.UtcNow;
                story.CurrentWordCount = 0;

                _context.Add(story);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(story);
        }

        // GET: Stories/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var story = await _context.Stories.FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.Id);
            if (story == null) return NotFound();

            return View(story);
        }

        // POST: Stories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Title,Genre,TargetWordCount,Status,CoverImageUrl,CurrentWordCount,CreatedAt")] Story story)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (id != story.Id) return NotFound();

            var existingStory = await _context.Stories.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.Id);
            if (existingStory == null) return NotFound();

            if (ModelState.IsValid)
            {
                story.UserId = user.Id;
                story.UpdatedAt = System.DateTime.UtcNow;

                _context.Update(story);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(story);
        }

        // POST: Stories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var story = await _context.Stories.FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.Id);
            if (story == null) return NotFound();

            _context.Stories.Remove(story);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
