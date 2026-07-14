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
    public class TimelineController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TimelineController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Timeline?storyId=...
        public async Task<IActionResult> Index(Guid? storyId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (storyId.HasValue && storyId.Value != Guid.Empty)
            {
                var story = await _context.Stories
                    .FirstOrDefaultAsync(s => s.Id == storyId.Value && s.UserId == user.Id);

                if (story == null) return NotFound();
                ViewBag.Story = story;
            }
            else
            {
                ViewBag.Story = null;
            }

            return View();
        }
    }
}
