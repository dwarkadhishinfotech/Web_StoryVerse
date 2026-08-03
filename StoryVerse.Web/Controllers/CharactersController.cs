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

        // GET: Characters?storyId=...&search=...&role=...&status=...&sortBy=...
        public async Task<IActionResult> Index(Guid? storyId, string? search, string? role, string? status, string? sortBy)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Fetch user's stories for filter dropdown
            var userStories = await _context.Stories
                .Where(s => s.UserId == user.Id)
                .OrderBy(s => s.Title)
                .ToListAsync();

            ViewBag.Stories = userStories;

            // Selected Story filter
            Story? selectedStory = null;
            if (storyId.HasValue && storyId.Value != Guid.Empty)
            {
                selectedStory = userStories.FirstOrDefault(s => s.Id == storyId.Value);
            }
            ViewBag.Story = selectedStory;
            var worldLocations = selectedStory != null
                ? await _context.Locations.Where(l => l.StoryId == selectedStory.Id).OrderBy(l => l.Name).ToListAsync()
                : new List<Location>();
            ViewBag.WorldLocations = worldLocations;

            // Base query for characters
            var query = _context.Characters
                .Include(c => c.Story)
                .Where(c => c.Story.UserId == user.Id)
                .AsQueryable();

            // All characters for overall stats & relationships mapping
            var allUserCharacters = await query.ToListAsync();
            ViewBag.AllCharacters = allUserCharacters;

            // Calculate overall DB Stats
            ViewBag.TotalCount = allUserCharacters.Count;
            ViewBag.MainCount = allUserCharacters.Count(c => (c.ArcType != null && c.ArcType.Equals("Main", StringComparison.OrdinalIgnoreCase)) || (c.Role != null && (c.Role.Contains("Protagonist", StringComparison.OrdinalIgnoreCase) || c.Role.Contains("Antagonist", StringComparison.OrdinalIgnoreCase))));
            ViewBag.SupportingCount = allUserCharacters.Count(c => (c.ArcType != null && c.ArcType.Equals("Supporting", StringComparison.OrdinalIgnoreCase)) || (c.Role != null && (c.Role.Contains("Mentor", StringComparison.OrdinalIgnoreCase) || c.Role.Contains("Supporter", StringComparison.OrdinalIgnoreCase) || c.Role.Contains("Supporting", StringComparison.OrdinalIgnoreCase))));
            ViewBag.MinorCount = allUserCharacters.Count(c => (c.ArcType != null && c.ArcType.Equals("Minor", StringComparison.OrdinalIgnoreCase)) || (c.Role != null && (c.Role.Contains("Minor", StringComparison.OrdinalIgnoreCase) || c.Role.Contains("Authority", StringComparison.OrdinalIgnoreCase))));
            ViewBag.RecentlyAddedCount = allUserCharacters.Count(c => c.CreatedAt >= DateTime.UtcNow.AddDays(-30));

            // Apply Story Filter
            if (selectedStory != null)
            {
                query = query.Where(c => c.StoryId == selectedStory.Id);
            }

            // Apply Search Filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var sLower = search.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(sLower) 
                    || (c.OneLineDescription != null && c.OneLineDescription.ToLower().Contains(sLower))
                    || (c.Role != null && c.Role.ToLower().Contains(sLower)));
            }

            // Apply Role Filter
            if (!string.IsNullOrWhiteSpace(role) && !role.Equals("All Roles", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(c => (c.Role != null && c.Role.Equals(role, StringComparison.OrdinalIgnoreCase))
                    || (c.ArcType != null && c.ArcType.Equals(role, StringComparison.OrdinalIgnoreCase)));
            }

            // Apply Status Filter
            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All Status", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(c => c.Status != null && c.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
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
                case "recently_updated":
                default:
                    query = query.OrderByDescending(c => c.UpdatedAt);
                    break;
            }

            var charactersList = await query.ToListAsync();

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentRole = role;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSortBy = sortBy ?? "recently_updated";

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

            ViewBag.Story = character.Story;
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
            var character = new Character
            {
                StoryId = selectedStory?.Id ?? Guid.Empty
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

            if (string.IsNullOrWhiteSpace(character.Role))
            {
                ModelState.AddModelError("Role", "Role in Story is required.");
            }

            if (ModelState.IsValid)
            {
                character.Id = Guid.NewGuid();
                character.CreatedAt = DateTime.UtcNow;
                character.UpdatedAt = DateTime.UtcNow;

                _context.Characters.Add(character);

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
                return RedirectToAction(nameof(Index), new { storyId = character.StoryId });
            }

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
    }
}


