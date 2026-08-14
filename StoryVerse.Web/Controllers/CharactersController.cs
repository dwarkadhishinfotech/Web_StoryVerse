using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryVerse.Core.Entities;
using StoryVerse.Core.Entities.Identity;
using StoryVerse.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StoryVerse.Web.Controllers
{
    [Authorize]
    public class CharactersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly StoryVerse.Web.Services.IActiveStoryService _activeStoryService;

        public CharactersController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            StoryVerse.Web.Services.IActiveStoryService activeStoryService)
        {
            _context = context;
            _userManager = userManager;
            _activeStoryService = activeStoryService;
        }

        // GET: Characters?storyId=...&search=...&role=...&status=...&sortBy=...&arcType=...&page=1&pageSize=10
        public async Task<IActionResult> Index(Guid? storyId, string? search, string? role, string? status, string? sortBy, string? arcType, int page = 1, int pageSize = 10)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Fetch user's stories for filter dropdown
            var userStories = await _activeStoryService.GetUserStoriesAsync(user.Id);
            ViewBag.Stories = userStories;

            // Selected Story filter via ActiveStoryService
            var activeStoryIdGuid = await _activeStoryService.GetActiveStoryIdAsync(HttpContext, user.Id, storyId);
            
            Story? selectedStory = activeStoryIdGuid.HasValue
                ? userStories.FirstOrDefault(s => s.Id == activeStoryIdGuid.Value)
                : null;

            ViewBag.Story = selectedStory;
            var worldLocations = selectedStory != null
                ? await _context.Locations.AsNoTracking().Where(l => l.StoryId == selectedStory.Id).OrderBy(l => l.Name).ToListAsync()
                : new List<Location>();
            ViewBag.WorldLocations = worldLocations;

            // Base query for characters
            var query = _context.Characters
                .AsNoTracking()
                .Include(c => c.Story)
                .Where(c => c.Story.UserId == user.Id)
                .AsQueryable();

            // Apply Story Filter first so stats and cards reflect active story
            if (selectedStory != null)
            {
                query = query.Where(c => c.StoryId == selectedStory.Id);
            }

            // All characters for active story stats & relationships mapping
            var allUserCharacters = await query.ToListAsync();
            ViewBag.AllCharacters = allUserCharacters;

            // Calculate overall Stats for active story
            ViewBag.TotalCount = allUserCharacters.Count;
            ViewBag.MainCount = allUserCharacters.Count(c => (c.ArcType != null && c.ArcType.Equals("Main", StringComparison.OrdinalIgnoreCase)) || (c.Role != null && (c.Role.Contains("Protagonist", StringComparison.OrdinalIgnoreCase) || c.Role.Contains("Antagonist", StringComparison.OrdinalIgnoreCase))));
            ViewBag.SupportingCount = allUserCharacters.Count(c => (c.ArcType != null && c.ArcType.Equals("Supporting", StringComparison.OrdinalIgnoreCase)) || (c.Role != null && (c.Role.Contains("Mentor", StringComparison.OrdinalIgnoreCase) || c.Role.Contains("Supporter", StringComparison.OrdinalIgnoreCase) || c.Role.Contains("Supporting", StringComparison.OrdinalIgnoreCase))));
            ViewBag.MinorCount = allUserCharacters.Count(c => (c.ArcType != null && c.ArcType.Equals("Minor", StringComparison.OrdinalIgnoreCase)) || (c.Role != null && (c.Role.Contains("Minor", StringComparison.OrdinalIgnoreCase) || c.Role.Contains("Authority", StringComparison.OrdinalIgnoreCase))));
            ViewBag.RecentlyAddedCount = allUserCharacters.Count(c => c.CreatedAt >= DateTime.UtcNow.AddDays(-30));

            // Apply Search Filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var sLower = search.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(sLower) 
                    || (c.OneLineDescription != null && c.OneLineDescription.ToLower().Contains(sLower))
                    || (c.Role != null && c.Role.ToLower().Contains(sLower))
                    || (c.Nicknames != null && c.Nicknames.ToLower().Contains(sLower))
                    || (c.Occupation != null && c.Occupation.ToLower().Contains(sLower))
                    || (c.Tags != null && c.Tags.ToLower().Contains(sLower)));
            }

            // Apply Role Filter
            if (!string.IsNullOrWhiteSpace(role) && !role.Equals("All Roles", StringComparison.OrdinalIgnoreCase))
            {
                var rLower = role.Trim().ToLower();
                query = query.Where(c => (c.Role != null && c.Role.ToLower() == rLower)
                    || (c.ArcType != null && c.ArcType.ToLower() == rLower));
            }

            // Apply Status Filter
            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All Status", StringComparison.OrdinalIgnoreCase))
            {
                var stLower = status.Trim().ToLower();
                query = query.Where(c => c.Status != null && c.Status.ToLower() == stLower);
            }

            // Apply ArcType Filter
            if (!string.IsNullOrWhiteSpace(arcType) && !arcType.Equals("All Arcs", StringComparison.OrdinalIgnoreCase))
            {
                var aLower = arcType.Trim().ToLower();
                query = query.Where(c => c.ArcType != null && c.ArcType.ToLower() == aLower);
            }

            // Apply Sorting
            switch (sortBy?.ToLower())
            {
                case "name":
                    query = query.OrderBy(c => c.Name);
                    break;
                case "recently_added":
                    query = query.OrderByDescending(c => c.CreatedAt);
                    break;
                case "role":
                    query = query.OrderBy(c => c.Role);
                    break;
                case "status":
                    query = query.OrderBy(c => c.Status);
                    break;
                case "recently_updated":
                default:
                    query = query.OrderByDescending(c => c.UpdatedAt);
                    break;
            }

            var totalFilteredCount = await query.CountAsync();

            // Pagination
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            var totalPages = (int)Math.Ceiling(totalFilteredCount / (double)pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var charactersList = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentRole = role;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentArcType = arcType;
            ViewBag.CurrentSortBy = sortBy ?? "recently_updated";
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalFilteredCount = totalFilteredCount;

            return View(charactersList);
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

            var story = character.Story;

            // Fetch chapters count for this story
            var chapterCount = story != null
                ? await _context.Chapters.CountAsync(ch => ch.StoryId == story.Id)
                : 0;

            // Fetch recent chapters (for "Recent Appearances" section)
            var recentChapters = story != null
                ? await _context.Chapters
                    .Where(ch => ch.StoryId == story.Id)
                    .OrderByDescending(ch => ch.Order)
                    .Take(3)
                    .ToListAsync()
                : new List<Chapter>();

            // Fetch sibling characters for Relationships section
            var siblingCharacters = story != null
                ? await _context.Characters
                    .Where(c => c.StoryId == story.Id && c.Id != character.Id)
                    .OrderBy(c => c.Name)
                    .ToListAsync()
                : new List<Character>();

            ViewBag.Story = story;
            ViewBag.ChapterCount = chapterCount;
            ViewBag.RecentChapters = recentChapters;
            ViewBag.SiblingCharacters = siblingCharacters;
            ViewBag.CreatedByName = user.UserName ?? user.Email ?? "Author";

            return View(character);
        }

        // GET: Characters/Create?storyId=...
        public async Task<IActionResult> Create(Guid? storyId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var userStories = await _context.Stories
                .Where(s => s.UserId == user.Id)
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();

            ViewBag.Stories = userStories;

            Story? selectedStory = null;
            if (storyId.HasValue && storyId.Value != Guid.Empty)
            {
                selectedStory = userStories.FirstOrDefault(s => s.Id == storyId.Value);
            }
            if (selectedStory == null && userStories.Any())
            {
                selectedStory = userStories.First();
            }

            ViewBag.Story = selectedStory;
            var worldLocations = selectedStory != null
                ? await _context.Locations.Where(l => l.StoryId == selectedStory.Id).OrderBy(l => l.Name).ToListAsync()
                : new List<Location>();
            ViewBag.WorldLocations = worldLocations;

            var otherCharacters = selectedStory != null
                ? await _context.Characters.Where(c => c.StoryId == selectedStory.Id).OrderBy(c => c.Name).ToListAsync()
                : new List<Character>();
            ViewBag.OtherCharacters = otherCharacters;

            var character = new Character
            {
                StoryId = selectedStory?.Id ?? Guid.Empty,
                Role = "Protagonist" // Default role
            };

            return View(character);
        }

        // POST: Characters/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Character character)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var userStories = await _context.Stories
                .Where(s => s.UserId == user.Id)
                .ToListAsync();

            ViewBag.Stories = userStories;

            // Default StoryId if empty
            if (character.StoryId == Guid.Empty && userStories.Any())
            {
                character.StoryId = userStories.First().Id;
            }

            var story = userStories.FirstOrDefault(s => s.Id == character.StoryId);
            ViewBag.Story = story;

            if (story == null)
            {
                ModelState.AddModelError("StoryId", "Please select a valid story for this character.");
            }

            if (string.IsNullOrWhiteSpace(character.Name))
            {
                ModelState.AddModelError("Name", "Character Name is required.");
            }

            // Default fallbacks if empty
            if (string.IsNullOrWhiteSpace(character.Role)) character.Role = "Protagonist";
            if (string.IsNullOrWhiteSpace(character.ArcType)) character.ArcType = "Main";
            if (string.IsNullOrWhiteSpace(character.Status)) character.Status = "Active";

            // Only Name and StoryId are required. Clear any validation errors on optional fields.
            foreach (var key in ModelState.Keys.Where(k => k != nameof(character.Name) && k != nameof(character.StoryId)).ToList())
            {
                ModelState.Remove(key);
            }

            if (ModelState.IsValid)
            {
                character.Id = Guid.NewGuid();
                character.CreatedAt = DateTime.UtcNow;
                character.UpdatedAt = DateTime.UtcNow;

                EnsureNoNullStrings(character);

                _context.Characters.Add(character);

                // Auto-create locations and relationship characters if specified
                await EnsureWorldLocationExistsAsync(character.StoryId, character.PlaceOfBirth);
                await EnsureWorldLocationExistsAsync(character.StoryId, character.CurrentResidence);
                await EnsureRelationshipCharactersExistAsync(character.StoryId, character.RelationshipsJson, character.Name);

                // Log activity
                _context.ActivityLogs.Add(new ActivityLog
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    ActionType = "Character",
                    Description = $"Added Character: {character.Name} to story: {story?.Title ?? "Story"}",
                    RelatedEntityName = character.Name,
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Character '{character.Name}' created successfully!";

                // Redirect to the Character List Page
                return RedirectToAction(nameof(Index), new { storyId = character.StoryId });
            }

            var worldLocations = story != null
                ? await _context.Locations.Where(l => l.StoryId == story.Id).OrderBy(l => l.Name).ToListAsync()
                : new List<Location>();
            ViewBag.WorldLocations = worldLocations;

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

            var userStories = await _context.Stories
                .Where(s => s.UserId == user.Id)
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();

            ViewBag.Stories = userStories;
            ViewBag.Story = character.Story;
            var worldLocations = character.Story != null
                ? await _context.Locations.Where(l => l.StoryId == character.StoryId).OrderBy(l => l.Name).ToListAsync()
                : new List<Location>();
            ViewBag.WorldLocations = worldLocations;

            var otherCharacters = await _context.Characters
                .Where(c => c.StoryId == character.StoryId && c.Id != character.Id)
                .OrderBy(c => c.Name)
                .ToListAsync();
            ViewBag.OtherCharacters = otherCharacters;

            return View(character);
        }

        // POST: Characters/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Character character)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (id != character.Id) return NotFound();

            var existingCharacter = await _context.Characters
                .Include(c => c.Story)
                .FirstOrDefaultAsync(c => c.Id == id && c.Story.UserId == user.Id);

            if (existingCharacter == null) return NotFound();

            var userStories = await _context.Stories
                .Where(s => s.UserId == user.Id)
                .ToListAsync();
            ViewBag.Stories = userStories;

            var story = userStories.FirstOrDefault(s => s.Id == character.StoryId);
            if (story == null)
            {
                ModelState.AddModelError("StoryId", "Please select a valid story for this character.");
            }
            ViewBag.Story = story;

            if (string.IsNullOrWhiteSpace(character.Name))
            {
                ModelState.AddModelError("Name", "Character Name is required.");
            }

            if (string.IsNullOrWhiteSpace(character.Role)) character.Role = "Protagonist";
            if (string.IsNullOrWhiteSpace(character.ArcType)) character.ArcType = "Main";
            if (string.IsNullOrWhiteSpace(character.Status)) character.Status = "Active";

            // Only Name and StoryId are required. Clear any validation errors on optional fields.
            foreach (var key in ModelState.Keys.Where(k => k != nameof(character.Name) && k != nameof(character.StoryId)).ToList())
            {
                ModelState.Remove(key);
            }

            if (ModelState.IsValid)
            {
                // Update ALL editable character properties
                existingCharacter.StoryId = character.StoryId;
                existingCharacter.Name = character.Name;
                existingCharacter.Role = character.Role;
                existingCharacter.ArcType = character.ArcType;
                existingCharacter.Nicknames = character.Nicknames ?? string.Empty;
                existingCharacter.Age = character.Age ?? string.Empty;
                existingCharacter.Gender = character.Gender ?? string.Empty;
                existingCharacter.Pronouns = character.Pronouns ?? string.Empty;
                existingCharacter.Occupation = character.Occupation ?? string.Empty;
                existingCharacter.Status = character.Status;
                existingCharacter.Alignment = character.Alignment ?? string.Empty;
                existingCharacter.OneLineDescription = character.OneLineDescription ?? string.Empty;
                existingCharacter.BackgroundSummary = character.BackgroundSummary ?? string.Empty;
                existingCharacter.Tags = character.Tags ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(character.AvatarUrl))
                    existingCharacter.AvatarUrl = character.AvatarUrl;

                existingCharacter.Height = character.Height ?? string.Empty;
                existingCharacter.Build = character.Build ?? string.Empty;
                existingCharacter.Complexion = character.Complexion ?? string.Empty;
                existingCharacter.EyeColor = character.EyeColor ?? string.Empty;
                existingCharacter.HairColor = character.HairColor ?? string.Empty;
                existingCharacter.HairStyle = character.HairStyle ?? string.Empty;
                existingCharacter.DistinguishingFeatures = character.DistinguishingFeatures ?? string.Empty;
                existingCharacter.ClothingStyle = character.ClothingStyle ?? string.Empty;
                existingCharacter.PreferredColors = character.PreferredColors ?? string.Empty;
                existingCharacter.Accessories = character.Accessories ?? string.Empty;
                existingCharacter.VoiceTone = character.VoiceTone ?? string.Empty;
                existingCharacter.Accent = character.Accent ?? string.Empty;
                existingCharacter.SpeechPattern = character.SpeechPattern ?? string.Empty;
                existingCharacter.AppearanceNotes = character.AppearanceNotes ?? string.Empty;

                existingCharacter.PersonalityTraits = character.PersonalityTraits ?? string.Empty;
                existingCharacter.PersonalityOverview = character.PersonalityOverview ?? string.Empty;
                existingCharacter.ValuesBeliefs = character.ValuesBeliefs ?? string.Empty;
                existingCharacter.Strengths = character.Strengths ?? string.Empty;
                existingCharacter.Motivations = character.Motivations ?? string.Empty;
                existingCharacter.Temperament = character.Temperament ?? string.Empty;
                existingCharacter.Flaws = character.Flaws ?? string.Empty;
                existingCharacter.Fears = character.Fears ?? string.Empty;
                existingCharacter.Desires = character.Desires ?? string.Empty;

                existingCharacter.PlaceOfBirth = character.PlaceOfBirth ?? string.Empty;
                existingCharacter.DateOfBirth = character.DateOfBirth ?? string.Empty;
                existingCharacter.Nationality = character.Nationality ?? string.Empty;
                existingCharacter.FamilyBackground = character.FamilyBackground ?? string.Empty;
                existingCharacter.Upbringing = character.Upbringing ?? string.Empty;
                existingCharacter.Education = character.Education ?? string.Empty;
                existingCharacter.KeyEvents = character.KeyEvents ?? string.Empty;
                existingCharacter.Backstory = character.Backstory ?? string.Empty;
                existingCharacter.SocioeconomicStatus = character.SocioeconomicStatus ?? string.Empty;
                existingCharacter.CurrentResidence = character.CurrentResidence ?? string.Empty;
                existingCharacter.Languages = character.Languages ?? string.Empty;
                existingCharacter.BackgroundDocumentUrl = character.BackgroundDocumentUrl ?? string.Empty;

                existingCharacter.Allies = character.Allies ?? string.Empty;
                existingCharacter.Enemies = character.Enemies ?? string.Empty;
                existingCharacter.Family = character.Family ?? string.Empty;
                existingCharacter.LoveInterests = character.LoveInterests ?? string.Empty;
                existingCharacter.RelationshipsJson = character.RelationshipsJson ?? string.Empty;
                existingCharacter.RelationshipChartUrl = character.RelationshipChartUrl ?? string.Empty;

                existingCharacter.AuthorNotes = character.AuthorNotes ?? string.Empty;
                existingCharacter.FamilyCrest = character.FamilyCrest ?? string.Empty;
                existingCharacter.ThemeColor = character.ThemeColor ?? string.Empty;
                existingCharacter.CustomDocumentUrl = character.CustomDocumentUrl ?? string.Empty;

                existingCharacter.UpdatedAt = DateTime.UtcNow;

                await EnsureWorldLocationExistsAsync(existingCharacter.StoryId, existingCharacter.PlaceOfBirth);
                await EnsureWorldLocationExistsAsync(existingCharacter.StoryId, existingCharacter.CurrentResidence);
                await EnsureRelationshipCharactersExistAsync(existingCharacter.StoryId, existingCharacter.RelationshipsJson, existingCharacter.Name);

                // Log activity
                _context.ActivityLogs.Add(new ActivityLog
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    ActionType = "Character",
                    Description = $"Updated character info: {existingCharacter.Name}",
                    RelatedEntityName = existingCharacter.Name,
                    Timestamp = DateTime.UtcNow
                });

                EnsureNoNullStrings(existingCharacter);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Character '{existingCharacter.Name}' updated successfully!";

                // Redirect to the Character List Page
                return RedirectToAction(nameof(Index), new { storyId = existingCharacter.StoryId });
            }

            var worldLocations = story != null
                ? await _context.Locations.Where(l => l.StoryId == story.Id).OrderBy(l => l.Name).ToListAsync()
                : new List<Location>();
            ViewBag.WorldLocations = worldLocations;

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
            var characterName = character.Name;
            _context.Characters.Remove(character);

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ActionType = "Character",
                Description = $"Removed character: {characterName}",
                RelatedEntityName = characterName,
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Character '{characterName}' deleted successfully!";

            // Redirect to the Character List Page
            return RedirectToAction(nameof(Index), new { storyId = storyId });
        }

        // POST: Characters/UploadAvatar
        [HttpPost]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { success = false, message = "User not logged in." });

            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "No file selected." });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
            {
                return Json(new { success = false, message = "Invalid image format. Allowed: JPG, PNG, GIF, WEBP." });
            }

            var uploadsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "characters");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = System.IO.Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var avatarUrl = $"/uploads/characters/{fileName}";
            return Json(new { success = true, avatarUrl });
        }

        // POST: Characters/UploadBackgroundDoc
        [HttpPost]
        public async Task<IActionResult> UploadBackgroundDoc(IFormFile file)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { success = false, message = "User not logged in." });

            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "No file selected." });
            }

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".rtf", ".png", ".jpg", ".jpeg", ".webp" };
            var ext = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
            {
                return Json(new { success = false, message = "Invalid document format. Allowed: PDF, DOC, DOCX, TXT, RTF, images." });
            }

            var uploadsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "background_docs");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var originalName = System.IO.Path.GetFileName(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = System.IO.Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var documentUrl = $"/uploads/background_docs/{fileName}";
            return Json(new { success = true, documentUrl, fileName = originalName, fileSize = file.Length });
        }

        // POST: Characters/UploadRelationshipDoc
        [HttpPost]
        public async Task<IActionResult> UploadRelationshipDoc(IFormFile file)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { success = false, message = "User not logged in." });

            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "No file selected." });
            }

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".rtf", ".png", ".jpg", ".jpeg", ".webp", ".svg" };
            var ext = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
            {
                return Json(new { success = false, message = "Invalid document format. Allowed: PDF, DOC, DOCX, TXT, RTF, images, SVG." });
            }

            var uploadsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "relationship_docs");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var originalName = System.IO.Path.GetFileName(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = System.IO.Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var documentUrl = $"/uploads/relationship_docs/{fileName}";
            return Json(new { success = true, documentUrl, fileName = originalName, fileSize = file.Length });
        }

        // POST: Characters/UploadCustomDoc
        [HttpPost]
        public async Task<IActionResult> UploadCustomDoc(IFormFile file)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { success = false, message = "User not logged in." });

            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "No file selected." });
            }

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".rtf", ".png", ".jpg", ".jpeg", ".webp", ".svg" };
            var ext = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
            {
                return Json(new { success = false, message = "Invalid document format. Allowed: PDF, DOC, DOCX, TXT, RTF, images, SVG." });
            }

            var uploadsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "custom_docs");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var originalName = System.IO.Path.GetFileName(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = System.IO.Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var documentUrl = $"/uploads/custom_docs/{fileName}";
            return Json(new { success = true, documentUrl, fileName = originalName, fileSize = file.Length });
        }

        // GET: Characters/GetWorldLocations?storyId=...
        [HttpGet]
        public async Task<IActionResult> GetWorldLocations(Guid storyId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var locations = await _context.Locations
                .Where(l => l.StoryId == storyId && l.Story.UserId == user.Id)
                .OrderBy(l => l.Name)
                .Select(l => new { id = l.Id, name = l.Name })
                .ToListAsync();

            return Json(locations);
        }

        private async Task EnsureWorldLocationExistsAsync(Guid storyId, string? locationName)
        {
            if (string.IsNullOrWhiteSpace(locationName) || storyId == Guid.Empty) return;

            var nameTrimmed = locationName.Trim();

            if (nameTrimmed.Equals("None", StringComparison.OrdinalIgnoreCase) ||
                nameTrimmed.Equals("Unknown", StringComparison.OrdinalIgnoreCase) ||
                nameTrimmed.StartsWith("Wanderer", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var exists = await _context.Locations
                .AnyAsync(l => l.StoryId == storyId && l.Name.ToLower() == nameTrimmed.ToLower());

            if (!exists)
            {
                _context.Locations.Add(new Location
                {
                    Id = Guid.NewGuid(),
                    StoryId = storyId,
                    Name = nameTrimmed,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        private static void EnsureNoNullStrings(Character character)
        {
            character.Name ??= string.Empty;
            character.Role ??= "Protagonist";
            character.ArcType ??= "Main";
            character.Nicknames ??= string.Empty;
            character.Age ??= string.Empty;
            character.Gender ??= string.Empty;
            character.Pronouns ??= string.Empty;
            character.Occupation ??= string.Empty;
            character.Status ??= "Active";
            character.Alignment ??= string.Empty;
            character.OneLineDescription ??= string.Empty;
            character.BackgroundSummary ??= string.Empty;
            character.Tags ??= string.Empty;
            character.AvatarUrl ??= string.Empty;

            character.Height ??= string.Empty;
            character.Build ??= string.Empty;
            character.Complexion ??= string.Empty;
            character.EyeColor ??= string.Empty;
            character.HairColor ??= string.Empty;
            character.HairStyle ??= string.Empty;
            character.DistinguishingFeatures ??= string.Empty;
            character.ClothingStyle ??= string.Empty;
            character.PreferredColors ??= string.Empty;
            character.Accessories ??= string.Empty;
            character.VoiceTone ??= string.Empty;
            character.Accent ??= string.Empty;
            character.SpeechPattern ??= string.Empty;
            character.AppearanceNotes ??= string.Empty;

            character.PersonalityTraits ??= string.Empty;
            character.PersonalityOverview ??= string.Empty;
            character.ValuesBeliefs ??= string.Empty;
            character.Strengths ??= string.Empty;
            character.Motivations ??= string.Empty;
            character.Temperament ??= string.Empty;
            character.Flaws ??= string.Empty;
            character.Fears ??= string.Empty;
            character.Desires ??= string.Empty;

            character.PlaceOfBirth ??= string.Empty;
            character.DateOfBirth ??= string.Empty;
            character.Nationality ??= string.Empty;
            character.FamilyBackground ??= string.Empty;
            character.Upbringing ??= string.Empty;
            character.Education ??= string.Empty;
            character.KeyEvents ??= string.Empty;
            character.Backstory ??= string.Empty;
            character.SocioeconomicStatus ??= string.Empty;
            character.CurrentResidence ??= string.Empty;
            character.Languages ??= string.Empty;
            character.BackgroundDocumentUrl ??= string.Empty;

            character.Allies ??= string.Empty;
            character.Enemies ??= string.Empty;
            character.Family ??= string.Empty;
            character.LoveInterests ??= string.Empty;
            character.RelationshipsJson ??= string.Empty;
            character.RelationshipChartUrl ??= string.Empty;

            character.AuthorNotes ??= string.Empty;
            character.FamilyCrest ??= string.Empty;
            character.ThemeColor ??= string.Empty;
            character.CustomDocumentUrl ??= string.Empty;
        }

        private async Task EnsureRelationshipCharactersExistAsync(Guid storyId, string? relationshipsJson, string currentCharacterName)
        {
            if (string.IsNullOrWhiteSpace(relationshipsJson)) return;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(relationshipsJson);
                if (doc.RootElement.ValueKind != System.Text.Json.JsonValueKind.Array) return;

                foreach (var elem in doc.RootElement.EnumerateArray())
                {
                    string? relName = null;
                    if (elem.TryGetProperty("name", out var pName) && !string.IsNullOrWhiteSpace(pName.GetString()))
                        relName = pName.GetString();
                    else if (elem.TryGetProperty("targetName", out var pTName) && !string.IsNullOrWhiteSpace(pTName.GetString()))
                        relName = pTName.GetString();
                    else if (elem.TryGetProperty("TargetName", out var pTName2) && !string.IsNullOrWhiteSpace(pTName2.GetString()))
                        relName = pTName2.GetString();

                    if (string.IsNullOrWhiteSpace(relName)) continue;
                    relName = relName.Trim();

                    // Check if character already exists in story
                    var rNameLower = relName.ToLower();
                    bool exists = await _context.Characters.AnyAsync(c => c.StoryId == storyId && c.Name.ToLower() == rNameLower);
                    if (!exists)
                    {
                        string? avatar = null;
                        if (elem.TryGetProperty("avatar", out var pAv) && !string.IsNullOrWhiteSpace(pAv.GetString()))
                            avatar = pAv.GetString();
                        else if (elem.TryGetProperty("avatarUrl", out var pAvUrl) && !string.IsNullOrWhiteSpace(pAvUrl.GetString()))
                            avatar = pAvUrl.GetString();

                        string? role = null;
                        if (elem.TryGetProperty("role", out var pRole) && !string.IsNullOrWhiteSpace(pRole.GetString()))
                            role = pRole.GetString();

                        string? relationshipType = null;
                        if (elem.TryGetProperty("relationship", out var pRel) && !string.IsNullOrWhiteSpace(pRel.GetString()))
                            relationshipType = pRel.GetString();
                        else if (elem.TryGetProperty("relationType", out var pRelType) && !string.IsNullOrWhiteSpace(pRelType.GetString()))
                            relationshipType = pRelType.GetString();

                        var newRelChar = new Character
                        {
                            Id = Guid.NewGuid(),
                            StoryId = storyId,
                            Name = relName,
                            AvatarUrl = avatar ?? string.Empty,
                            Role = string.IsNullOrWhiteSpace(role) ? "Supporting" : role,
                            ArcType = "Supporting",
                            Status = "Active",
                            OneLineDescription = !string.IsNullOrWhiteSpace(relationshipType) ? $"{relationshipType} of {currentCharacterName}" : $"Related to {currentCharacterName}",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        EnsureNoNullStrings(newRelChar);
                        _context.Characters.Add(newRelChar);
                    }
                }
            }
            catch
            {
                // Silently skip if JSON parsing fails
            }
        }
    }
}
