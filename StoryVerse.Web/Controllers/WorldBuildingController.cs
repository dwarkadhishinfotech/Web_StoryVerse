using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryVerse.Core.Entities.Identity;
using StoryVerse.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace StoryVerse.Web.Controllers
{
    [Authorize]
    public class WorldBuildingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WorldBuildingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: WorldBuilding?storyId=...
        public async Task<IActionResult> Index(Guid? storyId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (storyId.HasValue && storyId.Value != Guid.Empty)
            {
                var story = await _context.Stories
                    .Include(s => s.Locations)
                    .FirstOrDefaultAsync(s => s.Id == storyId.Value && s.UserId == user.Id);

                if (story == null) return NotFound();
                ViewBag.Story = story;
                return View(story.Locations);
            }
            else
            {
                ViewBag.Story = null;
                // Just pass empty list or all locations if that exists. For now, empty list is safer if Locations is bound to a story.
                // Or fetch all locations for user.
                var allLocations = await _context.Locations
                    .Include(l => l.Story)
                    .Where(l => l.Story.UserId == user.Id)
                    .ToListAsync();
                return View(allLocations);
            }
        }
    }
}
