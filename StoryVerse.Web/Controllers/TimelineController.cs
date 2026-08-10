using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryVerse.Core.Entities;
using StoryVerse.Core.Entities.Identity;
using StoryVerse.Infrastructure.Data;
using StoryVerse.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace StoryVerse.Web.Controllers
{
    [Authorize]
    public class TimelineController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public TimelineController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // GET: /Timeline?storyId=...&activeTab=...&viewMode=...
        public async Task<IActionResult> Index(Guid? storyId, string activeTab = "TimelineView", string viewMode = "Vertical", string category = "All Events", string search = "")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var userStories = await _context.Stories
                .Where(s => s.UserId == user.Id)
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();

            if (!userStories.Any())
            {
                // Create a default story if none exists
                var defaultStory = new Story
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Title = "Revenge for Love",
                    Genre = "Romance • Mystery",
                    Status = "InProgress",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Stories.Add(defaultStory);
                await _context.SaveChangesAsync();
                userStories.Add(defaultStory);
            }

            var currentStory = storyId.HasValue
                ? userStories.FirstOrDefault(s => s.Id == storyId.Value) ?? userStories.First()
                : userStories.First();

            var storyGuid = currentStory.Id;

            // Load Timeline Events
            var eventsQuery = _context.TimelineEvents
                .Include(e => e.CharacterLinks).ThenInclude(cl => cl.Character)
                .Include(e => e.WorldEntityLinks).ThenInclude(wl => wl.WorldEntity)
                .Include(e => e.ChapterLinks).ThenInclude(chl => chl.Chapter)
                .Where(e => e.StoryId == storyGuid);

            if (!string.IsNullOrWhiteSpace(category) && category != "All Events")
            {
                eventsQuery = eventsQuery.Where(e => e.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLower();
                eventsQuery = eventsQuery.Where(e => e.Title.ToLower().Contains(q) || (e.Summary != null && e.Summary.ToLower().Contains(q)) || (e.LocationName != null && e.LocationName.ToLower().Contains(q)));
            }

            var timelineEvents = await eventsQuery.OrderBy(e => e.DisplayOrder).ThenBy(e => e.CreatedAt).ToListAsync();

            // Load Story Arcs
            var storyArcs = await _context.StoryArcs
                .Include(sa => sa.ArcEvents)
                .Where(sa => sa.StoryId == storyGuid)
                .OrderBy(sa => sa.DisplayOrder)
                .ToListAsync();

            // Load Available Related Entities
            var characters = await _context.Characters.Where(c => c.StoryId == storyGuid).ToListAsync();
            var worldEntities = await _context.WorldEntities.Include(w => w.EntityType).Where(w => w.StoryId == storyGuid).ToListAsync();
            var chapters = await _context.Chapters.Where(c => c.StoryId == storyGuid).OrderBy(c => c.Order).ToListAsync();
            var researchNotes = await _context.ResearchNotes.Where(r => r.StoryId == storyGuid).ToListAsync();
            var assets = await _context.Assets.Where(a => a.StoryId == storyGuid).ToListAsync();

            // Transform DTOs
            var eventDtos = timelineEvents.Select(e => MapToEventDto(e)).ToList();

            var arcDtos = storyArcs.Select(a => new StoryArcDto
            {
                Id = a.Id,
                Title = a.Title,
                ArcType = a.ArcType,
                Color = a.Color ?? "#13A8A6",
                ProgressPercent = a.TargetCompletionPercent,
                EventCount = a.ArcEvents.Count
            }).ToList();

            var upcomingEvents = eventDtos
                .Where(e => e.RealDate.HasValue && e.RealDate.Value >= DateTime.UtcNow)
                .OrderBy(e => e.RealDate)
                .Take(5)
                .ToList();

            // Load Story Timelines from DB
            var dbTimelines = await _context.StoryTimelines
                .Include(st => st.LinkedStoryArcs).ThenInclude(lsa => lsa.StoryArc)
                .Where(st => st.StoryId == storyGuid)
                .OrderByDescending(st => st.UpdatedAt)
                .ToListAsync();

            var timelineDtos = dbTimelines.Select(st => new TimelineDto
            {
                Id = st.Id,
                StoryId = st.StoryId,
                StoryTitle = currentStory.Title,
                Name = st.Name,
                Description = st.Description,
                Color = st.Color,
                StartDate = st.StartDate,
                EndDate = st.EndDate,
                Tags = st.Tags,
                CoverImageUrl = st.CoverImageUrl,
                Status = st.Status,
                TimelineType = st.TimelineType,
                DateFormat = st.DateFormat,
                TimeFormat = st.TimeFormat,
                DefaultTime = st.DefaultTime,
                CalendarStartDay = st.CalendarStartDay,
                TimeZone = st.TimeZone,
                DefaultTimelineView = st.DefaultTimelineView,
                EventGrouping = st.EventGrouping,
                ShowTimeOnTimeline = st.ShowTimeOnTimeline,
                ShowEventIcons = st.ShowEventIcons,
                ShowEventDescriptions = st.ShowEventDescriptions,
                CompactMode = st.CompactMode,
                AllowOverlappingEvents = st.AllowOverlappingEvents,
                EnableTimelineDependencies = st.EnableTimelineDependencies,
                AutoSortNewEvents = st.AutoSortNewEvents,
                EnableReminders = st.EnableReminders,
                LockTimelineDates = st.LockTimelineDates,
                ShowFutureEvents = st.ShowFutureEvents,
                ShowCompletedEvents = st.ShowCompletedEvents,
                EventCount = timelineEvents.Count,
                StoryArcCount = st.LinkedStoryArcs.Count,
                CreatedAt = st.CreatedAt,
                UpdatedAt = st.UpdatedAt,
                LinkedStoryArcs = st.LinkedStoryArcs.Select(l => new StoryArcOptionDto
                {
                    Id = l.StoryArc.Id,
                    Title = l.StoryArc.Title,
                    ArcType = l.StoryArc.ArcType,
                    Description = l.StoryArc.Description,
                    Color = l.StoryArc.Color ?? "#13A8A6",
                    ProgressPercent = l.StoryArc.TargetCompletionPercent,
                    IsSelected = true
                }).ToList()
            }).ToList();

            var selectedTimeline = timelineDtos.FirstOrDefault();

            var model = new TimelineStudioViewModel
            {
                SelectedStoryId = storyGuid,
                StoryTitle = currentStory.Title,
                StoryGenre = currentStory.Genre,
                Stories = userStories.Select(s => new StoryOptionDto { Id = s.Id, Title = s.Title }).ToList(),
                SelectedTimelineId = selectedTimeline?.Id,
                Timelines = timelineDtos,

                TotalEventsCount = timelineEvents.Count,
                UpcomingEventsCount = upcomingEvents.Count,
                HistoricalEventsCount = timelineEvents.Count(e => e.Category == "Historical" || e.Category == "Birth" || e.Category == "Backstory"),
                StoryArcsCount = storyArcs.Count,
                CharactersInvolvedCount = timelineEvents.SelectMany(e => e.CharacterLinks).Select(c => c.CharacterId).Distinct().Count(),
                LocationsInvolvedCount = timelineEvents.SelectMany(e => e.WorldEntityLinks).Select(w => w.WorldEntityId).Distinct().Count(),

                ActiveTab = activeTab,
                ViewMode = viewMode,
                SelectedCategory = category,
                SearchQuery = search,

                Events = eventDtos,
                StoryArcs = arcDtos,
                UpcomingEvents = upcomingEvents,

                Characters = characters.Select(c => new CharacterOptionDto { Id = c.Id, Name = c.Name, Role = c.Role, AvatarUrl = c.AvatarUrl }).ToList(),
                WorldEntities = worldEntities.Select(w => new WorldEntityOptionDto { Id = w.Id, Name = w.Name, TypeName = w.EntityType.Name, Icon = w.Icon }).ToList(),
                Chapters = chapters.Select(c => new ChapterOptionDto { Id = c.Id, Title = c.Title, Order = c.Order }).ToList(),
                ResearchNotes = researchNotes.Select(r => new ResearchOptionDto { Id = r.Id, Title = r.Title, Category = r.Category }).ToList(),
                Assets = assets.Select(a => new AssetOptionDto { Id = a.Id, Title = a.Title, Type = a.Type }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid? storyId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var userStories = await _context.Stories
                .Where(s => s.UserId == user.Id)
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();

            if (!userStories.Any())
            {
                var defaultStory = new Story
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Title = "Revenge for Love",
                    Genre = "Romance • Mystery",
                    Status = "InProgress",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Stories.Add(defaultStory);
                await _context.SaveChangesAsync();
                userStories.Add(defaultStory);
            }

            var selectedStory = storyId.HasValue
                ? userStories.FirstOrDefault(s => s.Id == storyId.Value) ?? userStories.First()
                : userStories.First();

            var storyArcs = await _context.StoryArcs
                .Include(sa => sa.ArcEvents)
                .Where(sa => sa.StoryId == selectedStory.Id)
                .OrderBy(sa => sa.DisplayOrder)
                .ToListAsync();

            var model = new TimelineFormViewModel
            {
                IsEdit = false,
                StoryId = selectedStory.Id,
                StoryTitle = selectedStory.Title,
                Stories = userStories.Select(s => new StoryOptionDto { Id = s.Id, Title = s.Title }).ToList(),
                AvailableStoryArcs = storyArcs.Select(a => new StoryArcOptionDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    ArcType = a.ArcType,
                    Description = a.Description,
                    Color = a.Color ?? "#13A8A6",
                    ProgressPercent = a.TargetCompletionPercent,
                    IsSelected = false
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TimelineFormViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Timeline name is required.");
            }

            if (!ModelState.IsValid)
            {
                var userStories = await _context.Stories.Where(s => s.UserId == user.Id).ToListAsync();
                model.Stories = userStories.Select(s => new StoryOptionDto { Id = s.Id, Title = s.Title }).ToList();
                var storyArcs = await _context.StoryArcs.Where(sa => sa.StoryId == model.StoryId).ToListAsync();
                model.AvailableStoryArcs = storyArcs.Select(a => new StoryArcOptionDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    ArcType = a.ArcType,
                    Description = a.Description,
                    Color = a.Color ?? "#13A8A6",
                    ProgressPercent = a.TargetCompletionPercent,
                    IsSelected = model.SelectedStoryArcIds.Contains(a.Id)
                }).ToList();
                return View(model);
            }

            string? coverUrl = null;
            if (model.BannerFile != null && model.BannerFile.Length > 0)
            {
                coverUrl = await SaveUploadedFileAsync(model.BannerFile);
            }

            var timeline = new StoryTimeline
            {
                Id = Guid.NewGuid(),
                StoryId = model.StoryId,
                Name = model.Name.Trim(),
                Description = model.Description?.Trim(),
                Color = string.IsNullOrWhiteSpace(model.Color) ? "Teal" : model.Color,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Tags = model.Tags?.Trim(),
                CoverImageUrl = coverUrl,
                Status = "Active",
                TimelineType = string.IsNullOrWhiteSpace(model.TimelineType) ? "Chronological Timeline" : model.TimelineType,
                DateFormat = model.DateFormat ?? "DD MMM YYYY (31 Dec 2025)",
                TimeFormat = model.TimeFormat ?? "12 Hour (AM/PM)",
                DefaultTime = model.DefaultTime ?? "12:00 PM",
                CalendarStartDay = model.CalendarStartDay ?? "Monday",
                TimeZone = model.TimeZone ?? "(GMT+05:30) Asia/Kolkata",
                DefaultTimelineView = model.DefaultTimelineView ?? "Chronological Timeline",
                EventGrouping = model.EventGrouping ?? "Group by Date",
                ShowTimeOnTimeline = model.ShowTimeOnTimeline,
                ShowEventIcons = model.ShowEventIcons,
                ShowEventDescriptions = model.ShowEventDescriptions,
                CompactMode = model.CompactMode,
                AllowOverlappingEvents = model.AllowOverlappingEvents,
                EnableTimelineDependencies = model.EnableTimelineDependencies,
                AutoSortNewEvents = model.AutoSortNewEvents,
                EnableReminders = model.EnableReminders,
                LockTimelineDates = model.LockTimelineDates,
                ShowFutureEvents = model.ShowFutureEvents,
                ShowCompletedEvents = model.ShowCompletedEvents,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (model.SelectedStoryArcIds != null && model.SelectedStoryArcIds.Any())
            {
                foreach (var arcId in model.SelectedStoryArcIds)
                {
                    timeline.LinkedStoryArcs.Add(new TimelineStoryArc
                    {
                        Id = Guid.NewGuid(),
                        StoryTimelineId = timeline.Id,
                        StoryArcId = arcId
                    });
                }
            }

            _context.StoryTimelines.Add(timeline);
            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId = user.Id,
                ActionType = "Timeline",
                Description = $"Created timeline '{timeline.Name}'",
                RelatedEntityName = timeline.Name,
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Timeline '{timeline.Name}' created successfully!";
            return RedirectToAction(nameof(Index), new { storyId = model.StoryId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var timeline = await _context.StoryTimelines
                .Include(st => st.LinkedStoryArcs)
                .FirstOrDefaultAsync(st => st.Id == id);

            if (timeline == null) return NotFound();

            var userStories = await _context.Stories
                .Where(s => s.UserId == user.Id)
                .ToListAsync();

            var storyArcs = await _context.StoryArcs
                .Where(sa => sa.StoryId == timeline.StoryId)
                .ToListAsync();

            var linkedArcIds = timeline.LinkedStoryArcs.Select(l => l.StoryArcId).ToList();

            var model = new TimelineFormViewModel
            {
                IsEdit = true,
                TimelineId = timeline.Id,
                StoryId = timeline.StoryId,
                StoryTitle = userStories.FirstOrDefault(s => s.Id == timeline.StoryId)?.Title ?? "",
                Name = timeline.Name,
                Description = timeline.Description,
                Color = timeline.Color,
                StartDate = timeline.StartDate,
                EndDate = timeline.EndDate,
                Tags = timeline.Tags,
                CoverImageUrl = timeline.CoverImageUrl,
                TimelineType = timeline.TimelineType,
                DateFormat = timeline.DateFormat,
                TimeFormat = timeline.TimeFormat,
                DefaultTime = timeline.DefaultTime,
                CalendarStartDay = timeline.CalendarStartDay,
                TimeZone = timeline.TimeZone,
                DefaultTimelineView = timeline.DefaultTimelineView,
                EventGrouping = timeline.EventGrouping,
                ShowTimeOnTimeline = timeline.ShowTimeOnTimeline,
                ShowEventIcons = timeline.ShowEventIcons,
                ShowEventDescriptions = timeline.ShowEventDescriptions,
                CompactMode = timeline.CompactMode,
                AllowOverlappingEvents = timeline.AllowOverlappingEvents,
                EnableTimelineDependencies = timeline.EnableTimelineDependencies,
                AutoSortNewEvents = timeline.AutoSortNewEvents,
                EnableReminders = timeline.EnableReminders,
                LockTimelineDates = timeline.LockTimelineDates,
                ShowFutureEvents = timeline.ShowFutureEvents,
                ShowCompletedEvents = timeline.ShowCompletedEvents,
                SelectedStoryArcIds = linkedArcIds,
                Stories = userStories.Select(s => new StoryOptionDto { Id = s.Id, Title = s.Title }).ToList(),
                AvailableStoryArcs = storyArcs.Select(a => new StoryArcOptionDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    ArcType = a.ArcType,
                    Description = a.Description,
                    Color = a.Color ?? "#13A8A6",
                    ProgressPercent = a.TargetCompletionPercent,
                    IsSelected = linkedArcIds.Contains(a.Id)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, TimelineFormViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var timeline = await _context.StoryTimelines
                .Include(st => st.LinkedStoryArcs)
                .FirstOrDefaultAsync(st => st.Id == id);

            if (timeline == null) return NotFound();

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Timeline name is required.");
            }

            if (!ModelState.IsValid)
            {
                var userStories = await _context.Stories.Where(s => s.UserId == user.Id).ToListAsync();
                model.IsEdit = true;
                model.TimelineId = id;
                model.Stories = userStories.Select(s => new StoryOptionDto { Id = s.Id, Title = s.Title }).ToList();
                var storyArcs = await _context.StoryArcs.Where(sa => sa.StoryId == model.StoryId).ToListAsync();
                model.AvailableStoryArcs = storyArcs.Select(a => new StoryArcOptionDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    ArcType = a.ArcType,
                    Description = a.Description,
                    Color = a.Color ?? "#13A8A6",
                    ProgressPercent = a.TargetCompletionPercent,
                    IsSelected = model.SelectedStoryArcIds.Contains(a.Id)
                }).ToList();
                return View(model);
            }

            if (model.BannerFile != null && model.BannerFile.Length > 0)
            {
                timeline.CoverImageUrl = await SaveUploadedFileAsync(model.BannerFile);
            }

            timeline.StoryId = model.StoryId;
            timeline.Name = model.Name.Trim();
            timeline.Description = model.Description?.Trim();
            timeline.Color = string.IsNullOrWhiteSpace(model.Color) ? "Teal" : model.Color;
            timeline.StartDate = model.StartDate;
            timeline.EndDate = model.EndDate;
            timeline.Tags = model.Tags?.Trim();
            timeline.TimelineType = string.IsNullOrWhiteSpace(model.TimelineType) ? "Chronological Timeline" : model.TimelineType;
            timeline.DateFormat = model.DateFormat ?? "DD MMM YYYY (31 Dec 2025)";
            timeline.TimeFormat = model.TimeFormat ?? "12 Hour (AM/PM)";
            timeline.DefaultTime = model.DefaultTime ?? "12:00 PM";
            timeline.CalendarStartDay = model.CalendarStartDay ?? "Monday";
            timeline.TimeZone = model.TimeZone ?? "(GMT+05:30) Asia/Kolkata";
            timeline.DefaultTimelineView = model.DefaultTimelineView ?? "Chronological Timeline";
            timeline.EventGrouping = model.EventGrouping ?? "Group by Date";
            timeline.ShowTimeOnTimeline = model.ShowTimeOnTimeline;
            timeline.ShowEventIcons = model.ShowEventIcons;
            timeline.ShowEventDescriptions = model.ShowEventDescriptions;
            timeline.CompactMode = model.CompactMode;
            timeline.AllowOverlappingEvents = model.AllowOverlappingEvents;
            timeline.EnableTimelineDependencies = model.EnableTimelineDependencies;
            timeline.AutoSortNewEvents = model.AutoSortNewEvents;
            timeline.EnableReminders = model.EnableReminders;
            timeline.LockTimelineDates = model.LockTimelineDates;
            timeline.ShowFutureEvents = model.ShowFutureEvents;
            timeline.ShowCompletedEvents = model.ShowCompletedEvents;
            timeline.UpdatedAt = DateTime.UtcNow;

            // Sync Linked Story Arcs
            _context.TimelineStoryArcs.RemoveRange(timeline.LinkedStoryArcs);
            if (model.SelectedStoryArcIds != null && model.SelectedStoryArcIds.Any())
            {
                foreach (var arcId in model.SelectedStoryArcIds)
                {
                    timeline.LinkedStoryArcs.Add(new TimelineStoryArc
                    {
                        Id = Guid.NewGuid(),
                        StoryTimelineId = timeline.Id,
                        StoryArcId = arcId
                    });
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Timeline '{timeline.Name}' updated successfully!";
            return RedirectToAction(nameof(Index), new { storyId = model.StoryId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTimeline(Guid id)
        {
            var timeline = await _context.StoryTimelines.FindAsync(id);
            if (timeline == null) return NotFound(new { success = false, message = "Timeline not found." });

            var storyId = timeline.StoryId;
            _context.StoryTimelines.Remove(timeline);
            await _context.SaveChangesAsync();

            return Json(new { success = true, storyId = storyId, message = "Timeline deleted successfully." });
        }

        private async Task<string> SaveUploadedFileAsync(IFormFile file)
        {
            var uploadFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "timelines");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return $"/uploads/timelines/{uniqueFileName}";
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] TimelineEventInputModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Title))
                return BadRequest(new { success = false, message = "Event title is required." });

            var story = await _context.Stories.FindAsync(model.StoryId);
            if (story == null) return NotFound(new { success = false, message = "Story not found." });

            var newEvent = new TimelineEvent
            {
                Id = Guid.NewGuid(),
                StoryId = model.StoryId,
                Title = model.Title,
                Summary = model.Summary,
                Description = model.Description,
                Category = string.IsNullOrWhiteSpace(model.Category) ? "General" : model.Category,
                EventType = model.EventType,
                RealDate = model.RealDate,
                StoryDate = model.StoryDate,
                LocationName = model.LocationName,
                Importance = model.Importance,
                Color = model.Color ?? GetDefaultCategoryColor(model.Category),
                Icon = model.Icon ?? GetDefaultCategoryIcon(model.Category),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (model.SelectedCharacterIds != null)
            {
                foreach (var charId in model.SelectedCharacterIds)
                {
                    newEvent.CharacterLinks.Add(new TimelineCharacter
                    {
                        Id = Guid.NewGuid(),
                        TimelineEventId = newEvent.Id,
                        CharacterId = charId,
                        Role = "Participant"
                    });
                }
            }

            if (model.SelectedWorldEntityIds != null)
            {
                foreach (var worldId in model.SelectedWorldEntityIds)
                {
                    newEvent.WorldEntityLinks.Add(new TimelineWorldEntity
                    {
                        Id = Guid.NewGuid(),
                        TimelineEventId = newEvent.Id,
                        WorldEntityId = worldId,
                        Role = "Location"
                    });
                }
            }

            if (model.SelectedChapterIds != null)
            {
                foreach (var chapId in model.SelectedChapterIds)
                {
                    newEvent.ChapterLinks.Add(new TimelineEventChapter
                    {
                        Id = Guid.NewGuid(),
                        TimelineEventId = newEvent.Id,
                        ChapterId = chapId
                    });
                }
            }

            _context.TimelineEvents.Add(newEvent);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = newEvent.Id, message = "Timeline event created successfully." });
        }

        [HttpGet]
        public async Task<IActionResult> GetEventDetails(Guid id)
        {
            var timelineEvent = await _context.TimelineEvents
                .Include(e => e.CharacterLinks).ThenInclude(cl => cl.Character)
                .Include(e => e.WorldEntityLinks).ThenInclude(wl => wl.WorldEntity)
                .Include(e => e.ChapterLinks).ThenInclude(chl => chl.Chapter)
                .Include(e => e.ResearchLinks).ThenInclude(rl => rl.ResearchNote)
                .Include(e => e.AssetLinks).ThenInclude(al => al.Asset)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (timelineEvent == null) return NotFound();

            var dto = MapToEventDto(timelineEvent);
            return Json(dto);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleBookmark(Guid id)
        {
            var evt = await _context.TimelineEvents.FindAsync(id);
            if (evt == null) return NotFound();

            evt.IsBookmarked = !evt.IsBookmarked;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isBookmarked = evt.IsBookmarked });
        }

        [HttpPost]
        public async Task<IActionResult> CreateStoryArc([FromBody] StoryArcInputModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Title))
                return BadRequest(new { success = false, message = "Arc title is required." });

            var arc = new StoryArc
            {
                Id = Guid.NewGuid(),
                StoryId = model.StoryId,
                Title = model.Title,
                ArcType = model.ArcType,
                Description = model.Description,
                Color = model.Color ?? "#13A8A6",
                TargetCompletionPercent = model.TargetCompletionPercent,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.StoryArcs.Add(arc);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = arc.Id, message = "Story arc created successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            var evt = await _context.TimelineEvents.FindAsync(id);
            if (evt == null) return NotFound();

            _context.TimelineEvents.Remove(evt);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Event deleted." });
        }

        [HttpGet]
        public IActionResult AiAnalyzeTimeline(Guid storyId, string type)
        {
            // Placeholder response for AI Timeline Readiness
            return Json(new
            {
                status = "Ready",
                feature = type,
                analysis = type switch
                {
                    "Summary" => "Story timeline covers a span of 6 key chronological milestones. Pacing is steady across Acts 1 and 2.",
                    "ConflictDetection" => "No chronological conflicts detected between character births and story events.",
                    "AgeValidation" => "Character ages are consistent across all assigned timeline events.",
                    "PlotHoles" => "All major plot setup events lead to logical consequences in later chapters.",
                    "Health" => "94% Timeline Consistency Score",
                    _ => "Analysis completed successfully."
                }
            });
        }

        private static TimelineEventDto MapToEventDto(TimelineEvent e)
        {
            var storyDate = e.StoryDate ?? (e.RealDate.HasValue ? e.RealDate.Value.ToString("dd MMM yyyy") : string.Empty);
            var parts = storyDate.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            string dateYear = string.Empty;
            string dateMonthDay = string.Empty;

            if (parts.Length >= 3)
            {
                dateYear = parts[^1];
                dateMonthDay = $"{parts[1].ToUpper()} {parts[0]}";
            }
            else if (parts.Length == 2)
            {
                dateYear = parts[1];
                dateMonthDay = parts[0].ToUpper();
            }
            else
            {
                dateYear = e.CreatedAt.ToString("yyyy");
                dateMonthDay = e.CreatedAt.ToString("MMM dd").ToUpper();
            }

            return new TimelineEventDto
            {
                Id = e.Id,
                StoryId = e.StoryId,
                Title = e.Title,
                Summary = e.Summary,
                Description = e.Description,
                Category = e.Category,
                EventType = e.EventType,
                RealDate = e.RealDate,
                StoryDate = storyDate,
                DateYear = dateYear,
                DateMonthDay = dateMonthDay,
                LocationName = e.LocationName ?? (e.WorldEntityLinks.FirstOrDefault()?.WorldEntity?.Name ?? string.Empty),
                Importance = e.Importance,
                Status = e.Status,
                Color = e.Color ?? GetDefaultCategoryColor(e.Category),
                Icon = e.Icon ?? GetDefaultCategoryIcon(e.Category),
                IsBookmarked = e.IsBookmarked,
                Characters = e.CharacterLinks.Select(cl => new CharacterOptionDto
                {
                    Id = cl.Character.Id,
                    Name = cl.Character.Name,
                    Role = cl.Character.Role,
                    AvatarUrl = cl.Character.AvatarUrl
                }).ToList(),
                WorldEntities = e.WorldEntityLinks.Select(wl => new WorldEntityOptionDto
                {
                    Id = wl.WorldEntity.Id,
                    Name = wl.WorldEntity.Name,
                    TypeName = wl.WorldEntity.EntityType?.Name ?? "Location",
                    Icon = wl.WorldEntity.Icon
                }).ToList(),
                Chapters = e.ChapterLinks.Select(chl => new ChapterOptionDto
                {
                    Id = chl.Chapter.Id,
                    Title = chl.Chapter.Title,
                    Order = chl.Chapter.Order
                }).ToList()
            };
        }

        private static string GetDefaultCategoryColor(string category)
        {
            return category?.ToLower() switch
            {
                "birth" => "#E6F4EA", // Mint green background
                "meeting" => "#FCE8E6", // Soft pink background
                "career" => "#FEF7E0", // Soft yellow background
                "investigation" => "#F3E8FD", // Soft purple background
                "incident" => "#FCE8E6", // Soft orange/coral
                "battle" => "#FCE8E6",
                "journey" => "#E8F0FE", // Soft blue
                _ => "#F1F5F9"
            };
        }

        private static string GetDefaultCategoryIcon(string category)
        {
            return category?.ToLower() switch
            {
                "birth" => "baby",
                "meeting" => "heart",
                "career" => "shield",
                "investigation" => "zap",
                "incident" => "sun",
                "battle" => "swords",
                "journey" => "compass",
                "discovery" => "lightbulb",
                _ => "calendar"
            };
        }
    }
}
