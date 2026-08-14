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
    private readonly StoryVerse.Web.Services.IActiveStoryService _activeStoryService;

    public DashboardController(
        ApplicationDbContext context, 
        UserManager<ApplicationUser> userManager,
        IQuoteService quoteService,
        StoryVerse.Web.Services.IActiveStoryService activeStoryService)
    {
        _context = context;
        _userManager = userManager;
        _quoteService = quoteService;
        _activeStoryService = activeStoryService;
    }

    [HttpGet("/dashboard")]
    public async Task<IActionResult> Index(Guid? storyId = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        var allStories = await _context.Stories
            .AsNoTracking()
            .Where(s => s.UserId == user.Id)
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync();

        var activeStoryIdGuid = await _activeStoryService.GetActiveStoryIdAsync(HttpContext, user.Id, storyId);
        var activeStory = activeStoryIdGuid.HasValue 
            ? allStories.FirstOrDefault(s => s.Id == activeStoryIdGuid.Value) 
            : (allStories.FirstOrDefault(s => s.Status == "InProgress") ?? allStories.FirstOrDefault());

        var recentActivities = await _context.ActivityLogs
            .AsNoTracking()
            .Where(a => a.UserId == user.Id)
            .OrderByDescending(a => a.Timestamp)
            .Take(5)
            .ToListAsync();

        UserGoal userGoal;
        try
        {
            userGoal = await _context.UserGoals
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.UserId == user.Id) ?? new UserGoal { UserId = user.Id };

            if (userGoal != null)
            {
                var todayUtc = DateTime.UtcNow.Date;
                var yesterdayUtc = todayUtc.AddDays(-1);

                if (userGoal.LastUpdated.Date != todayUtc)
                {
                    userGoal.WordsWrittenToday = 0;
                }
                if (userGoal.LastUpdated.Date < yesterdayUtc)
                {
                    userGoal.CurrentStreakDays = 0;
                }
            }
        }
        catch
        {
            userGoal = new UserGoal { UserId = user.Id, MonthlyWordCountGoal = 50000 };
        }

        var totalWords = allStories.Sum(s => s.CurrentWordCount);
        var activeStoriesCount = allStories.Count(s => string.IsNullOrEmpty(s.Status) || s.Status != "Archived");
        
        var charactersCount = activeStory != null 
            ? await _context.Characters.AsNoTracking().CountAsync(c => c.StoryId == activeStory.Id) 
            : await _context.Characters.AsNoTracking().CountAsync(c => c.Story != null && c.Story.UserId == user.Id);

        var locationsCount = activeStory != null 
            ? await _context.Locations.AsNoTracking().CountAsync(l => l.StoryId == activeStory.Id) 
            : await _context.Locations.AsNoTracking().CountAsync(l => l.Story != null && l.Story.UserId == user.Id);

        var totalChapters = await _context.Chapters
            .AsNoTracking()
            .CountAsync(c => c.Story != null && c.Story.UserId == user.Id);

        var completedChaptersCount = await _context.Chapters
            .AsNoTracking()
            .CountAsync(c => c.Story != null && c.Story.UserId == user.Id && (c.Status == "Completed" || c.WordCount > 0));

        var viewModel = new DashboardViewModel
        {
            ActiveStory = activeStory,
            RecentStories = allStories,
            RecentActivities = recentActivities,
            TotalWords = totalWords,
            ActiveStoriesCount = activeStoriesCount,
            CharactersCount = charactersCount,
            LocationsCount = locationsCount,
            TotalChapters = totalChapters,
            CompletedChaptersCount = completedChaptersCount,
            UserGoal = userGoal,
            InspirationQuote = _quoteService.GetDailyQuote()
        };

        return View(viewModel);
    }

    [HttpGet("/dashboard/activity")]
    public async Task<IActionResult> Activity(string? type = null, Guid? storyId = null, string? search = null, string? dateRange = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        var allUserStories = await _context.Stories
            .AsNoTracking()
            .Where(s => s.UserId == user.Id)
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync();

        var query = _context.ActivityLogs
            .AsNoTracking()
            .Where(a => a.UserId == user.Id);

        DateTime? startDate = null;
        DateTime? endDate = null;

        if (!string.IsNullOrWhiteSpace(dateRange))
        {
            var parts = dateRange.Split(new[] { " to ", ",", " - " }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 1 && DateTime.TryParse(parts[0].Trim(), out var pStart))
            {
                startDate = pStart.Date;
                query = query.Where(a => a.Timestamp >= startDate.Value);
            }
            if (parts.Length >= 2 && DateTime.TryParse(parts[1].Trim(), out var pEnd))
            {
                endDate = pEnd.Date.AddDays(1).AddTicks(-1);
                query = query.Where(a => a.Timestamp <= endDate.Value);
            }
        }

        if (storyId.HasValue)
        {
            var selectedStory = allUserStories.FirstOrDefault(s => s.Id == storyId.Value);
            if (selectedStory != null)
            {
                var title = selectedStory.Title;
                query = query.Where(a => a.RelatedEntityName.Contains(title) || a.Description.Contains(title));
            }
        }

        if (!string.IsNullOrWhiteSpace(type) && !type.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(a => a.ActionType.Contains(type) || a.Description.Contains(type));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(a => a.Description.ToLower().Contains(searchLower) || 
                                     a.RelatedEntityName.ToLower().Contains(searchLower) || 
                                     a.ActionType.ToLower().Contains(searchLower));
        }

        var allUserActivities = await _context.ActivityLogs
            .AsNoTracking()
            .Where(a => a.UserId == user.Id)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

        var filteredActivities = await query
            .OrderByDescending(a => a.Timestamp)
            .Take(200)
            .ToListAsync();

        // Calculate real DB metrics for the summary side-card
        var userStoryIds = allUserStories.Select(s => s.Id).ToList();

        var totalActivitiesCount = allUserActivities.Count;
        var chaptersUpdatedCount = await _context.Chapters.AsNoTracking().CountAsync(c => userStoryIds.Contains(c.StoryId));
        var charactersAddedCount = await _context.Characters.AsNoTracking().CountAsync(c => userStoryIds.Contains(c.StoryId));
        var worldEntitiesCount = await _context.WorldEntities.AsNoTracking().CountAsync(w => userStoryIds.Contains(w.StoryId))
                                + await _context.Locations.AsNoTracking().CountAsync(l => userStoryIds.Contains(l.StoryId));
        var timelineEventsCount = await _context.TimelineEvents.AsNoTracking().CountAsync(t => userStoryIds.Contains(t.StoryId));
        var notesResearchCount = await _context.ResearchNotes.AsNoTracking().CountAsync(n => userStoryIds.Contains(n.StoryId));

        // Count per-story activities for recent stories side-card
        var storyActivityCounts = new Dictionary<Guid, int>();
        foreach (var story in allUserStories)
        {
            var count = allUserActivities.Count(a => (!string.IsNullOrEmpty(a.RelatedEntityName) && a.RelatedEntityName.Contains(story.Title)) || (!string.IsNullOrEmpty(a.Description) && a.Description.Contains(story.Title)));
            storyActivityCounts[story.Id] = count;
        }

        ViewBag.UserStories = allUserStories;
        ViewBag.StoryActivityCounts = storyActivityCounts;
        ViewBag.CurrentTypeFilter = type ?? "all";
        ViewBag.CurrentStoryId = storyId;
        ViewBag.SearchQuery = search ?? "";
        ViewBag.CurrentDateRange = dateRange;
        ViewBag.StartDate = startDate;
        ViewBag.EndDate = endDate;
        ViewBag.TotalActivitiesCount = totalActivitiesCount;
        ViewBag.ChaptersUpdatedCount = chaptersUpdatedCount;
        ViewBag.CharactersAddedCount = charactersAddedCount;
        ViewBag.WorldEntitiesCount = worldEntitiesCount;
        ViewBag.TimelineEventsCount = timelineEventsCount;
        ViewBag.NotesResearchCount = notesResearchCount;

        return View(filteredActivities);
    }

    [HttpGet]
    public IActionResult GetRandomQuote([FromQuery] string? currentContent = null)
    {
        var quote = _quoteService.GetRandomQuote(currentContent);
        return Json(new { content = quote.Content, author = quote.Author });
    }
}
