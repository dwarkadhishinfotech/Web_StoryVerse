using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryVerse.Core.Entities;
using StoryVerse.Core.Entities.Identity;
using StoryVerse.Infrastructure.Data;
using StoryVerse.Web.Models;
using StoryVerse.Web.Services;
using System.Linq;
using System.Threading.Tasks;

namespace StoryVerse.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IQuoteService _quoteService;

    public DashboardController(
        ApplicationDbContext context, 
        UserManager<ApplicationUser> userManager,
        IQuoteService quoteService)
    {
        _context = context;
        _userManager = userManager;
        _quoteService = quoteService;
    }

    [HttpGet("/dashboard")]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        var allStories = await _context.Stories
            .Where(s => s.UserId == user.Id)
            .Include(s => s.Chapters)
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync();

        var activeStory = allStories.FirstOrDefault(s => s.Status == "InProgress") ?? allStories.FirstOrDefault();

        var recentActivities = await _context.ActivityLogs
            .Where(a => a.UserId == user.Id)
            .OrderByDescending(a => a.Timestamp)
            .Take(5)
            .ToListAsync();

        var userGoal = await _context.UserGoals
            .FirstOrDefaultAsync(g => g.UserId == user.Id) ?? new UserGoal { UserId = user.Id };

        var totalWords = allStories.Sum(s => s.CurrentWordCount);
        var activeStoriesCount = allStories.Count(s => s.Status == "InProgress");
        
        var charactersCount = await _context.Characters.CountAsync(c => c.Story.UserId == user.Id);
        var locationsCount = await _context.Locations.CountAsync(l => l.Story.UserId == user.Id);

        var viewModel = new DashboardViewModel
        {
            ActiveStory = activeStory,
            RecentStories = allStories,
            RecentActivities = recentActivities,
            TotalWords = totalWords,
            ActiveStoriesCount = activeStoriesCount,
            CharactersCount = charactersCount,
            LocationsCount = locationsCount,
            UserGoal = userGoal,
            InspirationQuote = _quoteService.GetDailyQuote()
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult GetRandomQuote([FromQuery] string? currentContent = null)
    {
        var quote = _quoteService.GetRandomQuote(currentContent);
        return Json(new { content = quote.Content, author = quote.Author });
    }
}
