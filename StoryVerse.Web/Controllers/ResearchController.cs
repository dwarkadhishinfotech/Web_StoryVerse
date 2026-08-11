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
    public class ResearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly StoryVerse.Web.Services.IActiveStoryService _activeStoryService;

        public ResearchController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            StoryVerse.Web.Services.IActiveStoryService activeStoryService)
        {
            _context = context;
            _userManager = userManager;
            _activeStoryService = activeStoryService;
        }

        // GET: Research?storyId=...
        public async Task<IActionResult> Index(Guid? storyId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var activeStoryIdGuid = await _activeStoryService.GetActiveStoryIdAsync(HttpContext, user.Id, storyId);

            if (activeStoryIdGuid.HasValue && activeStoryIdGuid.Value != Guid.Empty)
            {
                var story = await _context.Stories
                    .FirstOrDefaultAsync(s => s.Id == activeStoryIdGuid.Value && s.UserId == user.Id);

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
