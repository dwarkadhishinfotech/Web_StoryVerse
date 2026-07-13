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
    public class CharactersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CharactersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Characters?storyId=...
        public async Task<IActionResult> Index(Guid storyId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var story = await _context.Stories
                .Include(s => s.Characters)
                .FirstOrDefaultAsync(s => s.Id == storyId && s.UserId == user.Id);

            if (story == null) return NotFound();

            ViewBag.Story = story;
            var characters = story.Characters.ToList();
            return View(characters);
        }

        // GET: Characters/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var character = await _context.Characters
                .Include(c => c.Story)
                .FirstOrDefaultAsync(c => c.Id == id && c.Story.UserId == user.Id);

            if (character == null) return NotFound();

            ViewBag.Story = character.Story;
            return View(character);
        }

        // GET: Characters/Create?storyId=...
        public async Task<IActionResult> Create(Guid storyId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var story = await _context.Stories
                .FirstOrDefaultAsync(s => s.Id == storyId && s.UserId == user.Id);

            if (story == null) return NotFound();

            ViewBag.Story = story;
            return View(new Character { StoryId = storyId });
        }

        // POST: Characters/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StoryId,Name,Role")] Character character)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var story = await _context.Stories
                .FirstOrDefaultAsync(s => s.Id == character.StoryId && s.UserId == user.Id);

            if (story == null) return NotFound();

            if (ModelState.IsValid)
            {
                character.Id = Guid.NewGuid();
                character.CreatedAt = DateTime.UtcNow;

                _context.Characters.Add(character);

                // Log activity
                _context.ActivityLogs.Add(new ActivityLog
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    ActionType = "Character",
                    Description = $"Added Character: {character.Name} to story: {story.Title}",
                    RelatedEntityName = character.Name,
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { storyId = character.StoryId });
            }

            ViewBag.Story = story;
            return View(character);
        }

        // GET: Characters/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var character = await _context.Characters
                .Include(c => c.Story)
                .FirstOrDefaultAsync(c => c.Id == id && c.Story.UserId == user.Id);

            if (character == null) return NotFound();

            ViewBag.Story = character.Story;
            return View(character);
        }

        // POST: Characters/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,StoryId,Name,Role,CreatedAt")] Character character)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (id != character.Id) return NotFound();

            var story = await _context.Stories
                .FirstOrDefaultAsync(s => s.Id == character.StoryId && s.UserId == user.Id);

            if (story == null) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(character);

                // Log activity
                _context.ActivityLogs.Add(new ActivityLog
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    ActionType = "Character",
                    Description = $"Updated character info: {character.Name}",
                    RelatedEntityName = character.Name,
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { storyId = character.StoryId });
            }

            ViewBag.Story = story;
            return View(character);
        }

        // POST: Characters/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var character = await _context.Characters
                .Include(c => c.Story)
                .FirstOrDefaultAsync(c => c.Id == id && c.Story.UserId == user.Id);

            if (character == null) return NotFound();

            var storyId = character.StoryId;
            _context.Characters.Remove(character);

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ActionType = "Character",
                Description = $"Removed character: {character.Name}",
                RelatedEntityName = character.Name,
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { storyId = storyId });
        }
    }
}
