using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryVerse.Core.Entities;
using StoryVerse.Core.Entities.Identity;
using StoryVerse.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoryVerse.Web.Controllers
{
    [Authorize]
    public class ChaptersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly StoryVerse.Web.Services.IActiveStoryService _activeStoryService;

        public ChaptersController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            StoryVerse.Web.Services.IActiveStoryService activeStoryService)
        {
            _context = context;
            _userManager = userManager;
            _activeStoryService = activeStoryService;
        }

        // GET: Chapters?storyId=...
        public async Task<IActionResult> Index(Guid? storyId)
        {
            await StoryVerse.Infrastructure.Data.DbSeeder.EnsureDatabaseSchemaAsync(_context);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var userStories = await _activeStoryService.GetUserStoriesAsync(user.Id);
            ViewBag.Stories = userStories;

            var activeStoryIdGuid = await _activeStoryService.GetActiveStoryIdAsync(HttpContext, user.Id, storyId);
            
            Story? selectedStory = activeStoryIdGuid.HasValue
                ? await _context.Stories
                    .AsNoTracking()
                    .Include(s => s.Chapters)
                    .Include(s => s.StoryParts)
                    .Include(s => s.Characters)
                    .Include(s => s.Locations)
                    .Include(s => s.StoryGenres)
                        .ThenInclude(sg => sg.Genre)
                    .FirstOrDefaultAsync(s => s.Id == activeStoryIdGuid.Value && s.UserId == user.Id)
                : null;

            if (selectedStory == null && userStories.Any())
            {
                selectedStory = await _context.Stories
                    .AsNoTracking()
                    .Include(s => s.Chapters)
                    .Include(s => s.StoryParts)
                    .Include(s => s.Characters)
                    .Include(s => s.Locations)
                    .Include(s => s.StoryGenres)
                        .ThenInclude(sg => sg.Genre)
                    .FirstOrDefaultAsync(s => s.Id == userStories.First().Id && s.UserId == user.Id);
            }

            ViewBag.Story = selectedStory;

            var userGoal = await _context.UserGoals.FirstOrDefaultAsync(g => g.UserId == user.Id);
            ViewBag.WritingStreak = userGoal?.CurrentStreakDays ?? 0;

            if (selectedStory != null)
            {
                var chapters = selectedStory.Chapters.OrderBy(c => c.Order).ToList();
                var storyParts = selectedStory.StoryParts.OrderBy(p => p.Order).ToList();

                // Compute Real-time Dashboard Metrics
                var totalChapters = chapters.Count;
                var completedCount = chapters.Count(c => c.Status == "Completed");
                var inProgressCount = chapters.Count(c => c.Status == "InProgress");
                var outliningCount = chapters.Count(c => c.Status == "Outlining");
                var plannedCount = chapters.Count(c => c.Status == "Planned");
                var onHoldCount = chapters.Count(c => c.Status == "OnHold");
                var archivedCount = chapters.Count(c => c.Status == "Archived");

                var totalWords = chapters.Sum(c => c.WordCount);
                var targetWords = selectedStory.TargetWordCount > 0 ? selectedStory.TargetWordCount : (userGoal?.DailyWordCountGoal > 0 ? userGoal.DailyWordCountGoal * 30 : 0);
                var progressPercent = targetWords > 0 ? (int)Math.Clamp(((double)totalWords / targetWords) * 100, 0, 100) : 0;
                var avgWordsPerChapter = totalChapters > 0 ? (int)Math.Round((double)totalWords / totalChapters) : 0;

                // Currently Writing Chapter (InProgress or latest updated)
                var currentlyWritingChapter = chapters.FirstOrDefault(c => c.Status == "InProgress") 
                    ?? chapters.OrderByDescending(c => c.UpdatedAt).FirstOrDefault();

                ViewBag.TotalChapters = totalChapters;
                ViewBag.CompletedCount = completedCount;
                ViewBag.InProgressCount = inProgressCount;
                ViewBag.OutliningCount = outliningCount;
                ViewBag.PlannedCount = plannedCount;
                ViewBag.OnHoldCount = onHoldCount;
                ViewBag.ArchivedCount = archivedCount;

                ViewBag.TotalWords = totalWords;
                ViewBag.TargetWords = targetWords;
                ViewBag.ProgressPercent = progressPercent;
                ViewBag.AvgWordsPerChapter = avgWordsPerChapter;
                ViewBag.TotalParts = storyParts.Count;

                ViewBag.CurrentlyWritingChapter = currentlyWritingChapter;
                ViewBag.StoryParts = storyParts;
                ViewBag.RecentChapters = chapters.OrderByDescending(c => c.UpdatedAt).Take(5).ToList();

                return View(chapters);
            }

            ViewBag.TotalChapters = 0;
            ViewBag.CompletedCount = 0;
            ViewBag.InProgressCount = 0;
            ViewBag.OutliningCount = 0;
            ViewBag.PlannedCount = 0;
            ViewBag.OnHoldCount = 0;
            ViewBag.ArchivedCount = 0;
            ViewBag.TotalWords = 0;
            ViewBag.TargetWords = 32000;
            ViewBag.ProgressPercent = 0;
            ViewBag.AvgWordsPerChapter = 0;
            ViewBag.TotalParts = 0;
            ViewBag.CurrentlyWritingChapter = null;
            ViewBag.StoryParts = new List<StoryPart>();
            ViewBag.RecentChapters = new List<Chapter>();

            var allUserChapters = await _context.Chapters
                .Include(c => c.Story)
                .Where(c => c.Story.UserId == user.Id)
                .OrderBy(c => c.Story.Title)
                .ThenBy(c => c.Order)
                .ToListAsync();

            return View(allUserChapters);
        }

        // GET: Chapters/Editor?storyId=...&chapterId=...
        public async Task<IActionResult> Editor(Guid storyId, Guid chapterId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var story = await _context.Stories
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == storyId && s.UserId == user.Id);

            if (story == null) return NotFound();

            var chapter = await _context.Chapters
                .AsNoTracking()
                .Include(c => c.Part)
                .FirstOrDefaultAsync(c => c.Id == chapterId && c.StoryId == storyId);

            if (chapter == null) return NotFound();

            // Set active story session
            _activeStoryService.SetActiveStoryId(HttpContext, storyId);

            // Manuscript structure for left panel
            var storyParts = await _context.StoryParts
                .AsNoTracking()
                .Where(p => p.StoryId == storyId)
                .OrderBy(p => p.Order)
                .ToListAsync();

            var allChapters = await _context.Chapters
                .AsNoTracking()
                .Where(c => c.StoryId == storyId)
                .OrderBy(c => c.Order)
                .ToListAsync();

            // Story context items for right panel
            var characters = await _context.Characters
                .AsNoTracking()
                .Where(c => c.StoryId == storyId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var worldEntities = await _context.WorldEntities
                .AsNoTracking()
                .Include(w => w.EntityType)
                .Where(w => w.StoryId == storyId && w.ActiveStatus)
                .OrderBy(w => w.Name)
                .ToListAsync();

            var timelineEvents = await _context.TimelineEvents
                .AsNoTracking()
                .Where(t => t.StoryId == storyId)
                .OrderBy(t => t.DisplayOrder)
                .ThenBy(t => t.CreatedAt)
                .ToListAsync();

            var researchNotes = await _context.ResearchNotes
                .AsNoTracking()
                .Where(r => r.StoryId == storyId)
                .OrderByDescending(r => r.UpdatedAt)
                .ToListAsync();

            var storyArcs = await _context.StoryArcs
                .AsNoTracking()
                .Where(a => a.StoryId == storyId)
                .OrderBy(a => a.DisplayOrder)
                .ToListAsync();

            // Linked entity IDs for active chapter
            var linkedCharacterIds = await _context.ChapterCharacters
                .AsNoTracking()
                .Where(cc => cc.ChapterId == chapterId)
                .Select(cc => cc.CharacterId)
                .ToListAsync();

            var linkedWorldEntityIds = await _context.ChapterWorldEntities
                .AsNoTracking()
                .Where(cw => cw.ChapterId == chapterId)
                .Select(cw => cw.WorldEntityId)
                .ToListAsync();

            var linkedTimelineEventIds = await _context.TimelineEventChapters
                .AsNoTracking()
                .Where(tc => tc.ChapterId == chapterId)
                .Select(tc => tc.TimelineEventId)
                .ToListAsync();

            var linkedResearchNoteIds = await _context.ResearchChapters
                .AsNoTracking()
                .Where(rc => rc.ChapterId == chapterId)
                .Select(rc => rc.ResearchNoteId)
                .ToListAsync();

            ViewBag.Story = story;
            ViewBag.StoryParts = storyParts;
            ViewBag.AllChapters = allChapters;
            ViewBag.Characters = characters;
            ViewBag.WorldEntities = worldEntities;
            ViewBag.TimelineEvents = timelineEvents;
            ViewBag.ResearchNotes = researchNotes;
            ViewBag.StoryArcs = storyArcs;
            ViewBag.LinkedCharacterIds = linkedCharacterIds;
            ViewBag.LinkedWorldEntityIds = linkedWorldEntityIds;
            ViewBag.LinkedTimelineEventIds = linkedTimelineEventIds;
            ViewBag.LinkedResearchNoteIds = linkedResearchNoteIds;

            return View(chapter);
        }

        // GET: Chapters/Read?storyId=...&chapterId=...
        public async Task<IActionResult> Read(Guid? storyId, Guid? chapterId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var userStories = await _activeStoryService.GetUserStoriesAsync(user.Id);
            ViewBag.Stories = userStories;

            var activeStoryIdGuid = await _activeStoryService.GetActiveStoryIdAsync(HttpContext, user.Id, storyId);
            if (!activeStoryIdGuid.HasValue && userStories.Any())
            {
                activeStoryIdGuid = userStories.First().Id;
            }

            if (!activeStoryIdGuid.HasValue)
            {
                return RedirectToAction("Index", "Stories");
            }

            var storyIdGuid = activeStoryIdGuid.Value;

            var story = await _context.Stories
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.StoryGenres)
                    .ThenInclude(sg => sg.Genre)
                .FirstOrDefaultAsync(s => s.Id == storyIdGuid && s.UserId == user.Id);

            if (story == null) return NotFound();

            // Set active story session
            _activeStoryService.SetActiveStoryId(HttpContext, storyIdGuid);

            var authorName = !string.IsNullOrEmpty(story.User?.DisplayName) ? story.User.DisplayName :
                             !string.IsNullOrEmpty(story.User?.FirstName) ? $"{story.User.FirstName} {story.User.LastName}".Trim() :
                             story.User?.UserName ?? user.DisplayName ?? user.UserName ?? "Author";
            ViewBag.AuthorName = authorName;

            var allChapters = await _context.Chapters
                .AsNoTracking()
                .Include(c => c.Part)
                .Where(c => c.StoryId == storyIdGuid)
                .OrderBy(c => c.Order)
                .ToListAsync();

            if (!allChapters.Any())
            {
                ViewBag.Story = story;
                ViewBag.AllChapters = allChapters;
                ViewBag.StoryParts = new List<StoryPart>();
                ViewBag.Characters = new List<Character>();
                ViewBag.WorldEntities = new List<WorldEntity>();
                ViewBag.TimelineEvents = new List<TimelineEvent>();
                ViewBag.LinkedCharacterIds = new List<Guid>();
                ViewBag.LinkedWorldEntityIds = new List<Guid>();
                ViewBag.LinkedTimelineEventIds = new List<Guid>();
                return View("Read", null);
            }

            Chapter? chapter = null;
            if (chapterId.HasValue && chapterId.Value != Guid.Empty)
            {
                chapter = allChapters.FirstOrDefault(c => c.Id == chapterId.Value);
            }

            if (chapter == null)
            {
                chapter = allChapters.FirstOrDefault(c => c.Status == "InProgress") 
                    ?? allChapters.First();
            }

            var storyParts = await _context.StoryParts
                .AsNoTracking()
                .Where(p => p.StoryId == storyIdGuid)
                .OrderBy(p => p.Order)
                .ToListAsync();

            var characters = await _context.Characters
                .AsNoTracking()
                .Where(c => c.StoryId == storyIdGuid)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var worldEntities = await _context.WorldEntities
                .AsNoTracking()
                .Include(w => w.EntityType)
                .Where(w => w.StoryId == storyIdGuid && w.ActiveStatus)
                .OrderBy(w => w.Name)
                .ToListAsync();

            var timelineEvents = await _context.TimelineEvents
                .AsNoTracking()
                .Where(t => t.StoryId == storyIdGuid)
                .OrderBy(t => t.DisplayOrder)
                .ThenBy(t => t.CreatedAt)
                .ToListAsync();

            var linkedCharacterIds = await _context.ChapterCharacters
                .AsNoTracking()
                .Where(cc => cc.ChapterId == chapter.Id)
                .Select(cc => cc.CharacterId)
                .ToListAsync();

            var linkedWorldEntityIds = await _context.ChapterWorldEntities
                .AsNoTracking()
                .Where(cw => cw.ChapterId == chapter.Id)
                .Select(cw => cw.WorldEntityId)
                .ToListAsync();

            var linkedTimelineEventIds = await _context.TimelineEventChapters
                .AsNoTracking()
                .Where(tc => tc.ChapterId == chapter.Id)
                .Select(tc => tc.TimelineEventId)
                .ToListAsync();

            ViewBag.Story = story;
            ViewBag.StoryParts = storyParts;
            ViewBag.AllChapters = allChapters;
            ViewBag.Characters = characters;
            ViewBag.WorldEntities = worldEntities;
            ViewBag.TimelineEvents = timelineEvents;
            ViewBag.LinkedCharacterIds = linkedCharacterIds;
            ViewBag.LinkedWorldEntityIds = linkedWorldEntityIds;
            ViewBag.LinkedTimelineEventIds = linkedTimelineEventIds;

            return View("Read", chapter);
        }

        // POST: Chapters/SaveDraft
        [HttpPost]
        public async Task<IActionResult> SaveDraft([FromBody] SaveDraftDto model)
        {
            if (model == null || model.ChapterId == Guid.Empty)
                return BadRequest(new { success = false, message = "Invalid chapter payload." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // 1. Authenticate user & Validate Story ownership & Chapter belonging
            var chapter = await _context.Chapters
                .Include(c => c.Story)
                .FirstOrDefaultAsync(c => c.Id == model.ChapterId && c.Story.UserId == user.Id);

            if (chapter == null)
                return NotFound(new { success = false, message = "Chapter not found or access denied." });

            if (model.StoryId != Guid.Empty && chapter.StoryId != model.StoryId)
            {
                return BadRequest(new { success = false, message = "Chapter does not belong to specified story." });
            }

            // 2. Concurrency / Stale Save Protection
            if (model.Version > 0 && model.Version < chapter.Version)
            {
                return StatusCode(409, new
                {
                    success = false,
                    message = $"Stale save attempt rejected. Server has revision {chapter.Version}, client sent revision {model.Version}.",
                    serverVersion = chapter.Version,
                    serverContent = chapter.Content,
                    serverTitle = chapter.Title,
                    wordCount = chapter.WordCount,
                    characterCount = chapter.CharacterCount
                });
            }

            var oldWordCount = chapter.WordCount;

            // 3. Update Title & Content & Status
            if (!string.IsNullOrWhiteSpace(model.Title))
            {
                chapter.Title = model.Title.Trim();
            }

            chapter.Content = model.Content ?? string.Empty;

            // 4. Sanitize and calculate word & character counts from text content
            var plainText = System.Text.RegularExpressions.Regex.Replace(chapter.Content, "<.*?>", " ");
            plainText = System.Net.WebUtility.HtmlDecode(plainText);
            var words = plainText.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            chapter.WordCount = words.Length;
            chapter.CharacterCount = plainText.Replace(" ", "").Length;

            if (!string.IsNullOrWhiteSpace(model.Status))
            {
                chapter.Status = model.Status;
            }

            if (!string.IsNullOrWhiteSpace(model.Summary))
            {
                chapter.Summary = model.Summary;
            }

            if (model.TargetWordCount.HasValue && model.TargetWordCount > 0)
            {
                chapter.TargetWordCount = model.TargetWordCount;
            }

            // Increment version & update timestamp
            chapter.Version = Math.Max(chapter.Version + 1, model.Version + 1);
            chapter.UpdatedAt = DateTime.UtcNow;

            // 5. Recalculate Story word count
            var totalWords = await _context.Chapters
                .Where(c => c.StoryId == chapter.StoryId && c.Id != chapter.Id)
                .SumAsync(c => c.WordCount) + chapter.WordCount;

            chapter.Story.CurrentWordCount = totalWords;
            chapter.Story.UpdatedAt = DateTime.UtcNow;

            // 6. Update user daily word count progress & writing streak if words increased
            var diff = chapter.WordCount - oldWordCount;
            if (diff > 0)
            {
                var todayUtc = DateTime.UtcNow.Date;
                var yesterdayUtc = todayUtc.AddDays(-1);

                var userGoal = await _context.UserGoals.FirstOrDefaultAsync(g => g.UserId == user.Id);
                if (userGoal == null)
                {
                    userGoal = new UserGoal
                    {
                        UserId = user.Id,
                        DailyWordCountGoal = 1000,
                        WeeklyWordCountGoal = 5000,
                        MonthlyWordCountGoal = 20000,
                        WordsWrittenToday = diff,
                        WordsWrittenThisWeek = diff,
                        WordsWrittenThisMonth = diff,
                        CurrentStreakDays = 1,
                        LastUpdated = DateTime.UtcNow
                    };
                    _context.UserGoals.Add(userGoal);
                }
                else
                {
                    var lastUpdatedDate = userGoal.LastUpdated.Date;

                    if (lastUpdatedDate != todayUtc)
                    {
                        userGoal.WordsWrittenToday = diff;

                        if (lastUpdatedDate == yesterdayUtc)
                        {
                            // Consecutive day writing: increment streak
                            userGoal.CurrentStreakDays += 1;
                        }
                        else
                        {
                            // Missed one or more days: reset streak to 1 today
                            userGoal.CurrentStreakDays = 1;
                        }
                    }
                    else
                    {
                        userGoal.WordsWrittenToday += diff;
                        if (userGoal.CurrentStreakDays <= 0)
                        {
                            userGoal.CurrentStreakDays = 1;
                        }
                    }
                    userGoal.WordsWrittenThisWeek += diff;
                    userGoal.WordsWrittenThisMonth += diff;
                    userGoal.LastUpdated = DateTime.UtcNow;
                }

                // Log Activity so daily activity indicators in sidebar highlight today
                var hasLoggedToday = await _context.ActivityLogs
                    .AnyAsync(a => a.UserId == user.Id && a.Timestamp.Date == todayUtc && a.ActionType == "Writing");

                if (!hasLoggedToday)
                {
                    _context.ActivityLogs.Add(new ActivityLog
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        ActionType = "Writing",
                        Description = $"Wrote {diff} words in chapter '{chapter.Title}'",
                        RelatedEntityName = chapter.Title,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                chapterId = chapter.Id,
                storyId = chapter.StoryId,
                title = chapter.Title,
                savedAt = DateTime.UtcNow.ToString("o"),
                lastSaved = DateTime.UtcNow.ToString("hh:mm:ss tt"),
                version = chapter.Version,
                wordCount = chapter.WordCount,
                characterCount = chapter.CharacterCount,
                totalStoryWords = chapter.Story.CurrentWordCount
            });
        }

        // POST: Chapters/SaveContent (Legacy alias delegating to SaveDraft)
        [HttpPost]
        public async Task<IActionResult> SaveContent([FromBody] SaveChapterDto model)
        {
            if (model == null) return BadRequest();
            return await SaveDraft(new SaveDraftDto
            {
                ChapterId = model.ChapterId,
                Title = model.Title,
                Content = model.Content,
                WordCount = model.WordCount,
                Status = model.Status
            });
        }

        // GET: Chapters/GetContextData?storyId=...&chapterId=...
        [HttpGet]
        public async Task<IActionResult> GetContextData(Guid storyId, Guid chapterId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var story = await _context.Stories.FirstOrDefaultAsync(s => s.Id == storyId && s.UserId == user.Id);
            if (story == null) return NotFound();

            var characters = await _context.Characters
                .Where(c => c.StoryId == storyId)
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Role,
                    c.Nicknames,
                    c.Age,
                    c.Occupation,
                    c.Status,
                    c.OneLineDescription,
                    c.AvatarUrl,
                    isLinked = _context.ChapterCharacters.Any(cc => cc.ChapterId == chapterId && cc.CharacterId == c.Id)
                }).ToListAsync();

            var worldEntities = await _context.WorldEntities
                .Include(w => w.EntityType)
                .Where(w => w.StoryId == storyId && w.ActiveStatus)
                .OrderBy(w => w.Name)
                .Select(w => new
                {
                    w.Id,
                    w.Name,
                    typeName = w.EntityType != null ? w.EntityType.Name : "Location",
                    w.Summary,
                    w.CoverImage,
                    w.Importance,
                    isLinked = _context.ChapterWorldEntities.Any(cw => cw.ChapterId == chapterId && cw.WorldEntityId == w.Id)
                }).ToListAsync();

            var timelineEvents = await _context.TimelineEvents
                .Where(t => t.StoryId == storyId)
                .OrderBy(t => t.DisplayOrder)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Category,
                    t.StoryDate,
                    t.Importance,
                    t.Summary,
                    isLinked = _context.TimelineEventChapters.Any(tc => tc.ChapterId == chapterId && tc.TimelineEventId == t.Id)
                }).ToListAsync();

            var researchNotes = await _context.ResearchNotes
                .Where(r => r.StoryId == storyId)
                .OrderByDescending(r => r.UpdatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.Title,
                    r.Category,
                    r.Content,
                    isLinked = _context.ResearchChapters.Any(rc => rc.ChapterId == chapterId && rc.ResearchNoteId == r.Id)
                }).ToListAsync();

            var manuscriptParts = await _context.StoryParts
                .Where(p => p.StoryId == storyId)
                .OrderBy(p => p.Order)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Order,
                    chapters = _context.Chapters
                        .Where(c => c.PartId == p.Id)
                        .OrderBy(c => c.Order)
                        .Select(c => new { c.Id, c.Title, c.Order, c.WordCount, c.Status })
                        .ToList()
                }).ToListAsync();

            var unassignedChapters = await _context.Chapters
                .Where(c => c.StoryId == storyId && c.PartId == null)
                .OrderBy(c => c.Order)
                .Select(c => new { c.Id, c.Title, c.Order, c.WordCount, c.Status })
                .ToListAsync();

            return Json(new
            {
                success = true,
                storyId,
                chapterId,
                characters,
                worldEntities,
                timelineEvents,
                researchNotes,
                manuscriptParts,
                unassignedChapters
            });
        }

        // GET: Chapters/GetEntityPreview?type=character|world|timeline&id=...&storyId=...
        [HttpGet]
        public async Task<IActionResult> GetEntityPreview(string type, Guid id, Guid storyId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (type == "character")
            {
                var character = await _context.Characters
                    .FirstOrDefaultAsync(c => c.Id == id && c.StoryId == storyId);
                if (character == null) return NotFound();

                var appearsCount = await _context.ChapterCharacters
                    .CountAsync(cc => cc.CharacterId == id);

                return Json(new
                {
                    success = true,
                    type = "character",
                    id = character.Id,
                    name = character.Name,
                    role = character.Role ?? "Character",
                    status = character.Status ?? "Active",
                    avatarUrl = character.AvatarUrl,
                    description = character.OneLineDescription ?? character.BackgroundSummary ?? "No description available.",
                    appearsInChaptersCount = appearsCount,
                    tags = character.Tags,
                    age = character.Age,
                    occupation = character.Occupation
                });
            }
            else if (type == "world" || type == "location")
            {
                var entity = await _context.WorldEntities
                    .Include(w => w.EntityType)
                    .FirstOrDefaultAsync(w => w.Id == id && w.StoryId == storyId);
                if (entity == null) return NotFound();

                var appearsCount = await _context.ChapterWorldEntities
                    .CountAsync(cw => cw.WorldEntityId == id);

                return Json(new
                {
                    success = true,
                    type = "world",
                    id = entity.Id,
                    name = entity.Name,
                    typeName = entity.EntityType != null ? entity.EntityType.Name : "Location",
                    importance = entity.Importance ?? "Major",
                    coverImage = entity.CoverImage,
                    summary = entity.Summary ?? entity.Description ?? "No description available.",
                    appearsInChaptersCount = appearsCount
                });
            }
            else if (type == "timeline" || type == "event")
            {
                var evt = await _context.TimelineEvents
                    .FirstOrDefaultAsync(t => t.Id == id && t.StoryId == storyId);
                if (evt == null) return NotFound();

                var appearsCount = await _context.TimelineEventChapters
                    .CountAsync(tc => tc.TimelineEventId == id);

                return Json(new
                {
                    success = true,
                    type = "timeline",
                    id = evt.Id,
                    title = evt.Title,
                    category = evt.Category,
                    storyDate = evt.StoryDate,
                    importance = evt.Importance,
                    description = evt.Description ?? evt.Summary ?? "No details provided.",
                    appearsInChaptersCount = appearsCount
                });
            }

            return BadRequest();
        }

        // POST: Chapters/ToggleLinkCharacter
        [HttpPost]
        public async Task<IActionResult> ToggleLinkCharacter([FromBody] LinkEntityDto model)
        {
            if (model == null || model.ChapterId == Guid.Empty || model.EntityId == Guid.Empty)
                return BadRequest();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var existing = await _context.ChapterCharacters
                .FirstOrDefaultAsync(cc => cc.ChapterId == model.ChapterId && cc.CharacterId == model.EntityId);

            bool isLinked;
            if (existing != null)
            {
                _context.ChapterCharacters.Remove(existing);
                isLinked = false;
            }
            else
            {
                _context.ChapterCharacters.Add(new ChapterCharacter
                {
                    Id = Guid.NewGuid(),
                    ChapterId = model.ChapterId,
                    CharacterId = model.EntityId,
                    Role = !string.IsNullOrWhiteSpace(model.Role) ? model.Role : "Major"
                });
                isLinked = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, isLinked });
        }

        // POST: Chapters/ToggleLinkWorldEntity
        [HttpPost]
        public async Task<IActionResult> ToggleLinkWorldEntity([FromBody] LinkEntityDto model)
        {
            if (model == null || model.ChapterId == Guid.Empty || model.EntityId == Guid.Empty)
                return BadRequest();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var existing = await _context.ChapterWorldEntities
                .FirstOrDefaultAsync(cw => cw.ChapterId == model.ChapterId && cw.WorldEntityId == model.EntityId);

            bool isLinked;
            if (existing != null)
            {
                _context.ChapterWorldEntities.Remove(existing);
                isLinked = false;
            }
            else
            {
                _context.ChapterWorldEntities.Add(new ChapterWorldEntity
                {
                    Id = Guid.NewGuid(),
                    ChapterId = model.ChapterId,
                    WorldEntityId = model.EntityId
                });
                isLinked = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, isLinked });
        }

        // POST: Chapters/ToggleLinkTimelineEvent
        [HttpPost]
        public async Task<IActionResult> ToggleLinkTimelineEvent([FromBody] LinkEntityDto model)
        {
            if (model == null || model.ChapterId == Guid.Empty || model.EntityId == Guid.Empty)
                return BadRequest();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var existing = await _context.TimelineEventChapters
                .FirstOrDefaultAsync(tc => tc.ChapterId == model.ChapterId && tc.TimelineEventId == model.EntityId);

            bool isLinked;
            if (existing != null)
            {
                _context.TimelineEventChapters.Remove(existing);
                isLinked = false;
            }
            else
            {
                _context.TimelineEventChapters.Add(new TimelineEventChapter
                {
                    Id = Guid.NewGuid(),
                    ChapterId = model.ChapterId,
                    TimelineEventId = model.EntityId
                });
                isLinked = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, isLinked });
        }

        // GET: Chapters/CheckDuplicate?storyId=...&type=character|location|timeline&name=...
        [HttpGet]
        public async Task<IActionResult> CheckDuplicate(Guid storyId, string type, string name)
        {
            if (string.IsNullOrWhiteSpace(name) || storyId == Guid.Empty)
                return Json(new { isDuplicate = false });

            var queryName = name.Trim().ToLower();

            if (type == "character")
            {
                var match = await _context.Characters
                    .Where(c => c.StoryId == storyId && c.Name.ToLower() == queryName)
                    .Select(c => new { c.Id, c.Name, c.Role, c.OneLineDescription })
                    .FirstOrDefaultAsync();

                if (match != null)
                    return Json(new { isDuplicate = true, existingEntity = match });
            }
            else if (type == "location" || type == "world")
            {
                var match = await _context.WorldEntities
                    .Include(w => w.EntityType)
                    .Where(w => w.StoryId == storyId && w.Name.ToLower() == queryName && w.ActiveStatus)
                    .Select(w => new { w.Id, w.Name, entityType = w.EntityType != null ? w.EntityType.Name : "Location", w.Summary })
                    .FirstOrDefaultAsync();

                if (match != null)
                    return Json(new { isDuplicate = true, existingEntity = match });
            }
            else if (type == "timeline" || type == "event")
            {
                var match = await _context.TimelineEvents
                    .Where(t => t.StoryId == storyId && t.Title.ToLower() == queryName)
                    .Select(t => new { t.Id, t.Title, t.Category, t.StoryDate })
                    .FirstOrDefaultAsync();

                if (match != null)
                    return Json(new { isDuplicate = true, existingEntity = match });
            }

            return Json(new { isDuplicate = false });
        }

        // POST: Chapters/QuickCreateCharacter
        [HttpPost]
        public async Task<IActionResult> QuickCreateCharacter([FromBody] QuickCreateCharacterDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name) || dto.StoryId == Guid.Empty)
                return BadRequest(new { success = false, message = "Character name and story ID are required." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var story = await _context.Stories.FirstOrDefaultAsync(s => s.Id == dto.StoryId && s.UserId == user.Id);
            if (story == null) return NotFound(new { success = false, message = "Story not found." });

            // Duplicate prevention check
            if (!dto.ForceCreate)
            {
                var existing = await _context.Characters
                    .FirstOrDefaultAsync(c => c.StoryId == dto.StoryId && c.Name.ToLower() == dto.Name.Trim().ToLower());

                if (existing != null)
                {
                    return Json(new
                    {
                        success = false,
                        isDuplicate = true,
                        existingCharacter = new { existing.Id, existing.Name, existing.Role, existing.OneLineDescription },
                        message = $"Character '{existing.Name}' already exists in story."
                    });
                }
            }

            var character = new Character
            {
                Id = Guid.NewGuid(),
                StoryId = dto.StoryId,
                Name = dto.Name.Trim(),
                Role = !string.IsNullOrWhiteSpace(dto.Role) ? dto.Role : "Supporting",
                OneLineDescription = dto.OneLineDescription,
                Age = dto.Age,
                Gender = dto.Gender,
                Occupation = dto.Occupation,
                PersonalityTraits = dto.PersonalityTraits,
                Tags = dto.Tags,
                AvatarUrl = dto.AvatarUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Characters.Add(character);

            // Automatically link to Chapter
            if (dto.ChapterId.HasValue && dto.ChapterId.Value != Guid.Empty)
            {
                _context.ChapterCharacters.Add(new ChapterCharacter
                {
                    Id = Guid.NewGuid(),
                    ChapterId = dto.ChapterId.Value,
                    CharacterId = character.Id,
                    Role = character.Role
                });
            }

            // Optional Character Relationship
            if (dto.TargetCharacterId.HasValue && dto.TargetCharacterId.Value != Guid.Empty && !string.IsNullOrWhiteSpace(dto.RelationshipType))
            {
                _context.CharacterRelationships.Add(new CharacterRelationship
                {
                    Id = Guid.NewGuid(),
                    StoryId = dto.StoryId,
                    SourceCharacterId = character.Id,
                    TargetCharacterId = dto.TargetCharacterId.Value,
                    RelationshipType = dto.RelationshipType,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Activity Log
            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ActionType = "Character",
                Description = $"Quick-created character '{character.Name}' for story '{story.Title}'",
                RelatedEntityName = character.Name,
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var chapterTitle = dto.ChapterId.HasValue ? (await _context.Chapters.Where(c => c.Id == dto.ChapterId.Value).Select(c => c.Title).FirstOrDefaultAsync()) : null;

            return Json(new
            {
                success = true,
                character = new
                {
                    character.Id,
                    character.Name,
                    character.Role,
                    character.Nicknames,
                    character.Age,
                    character.Occupation,
                    character.Status,
                    character.OneLineDescription,
                    character.AvatarUrl,
                    isLinked = true
                },
                notification = $"{character.Name} added to {story.Title}" + (!string.IsNullOrEmpty(chapterTitle) ? $" and linked to {chapterTitle}." : ".")
            });
        }

        // POST: Chapters/QuickCreateWorldEntity
        [HttpPost]
        public async Task<IActionResult> QuickCreateWorldEntity([FromBody] QuickCreateWorldEntityDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name) || dto.StoryId == Guid.Empty)
                return BadRequest(new { success = false, message = "Location/Entity name and story ID are required." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var story = await _context.Stories.FirstOrDefaultAsync(s => s.Id == dto.StoryId && s.UserId == user.Id);
            if (story == null) return NotFound(new { success = false, message = "Story not found." });

            // Duplicate prevention check
            if (!dto.ForceCreate)
            {
                var existing = await _context.WorldEntities
                    .FirstOrDefaultAsync(w => w.StoryId == dto.StoryId && w.Name.ToLower() == dto.Name.Trim().ToLower() && w.ActiveStatus);

                if (existing != null)
                {
                    return Json(new
                    {
                        success = false,
                        isDuplicate = true,
                        existingEntity = new { existing.Id, existing.Name, existing.Summary },
                        message = $"World Entity '{existing.Name}' already exists in story."
                    });
                }
            }

            // Get or create EntityType
            var typeName = !string.IsNullOrWhiteSpace(dto.TypeName) ? dto.TypeName.Trim() : "Location";
            var entityType = await _context.WorldEntityTypes.FirstOrDefaultAsync(t => t.Name.ToLower() == typeName.ToLower());
            if (entityType == null)
            {
                entityType = new WorldEntityType
                {
                    Id = Guid.NewGuid(),
                    Name = typeName,
                    Category = typeName == "Location" ? "Locations" : "Organizations",
                    Icon = typeName == "Location" ? "map-pin" : "building",
                    IsSystemDefault = false,
                    UserId = user.Id
                };
                _context.WorldEntityTypes.Add(entityType);
                await _context.SaveChangesAsync();
            }

            var worldEntity = new WorldEntity
            {
                Id = Guid.NewGuid(),
                StoryId = dto.StoryId,
                EntityTypeId = entityType.Id,
                Name = dto.Name.Trim(),
                Summary = dto.Summary,
                Description = dto.Description,
                Importance = !string.IsNullOrWhiteSpace(dto.Importance) ? dto.Importance : "Major",
                Status = !string.IsNullOrWhiteSpace(dto.Status) ? dto.Status : "Active",
                ParentEntityId = dto.ParentEntityId,
                CoverImage = dto.CoverImage,
                ActiveStatus = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.WorldEntities.Add(worldEntity);

            // Also keep legacy Location sync if type is Location
            if (typeName.Equals("Location", StringComparison.OrdinalIgnoreCase))
            {
                _context.Locations.Add(new Location
                {
                    Id = Guid.NewGuid(),
                    StoryId = dto.StoryId,
                    Name = worldEntity.Name,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Automatically link to Chapter
            if (dto.ChapterId.HasValue && dto.ChapterId.Value != Guid.Empty)
            {
                _context.ChapterWorldEntities.Add(new ChapterWorldEntity
                {
                    Id = Guid.NewGuid(),
                    ChapterId = dto.ChapterId.Value,
                    WorldEntityId = worldEntity.Id
                });
            }

            // Optional World Relationship
            if (dto.ParentEntityId.HasValue && dto.ParentEntityId.Value != Guid.Empty && !string.IsNullOrWhiteSpace(dto.RelationshipType))
            {
                _context.WorldEntityRelationships.Add(new WorldEntityRelationship
                {
                    Id = Guid.NewGuid(),
                    SourceEntityId = worldEntity.Id,
                    TargetEntityId = dto.ParentEntityId.Value,
                    RelationshipType = dto.RelationshipType
                });
            }

            // Log Activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ActionType = "WorldEntity",
                Description = $"Quick-created {typeName} '{worldEntity.Name}' for story '{story.Title}'",
                RelatedEntityName = worldEntity.Name,
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var chapterTitle = dto.ChapterId.HasValue ? (await _context.Chapters.Where(c => c.Id == dto.ChapterId.Value).Select(c => c.Title).FirstOrDefaultAsync()) : null;

            return Json(new
            {
                success = true,
                worldEntity = new
                {
                    worldEntity.Id,
                    worldEntity.Name,
                    typeName = entityType.Name,
                    worldEntity.Summary,
                    worldEntity.CoverImage,
                    worldEntity.Importance,
                    isLinked = true
                },
                notification = $"{worldEntity.Name} added to {story.Title}" + (!string.IsNullOrEmpty(chapterTitle) ? $" and linked to {chapterTitle}." : ".")
            });
        }

        // POST: Chapters/QuickCreateTimelineEvent
        [HttpPost]
        public async Task<IActionResult> QuickCreateTimelineEvent([FromBody] QuickCreateTimelineEventDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title) || dto.StoryId == Guid.Empty)
                return BadRequest(new { success = false, message = "Event title and story ID are required." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var story = await _context.Stories.FirstOrDefaultAsync(s => s.Id == dto.StoryId && s.UserId == user.Id);
            if (story == null) return NotFound(new { success = false, message = "Story not found." });

            // Duplicate prevention check
            if (!dto.ForceCreate)
            {
                var existing = await _context.TimelineEvents
                    .FirstOrDefaultAsync(t => t.StoryId == dto.StoryId && t.Title.ToLower() == dto.Title.Trim().ToLower());

                if (existing != null)
                {
                    return Json(new
                    {
                        success = false,
                        isDuplicate = true,
                        existingEvent = new { existing.Id, existing.Title, existing.Category, existing.StoryDate },
                        message = $"Timeline Event '{existing.Title}' already exists in story."
                    });
                }
            }

            var nextOrder = await _context.TimelineEvents
                .Where(t => t.StoryId == dto.StoryId)
                .CountAsync() + 1;

            var timelineEvent = new TimelineEvent
            {
                Id = Guid.NewGuid(),
                StoryId = dto.StoryId,
                Title = dto.Title.Trim(),
                Category = !string.IsNullOrWhiteSpace(dto.Category) ? dto.Category : "General",
                EventType = !string.IsNullOrWhiteSpace(dto.EventType) ? dto.EventType : "Standard",
                StoryDate = dto.StoryDate,
                Description = dto.Description,
                Importance = !string.IsNullOrWhiteSpace(dto.Importance) ? dto.Importance : "Medium",
                DisplayOrder = nextOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TimelineEvents.Add(timelineEvent);

            // Automatically link to Chapter
            if (dto.ChapterId.HasValue && dto.ChapterId.Value != Guid.Empty)
            {
                _context.TimelineEventChapters.Add(new TimelineEventChapter
                {
                    Id = Guid.NewGuid(),
                    ChapterId = dto.ChapterId.Value,
                    TimelineEventId = timelineEvent.Id
                });
            }

            // Link participant characters
            if (dto.CharacterIds != null && dto.CharacterIds.Any())
            {
                foreach (var cid in dto.CharacterIds)
                {
                    _context.TimelineCharacters.Add(new TimelineCharacter
                    {
                        Id = Guid.NewGuid(),
                        TimelineEventId = timelineEvent.Id,
                        CharacterId = cid,
                        Role = "Participant"
                    });
                }
            }

            // Link participant world entities / locations
            if (dto.WorldEntityIds != null && dto.WorldEntityIds.Any())
            {
                foreach (var wid in dto.WorldEntityIds)
                {
                    _context.TimelineWorldEntities.Add(new TimelineWorldEntity
                    {
                        Id = Guid.NewGuid(),
                        TimelineEventId = timelineEvent.Id,
                        WorldEntityId = wid,
                        Role = "Location"
                    });
                }
            }

            // Link Story Arc if provided
            if (dto.StoryArcId.HasValue && dto.StoryArcId.Value != Guid.Empty)
            {
                _context.StoryArcEvents.Add(new StoryArcEvent
                {
                    Id = Guid.NewGuid(),
                    StoryArcId = dto.StoryArcId.Value,
                    TimelineEventId = timelineEvent.Id,
                    OrderInArc = nextOrder
                });
            }

            // Activity Log
            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ActionType = "TimelineEvent",
                Description = $"Quick-created timeline event '{timelineEvent.Title}' for story '{story.Title}'",
                RelatedEntityName = timelineEvent.Title,
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var chapterTitle = dto.ChapterId.HasValue ? (await _context.Chapters.Where(c => c.Id == dto.ChapterId.Value).Select(c => c.Title).FirstOrDefaultAsync()) : null;

            return Json(new
            {
                success = true,
                timelineEvent = new
                {
                    timelineEvent.Id,
                    timelineEvent.Title,
                    timelineEvent.Category,
                    timelineEvent.StoryDate,
                    timelineEvent.Importance,
                    timelineEvent.Summary,
                    isLinked = true
                },
                notification = $"'{timelineEvent.Title}' added to {story.Title}" + (!string.IsNullOrEmpty(chapterTitle) ? $" and linked to {chapterTitle}." : ".")
            });
        }

        // POST: Chapters/QuickCreateResearchNote
        [HttpPost]
        public async Task<IActionResult> QuickCreateResearchNote([FromBody] QuickCreateResearchNoteDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title) || dto.StoryId == Guid.Empty)
                return BadRequest(new { success = false, message = "Note title and story ID are required." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var story = await _context.Stories.FirstOrDefaultAsync(s => s.Id == dto.StoryId && s.UserId == user.Id);
            if (story == null) return NotFound(new { success = false, message = "Story not found." });

            var note = new ResearchNote
            {
                Id = Guid.NewGuid(),
                StoryId = dto.StoryId,
                Title = dto.Title.Trim(),
                Content = dto.Content,
                Category = !string.IsNullOrWhiteSpace(dto.Category) ? dto.Category : "General",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ResearchNotes.Add(note);

            if (dto.ChapterId.HasValue && dto.ChapterId.Value != Guid.Empty)
            {
                _context.ResearchChapters.Add(new ResearchChapter
                {
                    Id = Guid.NewGuid(),
                    ChapterId = dto.ChapterId.Value,
                    ResearchNoteId = note.Id
                });
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                note = new
                {
                    note.Id,
                    note.Title,
                    note.Category,
                    note.Content,
                    isLinked = true
                },
                notification = $"Research note '{note.Title}' added to story."
            });
        }

        // GET: Chapters/Create?storyId=...
        [HttpGet]
        public async Task<IActionResult> Create(Guid? storyId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var userStories = await _activeStoryService.GetUserStoriesAsync(user.Id);
            ViewBag.Stories = userStories;

            var activeStoryIdGuid = await _activeStoryService.GetActiveStoryIdAsync(HttpContext, user.Id, storyId);

            Story? selectedStory = activeStoryIdGuid.HasValue
                ? await _context.Stories
                    .Include(s => s.Chapters)
                    .Include(s => s.StoryParts)
                    .Include(s => s.Characters)
                    .Include(s => s.Locations)
                    .Include(s => s.StoryGenres)
                        .ThenInclude(sg => sg.Genre)
                    .FirstOrDefaultAsync(s => s.Id == activeStoryIdGuid.Value && s.UserId == user.Id)
                : null;

            if (selectedStory == null && userStories.Any())
            {
                selectedStory = await _context.Stories
                    .Include(s => s.Chapters)
                    .Include(s => s.StoryParts)
                    .Include(s => s.Characters)
                    .Include(s => s.Locations)
                    .Include(s => s.StoryGenres)
                        .ThenInclude(sg => sg.Genre)
                    .FirstOrDefaultAsync(s => s.Id == userStories.First().Id && s.UserId == user.Id);
            }

            if (selectedStory == null) return RedirectToAction("Index", "Stories");

            var chapters = selectedStory.Chapters.OrderBy(c => c.Order).ToList();
            var storyParts = selectedStory.StoryParts.OrderBy(p => p.Order).ToList();
            var nextChapterNo = chapters.Any() ? chapters.Max(c => c.Order) + 1 : 1;

            ViewBag.Story = selectedStory;
            ViewBag.StoryParts = storyParts;
            ViewBag.NextChapterNumber = nextChapterNo;
            ViewBag.ExistingChaptersCount = chapters.Count;

            var userGoal = await _context.UserGoals.FirstOrDefaultAsync(g => g.UserId == user.Id);
            ViewBag.WritingStreak = userGoal?.CurrentStreakDays ?? 0;

            return View();
        }

        // POST: Chapters/SaveWizardStep
        [HttpPost]
        public async Task<IActionResult> SaveWizardStep([FromBody] SaveWizardStepDto dto)
        {
            if (dto == null || dto.StoryId == Guid.Empty)
                return BadRequest(new { success = false, message = "StoryId is required." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var story = await _context.Stories
                .Include(s => s.Chapters)
                .FirstOrDefaultAsync(s => s.Id == dto.StoryId && s.UserId == user.Id);

            if (story == null) return NotFound(new { success = false, message = "Story not found." });

            Chapter chapter;
            if (dto.ChapterId.HasValue && dto.ChapterId.Value != Guid.Empty)
            {
                chapter = await _context.Chapters.FirstOrDefaultAsync(c => c.Id == dto.ChapterId.Value && c.StoryId == dto.StoryId);
                if (chapter == null) return NotFound(new { success = false, message = "Chapter not found." });
            }
            else
            {
                var nextNo = dto.Order > 0 ? dto.Order : (story.Chapters.Any() ? story.Chapters.Max(c => c.Order) + 1 : 1);
                chapter = new Chapter
                {
                    Id = Guid.NewGuid(),
                    StoryId = dto.StoryId,
                    Order = nextNo,
                    Title = string.IsNullOrWhiteSpace(dto.Title) ? $"Chapter {nextNo}" : dto.Title.Trim(),
                    Status = !string.IsNullOrWhiteSpace(dto.Status) ? dto.Status : "Planned",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Chapters.Add(chapter);
            }

            if (!string.IsNullOrWhiteSpace(dto.Title)) chapter.Title = dto.Title.Trim();
            if (dto.PartId.HasValue) chapter.PartId = dto.PartId.Value != Guid.Empty ? dto.PartId : null;
            if (dto.Order > 0) chapter.Order = dto.Order;
            if (!string.IsNullOrWhiteSpace(dto.Summary)) chapter.Summary = dto.Summary;
            if (!string.IsNullOrWhiteSpace(dto.Status)) chapter.Status = dto.Status;
            if (dto.TargetWordCount.HasValue && dto.TargetWordCount > 0) chapter.TargetWordCount = dto.TargetWordCount;
            if (!string.IsNullOrWhiteSpace(dto.Purpose)) chapter.Purpose = dto.Purpose;
            if (!string.IsNullOrWhiteSpace(dto.Goal)) chapter.Goal = dto.Goal;
            if (!string.IsNullOrWhiteSpace(dto.KeyEvents)) chapter.KeyEvents = dto.KeyEvents;
            if (!string.IsNullOrWhiteSpace(dto.EmotionalTone)) chapter.EmotionalTone = dto.EmotionalTone;
            if (!string.IsNullOrWhiteSpace(dto.PointOfView)) chapter.PointOfView = dto.PointOfView;

            chapter.Version++;
            chapter.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                chapterId = chapter.Id,
                storyId = chapter.StoryId,
                order = chapter.Order,
                title = chapter.Title,
                status = chapter.Status,
                version = chapter.Version,
                savedAt = DateTime.UtcNow.ToString("hh:mm:ss tt")
            });
        }

        // POST: Chapters/Create
        [HttpPost]
        public async Task<IActionResult> Create(Guid storyId, Guid? partId = null)
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
                PartId = partId,
                Title = $"Chapter {nextNo}",
                Order = nextNo,
                WordCount = 0,
                Status = "Planned",
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

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    chapterId = newChapter.Id,
                    redirectUrl = Url.Action("Editor", "Chapters", new { storyId = storyId, chapterId = newChapter.Id })
                });
            }

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

        // POST: Chapters/CreatePart
        [HttpPost]
        public async Task<IActionResult> CreatePart([FromForm] Guid storyId, [FromForm] string title)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (storyId == Guid.Empty || string.IsNullOrWhiteSpace(title))
            {
                return Json(new { success = false, message = "Invalid story or part title." });
            }

            var lastPartOrder = await _context.StoryParts
                .Where(p => p.StoryId == storyId)
                .MaxAsync(p => (int?)p.Order) ?? 0;

            var newPart = new StoryPart
            {
                Id = Guid.NewGuid(),
                StoryId = storyId,
                Title = title.Trim(),
                Order = lastPartOrder + 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.StoryParts.Add(newPart);
            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                return Json(new { success = true, partId = newPart.Id, title = newPart.Title, redirectUrl = Url.Action("Index", "Chapters", new { storyId }) });
            }

            return RedirectToAction("Index", new { storyId });
        }
    }

    #region DTOs for Chapter Operations
    public class SaveDraftDto
    {
        public Guid ChapterId { get; set; }
        public Guid StoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int WordCount { get; set; }
        public int CharacterCount { get; set; }
        public int Version { get; set; }
        public string? Status { get; set; }
        public string? Summary { get; set; }
        public int? TargetWordCount { get; set; }
    }

    public class SaveWizardStepDto
    {
        public Guid? ChapterId { get; set; }
        public Guid StoryId { get; set; }
        public Guid? PartId { get; set; }
        public int Order { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? Status { get; set; } = "Planned";
        public int? TargetWordCount { get; set; }
        public string? Purpose { get; set; }
        public string? Goal { get; set; }
        public string? KeyEvents { get; set; }
        public string? EmotionalTone { get; set; }
        public string? PointOfView { get; set; }
    }

    public class SaveChapterDto
    {
        public Guid ChapterId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int WordCount { get; set; }
        public string? Status { get; set; }
    }

    public class LinkEntityDto
    {
        public Guid ChapterId { get; set; }
        public Guid EntityId { get; set; }
        public string? Role { get; set; }
    }

    public class QuickCreateCharacterDto
    {
        public Guid StoryId { get; set; }
        public Guid? ChapterId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Role { get; set; }
        public string? OneLineDescription { get; set; }
        public string? Age { get; set; }
        public string? Gender { get; set; }
        public string? Occupation { get; set; }
        public string? PersonalityTraits { get; set; }
        public string? Tags { get; set; }
        public string? AvatarUrl { get; set; }
        public Guid? TargetCharacterId { get; set; }
        public string? RelationshipType { get; set; }
        public bool ForceCreate { get; set; } = false;
    }

    public class QuickCreateWorldEntityDto
    {
        public Guid StoryId { get; set; }
        public Guid? ChapterId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TypeName { get; set; } = "Location";
        public string? Summary { get; set; }
        public string? Description { get; set; }
        public string? Importance { get; set; } = "Major";
        public string? Status { get; set; } = "Active";
        public Guid? ParentEntityId { get; set; }
        public string? CoverImage { get; set; }
        public string? RelationshipType { get; set; }
        public bool ForceCreate { get; set; } = false;
    }

    public class QuickCreateTimelineEventDto
    {
        public Guid StoryId { get; set; }
        public Guid? ChapterId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Category { get; set; } = "General";
        public string? EventType { get; set; } = "Standard";
        public string? StoryDate { get; set; }
        public string? Time { get; set; }
        public string? Description { get; set; }
        public string? Importance { get; set; } = "Medium";
        public Guid? StoryArcId { get; set; }
        public List<Guid>? CharacterIds { get; set; }
        public List<Guid>? WorldEntityIds { get; set; }
        public bool ForceCreate { get; set; } = false;
    }

    public class QuickCreateResearchNoteDto
    {
        public Guid StoryId { get; set; }
        public Guid? ChapterId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? Category { get; set; } = "General";
    }
    #endregion
}


