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

        // GET: WorldBuilding
        public async Task<IActionResult> Index(
            Guid? storyId, 
            string category = "All Entities", 
            Guid? typeId = null, 
            string? search = null, 
            string? status = null, 
            string sort = "Recently Updated", 
            string view = "grid",
            Guid? selectedEntityId = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var userStories = await _context.Stories
                .Where(s => s.UserId == user.Id)
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();

            Story? currentStory = null;
            if (storyId.HasValue && storyId.Value != Guid.Empty)
            {
                currentStory = userStories.FirstOrDefault(s => s.Id == storyId.Value);
            }

            if (currentStory == null && userStories.Any())
            {
                currentStory = userStories.FirstOrDefault(s => s.Status == "InProgress") ?? userStories.First();
            }

            var activeStoryId = currentStory?.Id ?? Guid.Empty;

            // Base query for user's story entities
            var entitiesQuery = _context.WorldEntities
                .Include(e => e.EntityType)
                .Include(e => e.CharacterLinks)
                .Include(e => e.TimelineLinks)
                .Where(e => e.StoryId == activeStoryId && e.ActiveStatus);

            // Compute counts
            var totalEntities = await entitiesQuery.CountAsync();
            var locationsCount = await entitiesQuery.CountAsync(e => e.EntityType.Category == "Locations");
            var organizationsCount = await entitiesQuery.CountAsync(e => e.EntityType.Category == "Organizations");
            var peopleGroupsCount = await entitiesQuery.CountAsync(e => e.EntityType.Category == "People Groups");
            var historicalEventsCount = await entitiesQuery.CountAsync(e => e.EntityType.Category == "Historical Events");

            // Category filter
            if (!string.IsNullOrEmpty(category) && category != "All Entities" && category != "More")
            {
                entitiesQuery = entitiesQuery.Where(e => e.EntityType.Category == category);
            }

            // EntityType filter
            if (typeId.HasValue && typeId.Value != Guid.Empty)
            {
                entitiesQuery = entitiesQuery.Where(e => e.EntityTypeId == typeId.Value);
            }

            // Status filter
            if (!string.IsNullOrEmpty(status) && status != "All Status")
            {
                entitiesQuery = entitiesQuery.Where(e => e.Status == status);
            }

            // Search query
            if (!string.IsNullOrEmpty(search))
            {
                var queryLower = search.ToLower();
                entitiesQuery = entitiesQuery.Where(e => 
                    e.Name.ToLower().Contains(queryLower) || 
                    (e.Summary != null && e.Summary.ToLower().Contains(queryLower)) ||
                    (e.EntityType.Name != null && e.EntityType.Name.ToLower().Contains(queryLower)));
            }

            // Sorting
            switch (sort)
            {
                case "Name (A-Z)":
                    entitiesQuery = entitiesQuery.OrderBy(e => e.Name);
                    break;
                case "Name (Z-A)":
                    entitiesQuery = entitiesQuery.OrderByDescending(e => e.Name);
                    break;
                case "Oldest":
                    entitiesQuery = entitiesQuery.OrderBy(e => e.CreatedDate);
                    break;
                case "Recently Updated":
                default:
                    entitiesQuery = entitiesQuery.OrderByDescending(e => e.UpdatedDate);
                    break;
            }

            var entityList = await entitiesQuery.ToListAsync();

            var entityItems = entityList.Select(e => new WorldEntityItemViewModel
            {
                Id = e.Id,
                StoryId = e.StoryId,
                Name = e.Name,
                Category = e.EntityType.Category,
                TypeName = e.EntityType.Name,
                Icon = !string.IsNullOrEmpty(e.Icon) ? e.Icon : e.EntityType.Icon,
                CoverImage = !string.IsNullOrEmpty(e.CoverImage) ? e.CoverImage : GetDefaultCoverImage(e.EntityType.Category),
                Summary = e.Summary ?? "No summary provided.",
                IsFavorite = e.IsFavorite,
                Status = e.Status,
                Importance = e.Importance,
                ConnectedCharactersCount = e.CharacterLinks.Count,
                ConnectedTimelineEventsCount = e.TimelineLinks.Count,
                UpdatedDate = e.UpdatedDate,
                UpdatedAgo = GetTimeAgo(e.UpdatedDate)
            }).ToList();

            var systemTypes = await _context.WorldEntityTypes
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();

            var suggestedTemplates = await _context.WorldTemplates.ToListAsync();

            // Selected entity detail for slide-out drawer
            WorldEntityDetailViewModel? detailVm = null;
            if (selectedEntityId.HasValue && selectedEntityId.Value != Guid.Empty)
            {
                detailVm = await FetchEntityDetailViewModel(selectedEntityId.Value);
            }
            else if (entityItems.Any())
            {
                detailVm = await FetchEntityDetailViewModel(entityItems.First().Id);
            }

            var viewModel = new WorldBuildingIndexViewModel
            {
                Story = currentStory,
                UserStories = userStories,
                SelectedStoryId = activeStoryId,
                TotalEntities = totalEntities,
                LocationsCount = locationsCount,
                OrganizationsCount = organizationsCount,
                PeopleGroupsCount = peopleGroupsCount,
                HistoricalEventsCount = historicalEventsCount,
                ActiveCategory = category,
                ActiveView = view,
                SearchQuery = search,
                SelectedTypeId = typeId,
                SelectedStatus = status,
                SortBy = sort,
                Entities = entityItems,
                SystemTypes = systemTypes,
                SuggestedTemplates = suggestedTemplates,
                SelectedEntityDetail = detailVm
            };

            return View(viewModel);
        }

        // GET: WorldBuilding/Create
        [HttpGet]
        public async Task<IActionResult> Create(Guid? storyId = null, Guid? typeId = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var userStories = await _context.Stories
                .Where(s => s.UserId == user.Id)
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();

            Story? currentStory = null;
            if (storyId.HasValue && storyId.Value != Guid.Empty)
            {
                currentStory = userStories.FirstOrDefault(s => s.Id == storyId.Value);
            }

            if (currentStory == null && userStories.Any())
            {
                currentStory = userStories.FirstOrDefault(s => s.Status == "InProgress") ?? userStories.First();
            }

            var activeStoryId = currentStory?.Id ?? Guid.Empty;

            var entityTypes = await _context.WorldEntityTypes
                .Include(t => t.Fields)
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();

            var selectedType = typeId.HasValue ? entityTypes.FirstOrDefault(t => t.Id == typeId.Value) : entityTypes.FirstOrDefault();

            var parentEntities = await _context.WorldEntities
                .Include(e => e.EntityType)
                .Where(e => e.StoryId == activeStoryId && e.ActiveStatus)
                .OrderBy(e => e.Name)
                .ToListAsync();

            var characters = await _context.Characters
                .Where(c => c.StoryId == activeStoryId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var viewModel = new CreateWorldEntityViewModel
            {
                SelectedStoryId = activeStoryId,
                SelectedTypeId = selectedType?.Id ?? Guid.Empty,
                UserStories = userStories,
                EntityTypes = entityTypes,
                ParentEntities = parentEntities,
                Characters = characters,
                DynamicFields = selectedType?.Fields.OrderBy(f => f.DisplayOrder).ToList() ?? new List<WorldEntityField>()
            };

            return View(viewModel);
        }

        // POST: WorldBuilding/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateWorldEntityInputModel model, Microsoft.AspNetCore.Http.IFormFile? CoverFile, List<string>? RelationshipTargetId, List<string>? RelationshipType, List<string>? RelationshipDescription, List<string>? LinkedCharacterId, List<string>? CharacterRole)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                TempData["Error"] = "Entity Name is required.";
                return RedirectToAction(nameof(Create), new { storyId = model.StoryId, typeId = model.EntityTypeId });
            }

            string? coverImagePath = model.CoverImage;
            if (CoverFile != null && CoverFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "covers");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(CoverFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await CoverFile.CopyToAsync(fileStream);
                }
                coverImagePath = "/covers/" + uniqueFileName;
            }

            var entity = new WorldEntity
            {
                Id = Guid.NewGuid(),
                StoryId = model.StoryId,
                EntityTypeId = model.EntityTypeId,
                ParentEntityId = model.ParentEntityId,
                Name = model.Name.Trim(),
                Summary = model.Summary,
                Description = model.Description,
                Status = string.IsNullOrEmpty(model.Status) ? "Active" : model.Status,
                Importance = string.IsNullOrEmpty(model.Importance) ? "Major" : model.Importance,
                Icon = !string.IsNullOrEmpty(model.Icon) ? model.Icon : "map-pin",
                CoverImage = coverImagePath,
                Tags = model.Tags,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                CreatedBy = user.UserName,
                ActiveStatus = true
            };

            _context.WorldEntities.Add(entity);

            // Fetch fields for this entity type to match standard detail field values
            var typeFields = await _context.WorldEntityFields.Where(f => f.EntityTypeId == model.EntityTypeId).ToListAsync();
            var detailMap = new Dictionary<string, string?>
            {
                { "Population", model.Population },
                { "Founded", model.Founded },
                { "Government", model.Government },
                { "Ruler", model.Ruler },
                { "Currency", model.Currency },
                { "Languages", model.Languages },
                { "Climate", model.Climate },
                { "Time Zone", model.TimeZone }
            };

            foreach (var kvp in detailMap)
            {
                if (!string.IsNullOrWhiteSpace(kvp.Value))
                {
                    var existingField = typeFields.FirstOrDefault(f => f.FieldName.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase));
                    if (existingField != null)
                    {
                        _context.WorldEntityValues.Add(new WorldEntityValue
                        {
                            Id = Guid.NewGuid(),
                            EntityId = entity.Id,
                            FieldId = existingField.Id,
                            Value = kvp.Value
                        });
                    }
                }
            }

            if (model.Fields != null && model.Fields.Any())
            {
                foreach (var fv in model.Fields)
                {
                    if (!string.IsNullOrWhiteSpace(fv.Value))
                    {
                        _context.WorldEntityValues.Add(new WorldEntityValue
                        {
                            Id = Guid.NewGuid(),
                            EntityId = entity.Id,
                            FieldId = fv.FieldId,
                            Value = fv.Value
                        });
                    }
                }
            }

            if (RelationshipTargetId != null && RelationshipTargetId.Any())
            {
                for (int i = 0; i < RelationshipTargetId.Count; i++)
                {
                    if (Guid.TryParse(RelationshipTargetId[i], out var targetId) && targetId != Guid.Empty)
                    {
                        var relType = (RelationshipType != null && i < RelationshipType.Count) ? RelationshipType[i] : "Located In";
                        var relDesc = (RelationshipDescription != null && i < RelationshipDescription.Count) ? RelationshipDescription[i] : "";

                        _context.WorldEntityRelationships.Add(new WorldEntityRelationship
                        {
                            Id = Guid.NewGuid(),
                            SourceEntityId = entity.Id,
                            TargetEntityId = targetId,
                            RelationshipType = relType,
                            Description = relDesc
                        });
                    }
                }
            }

            if (LinkedCharacterId != null && LinkedCharacterId.Any())
            {
                for (int i = 0; i < LinkedCharacterId.Count; i++)
                {
                    if (Guid.TryParse(LinkedCharacterId[i], out var charId) && charId != Guid.Empty)
                    {
                        var role = (CharacterRole != null && i < CharacterRole.Count) ? CharacterRole[i] : "Member";

                        _context.WorldEntityCharacters.Add(new WorldEntityCharacter
                        {
                            Id = Guid.NewGuid(),
                            EntityId = entity.Id,
                            CharacterId = charId,
                            RelationshipRole = role
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Entity '{entity.Name}' created successfully.";
            return RedirectToAction(nameof(Index), new { storyId = model.StoryId, selectedEntityId = entity.Id });
        }

        // GET: WorldBuilding/GetEntityDetails/5 (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetEntityDetails(Guid id)
        {
            var detail = await FetchEntityDetailViewModel(id);
            if (detail == null) return NotFound();
            return PartialView("_EntityDetailDrawer", detail);
        }

        // GET: WorldBuilding/GetTypeFields/5 (AJAX for create modal)
        [HttpGet]
        public async Task<IActionResult> GetTypeFields(Guid typeId)
        {
            var fields = await _context.WorldEntityFields
                .Where(f => f.EntityTypeId == typeId)
                .OrderBy(f => f.DisplayOrder)
                .Select(f => new { f.Id, f.FieldName, f.FieldType, f.Required, f.OptionsJson })
                .ToListAsync();
            return Json(fields);
        }

        // POST: WorldBuilding/CreateEntity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEntity(CreateWorldEntityInputModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                TempData["Error"] = "Entity Name is required.";
                return RedirectToAction(nameof(Index), new { storyId = model.StoryId });
            }

            var entity = new WorldEntity
            {
                Id = Guid.NewGuid(),
                StoryId = model.StoryId,
                EntityTypeId = model.EntityTypeId,
                ParentEntityId = model.ParentEntityId,
                Name = model.Name.Trim(),
                Summary = model.Summary,
                Description = model.Description,
                Status = string.IsNullOrEmpty(model.Status) ? "Active" : model.Status,
                Importance = string.IsNullOrEmpty(model.Importance) ? "Major" : model.Importance,
                Icon = model.Icon,
                CoverImage = model.CoverImage,
                Tags = model.Tags,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                CreatedBy = user.UserName,
                ActiveStatus = true
            };

            _context.WorldEntities.Add(entity);

            if (model.Fields != null && model.Fields.Any())
            {
                foreach (var fv in model.Fields)
                {
                    if (!string.IsNullOrWhiteSpace(fv.Value))
                    {
                        _context.WorldEntityValues.Add(new WorldEntityValue
                        {
                            Id = Guid.NewGuid(),
                            EntityId = entity.Id,
                            FieldId = fv.FieldId,
                            Value = fv.Value
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Entity '{entity.Name}' created successfully.";
            return RedirectToAction(nameof(Index), new { storyId = model.StoryId, selectedEntityId = entity.Id });
        }

        // POST: WorldBuilding/CreateEntityType
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEntityType(string name, string category, string icon, string? description, List<string> fieldNames, List<string> fieldTypes)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name required.");

            var type = new WorldEntityType
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                Category = string.IsNullOrEmpty(category) ? "Custom" : category,
                Icon = string.IsNullOrEmpty(icon) ? "folder" : icon,
                Description = description,
                IsSystemDefault = false,
                UserId = user.Id
            };

            _context.WorldEntityTypes.Add(type);

            if (fieldNames != null && fieldNames.Any())
            {
                for (int i = 0; i < fieldNames.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(fieldNames[i]))
                    {
                        _context.WorldEntityFields.Add(new WorldEntityField
                        {
                            Id = Guid.NewGuid(),
                            EntityTypeId = type.Id,
                            FieldName = fieldNames[i].Trim(),
                            FieldType = i < fieldTypes.Count ? fieldTypes[i] : "Text",
                            DisplayOrder = i + 1
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: WorldBuilding/ApplyTemplate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyTemplate(Guid templateId, Guid storyId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var template = await _context.WorldTemplates.FindAsync(templateId);
            if (template == null)
            {
                TempData["Error"] = "Template not found.";
                return RedirectToAction(nameof(Index), new { storyId });
            }

            // 1. Ensure Entity Types exist for this genre
            var genreTypes = new Dictionary<string, List<(string Name, string Category, string Icon, string Desc)>>
            {
                ["Fantasy"] = new()
                {
                    ("Kingdom",     "Locations",       "crown",     "A realm ruled by a monarch."),
                    ("City",        "Locations",       "building-2","A major urban settlement."),
                    ("Castle",      "Locations",       "castle",    "A fortified stronghold."),
                    ("Village",     "Locations",       "home",      "A small rural community."),
                    ("Guild",       "Organizations",   "award",     "An association of craftsmen or mages."),
                    ("Dynasty",     "People Groups",   "tree-pine", "A hereditary line of rulers."),
                    ("Faith",       "Cultures",        "sparkles",  "A religious belief system."),
                },
                ["Sci-Fi"] = new()
                {
                    ("Planet",        "Locations",     "globe",     "A celestial body."),
                    ("Space Station", "Locations",     "orbit",     "An artificial orbital outpost."),
                    ("Colony",        "Locations",     "building-2","A settled off-world community."),
                    ("Faction",       "People Groups", "users",     "A political or ideological group."),
                    ("Alien Species", "Species",       "bot",       "An extraterrestrial lifeform."),
                    ("Corporation",   "Organizations", "briefcase", "A mega-corporation or tech conglomerate."),
                },
                ["Contemporary"] = new()
                {
                    ("City",       "Locations",       "building-2","An urban metropolitan area."),
                    ("District",   "Locations",       "map-pin",   "A defined area within a city."),
                    ("Building",   "Locations",       "landmark",  "A notable structure or venue."),
                    ("Landmark",   "Locations",       "map",       "An iconic or significant location."),
                    ("Agency",     "Organizations",   "badge",     "A government or private agency."),
                },
                ["Historical"] = new()
                {
                    ("Empire",       "Locations",       "crown",    "A vast empire with territories."),
                    ("Dynasty",      "People Groups",   "tree-pine","A ruling family or bloodline."),
                    ("Military",     "Organizations",   "shield",   "Armed forces or standing army."),
                    ("Trade Route",  "Locations",       "route",    "A major trade or commerce path."),
                    ("Historical Event", "Historical Events", "history", "A defining past event."),
                },
                ["Crime Thriller"] = new()
                {
                    ("Police Station", "Organizations", "badge",    "Law enforcement headquarters."),
                    ("Crime Syndicate","Organizations", "skull",    "Underworld criminal organization."),
                    ("Courthouse",     "Locations",     "landmark", "A judicial building."),
                    ("Safe House",     "Locations",     "home",     "A hidden refuge."),
                    ("Gang",           "Organizations", "users",    "A street-level criminal gang."),
                },
            };

            if (genreTypes.TryGetValue(template.Genre, out var typesToAdd))
            {
                int order = await _context.WorldEntityTypes.MaxAsync(t => (int?)t.DisplayOrder) ?? 0;
                foreach (var (name, category, icon, desc) in typesToAdd)
                {
                    bool exists = await _context.WorldEntityTypes
                        .AnyAsync(t => t.Name == name && t.Category == category);
                    if (!exists)
                    {
                        order++;
                        _context.WorldEntityTypes.Add(new WorldEntityType
                        {
                            Id = Guid.NewGuid(),
                            Name = name,
                            Category = category,
                            Icon = icon,
                            Description = desc,
                            DisplayOrder = order,
                            IsSystemDefault = false,
                            UserId = user.Id
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            // Fetch all available types
            var allTypes = await _context.WorldEntityTypes.ToListAsync();

            // 2. Starter Entities definition map per genre
            var starterEntitiesMap = new Dictionary<string, List<(string Name, string TypeName, string Category, string Summary, string Description, string Status, string Importance, string Icon, string CoverImage, string Tags)>>
            {
                ["Crime Thriller"] = new()
                {
                    ("Metropolitan Police 1st Precinct", "Police Station", "Organizations", 
                     "Primary law enforcement headquarters managing city precincts and homicide divisions.",
                     "A heavily fortified multi-story station housing law enforcement, detective bureaus, forensics labs, and high-security holding cells.",
                     "Active", "Critical", "badge", "https://images.unsplash.com/photo-1541872703-74c5e44368f9?w=800&auto=format&fit=crop&q=80", "Police, LawEnforcement, Crime, Precinct"),

                    ("Viper Crime Syndicate", "Crime Syndicate", "Organizations", 
                     "Underworld criminal cartel controlling harbor docks, illegal gambling, and black markets.",
                     "Operating from hidden VIP lounges and industrial warehouses, the Viper Syndicate controls major illicit operations across districts.",
                     "Active", "Critical", "skull", "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=800&auto=format&fit=crop&q=80", "Syndicate, Underworld, Cartel, Mafia"),

                    ("Central District Courthouse", "Courthouse", "Locations", 
                     "Historic judicial building where high-profile crime trials and prosecutions occur.",
                     "A grand stone courthouse with classical pillars, surrounded by bail bond agencies and defense attorney offices.",
                     "Active", "Major", "landmark", "https://images.unsplash.com/photo-1461360370896-922624d12aa1?w=800&auto=format&fit=crop&q=80", "Justice, Legal, Trials, Courthouse"),

                    ("Harbor Dock 14 Safe House", "Safe House", "Locations", 
                     "Covert emergency refuge used by undercover detectives and high-value informants.",
                     "Disguised as a decaying shipping office, equipped with reinforced steel doors, encrypted comms, and a secret canal escape route.",
                     "Active", "Minor", "home", "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=800&auto=format&fit=crop&q=80", "Safehouse, Covert, Hideout, Undercover"),

                    ("Nightshade Street Syndicate", "Gang", "Organizations", 
                     "Aggressive street syndicate controlling the Eastside turf and black-market arms trades.",
                     "Known for tactical gear and dark street attire, operating out of underground combat clubs and alleyways.",
                     "Active", "Major", "users", "https://images.unsplash.com/photo-1541872703-74c5e44368f9?w=800&auto=format&fit=crop&q=80", "Gang, Street, Turf, Eastside")
                },
                ["Fantasy"] = new()
                {
                    ("Citadel of Aethelgard", "Kingdom", "Locations",
                     "Ancient royal stronghold sitting atop the High Peaks of the realm.",
                     "Impenetrable fortress of white stone and banners, ruling over five vassal provinces.",
                     "Active", "Critical", "crown", "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=800&auto=format&fit=crop&q=80", "Citadel, Castle, Royal, HighFantasy"),

                    ("Sunfire Capital City", "City", "Locations",
                     "Bustling trade metropolis with marble plazas and grand alchemy bazaars.",
                     "The vibrant heart of kingdom trade, commerce, and diplomatic emissaries.",
                     "Active", "Major", "building-2", "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=800&auto=format&fit=crop&q=80", "Capital, City, Bazaars, Trade"),

                    ("Order of Arcane Mages", "Guild", "Organizations",
                     "Prestigious council governing elemental magic and ancient spell archives.",
                     "A guild of sorcerers and scholars researching elemental runes and ancient seals.",
                     "Active", "Major", "award", "https://images.unsplash.com/photo-1541872703-74c5e44368f9?w=800&auto=format&fit=crop&q=80", "Mages, Magic, Guild, Arcane"),

                    ("House Sterling Dynasty", "Dynasty", "People Groups",
                     "Hereditary noble bloodline ruling over the eastern valleys for centuries.",
                     "Renowned for diplomatic wisdom and formidable knightly guards.",
                     "Active", "Major", "tree-pine", "https://images.unsplash.com/photo-1541872703-74c5e44368f9?w=800&auto=format&fit=crop&q=80", "Dynasty, Nobles, Royalty, Bloodline"),

                    ("The Sun Temple", "Faith", "Cultures",
                     "Holy sanctuary devoted to divine light, solar rites, and ancient prophecies.",
                     "A towering cathedral decorated with stained-glass solar windows and sacred altars.",
                     "Active", "Minor", "sparkles", "https://images.unsplash.com/photo-1519817650390-64a93db51149?w=800&auto=format&fit=crop&q=80", "Temple, Faith, Divine, Sacred")
                },
                ["Sci-Fi"] = new()
                {
                    ("Astra Prime Orbital Hub", "Space Station", "Locations",
                     "Intergalactic orbital station connecting trade ships and star fleets.",
                     "A sprawling artificial ring station offering docking bays, market decks, and atmospheric domes.",
                     "Active", "Critical", "orbit", "https://images.unsplash.com/photo-1451187580459-43490279c0fa?w=800&auto=format&fit=crop&q=80", "SpaceStation, Orbit, SciFi, Trading"),

                    ("Nexus-9 Mineral Planet", "Planet", "Locations",
                     "Off-world desert planet rich in rare plasma crystals and terraforming rigs.",
                     "Arid planet covered in glowing crystal ravines and deep subterranean colonies.",
                     "Active", "Major", "globe", "https://images.unsplash.com/photo-1451187580459-43490279c0fa?w=800&auto=format&fit=crop&q=80", "Planet, Mining, Crystals, OuterRim"),

                    ("Vanguard Outer-Rim Faction", "Faction", "People Groups",
                     "Coalition of independent pilots fighting for outer-system freedom.",
                     "Organized rebel group operating agile fighter fleets across deep space sectors.",
                     "Active", "Major", "users", "https://images.unsplash.com/photo-1541872703-74c5e44368f9?w=800&auto=format&fit=crop&q=80", "Faction, Rebels, Pilots, Vanguard"),

                    ("Zeta-5 Alien Species", "Alien Species", "Species",
                     "Ancient telepathic species with bioluminescent crystalline skin.",
                     "Intelligent alien species capable of psychic communication and grav-manipulation.",
                     "Active", "Major", "bot", "https://images.unsplash.com/photo-1534447677768-be436bb09401?w=800&auto=format&fit=crop&q=80", "Aliens, Telepathic, Species, SciFi"),

                    ("OmniCorp Cybernetics", "Corporation", "Organizations",
                     "Megacorporation manufacturing advanced neural AI and cybernetic gear.",
                     "Dominant corporate titan controlling tech patents, security drones, and synth research.",
                     "Active", "Critical", "briefcase", "https://images.unsplash.com/photo-1541872703-74c5e44368f9?w=800&auto=format&fit=crop&q=80", "Corporate, Cybernetics, AI, Tech")
                },
                ["Contemporary"] = new()
                {
                    ("Metropolitan Central City", "City", "Locations",
                     "Vibrant modern city featuring financial towers, parks, and culture districts.",
                     "A bustling metropolis housing millions, central transport hubs, and media networks.",
                     "Active", "Critical", "building-2", "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=800&auto=format&fit=crop&q=80", "City, Metro, Urban, Contemporary"),

                    ("Financial High-Rise District", "District", "Locations",
                     "Commercial hub of stock exchanges, corporate towers, and tech incubators.",
                     "Skyscrapers gleaming above busy avenues, bustling with executives and traders.",
                     "Active", "Major", "map-pin", "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=800&auto=format&fit=crop&q=80", "District, Finance, Business"),

                    ("Federal Intelligence Agency", "Agency", "Organizations",
                     "Government agency conducting covert operations and national security cases.",
                     "High-tech headquarters equipped with surveillance rooms and tactical field offices.",
                     "Active", "Major", "badge", "https://images.unsplash.com/photo-1541872703-74c5e44368f9?w=800&auto=format&fit=crop&q=80", "Agency, Intelligence, Government")
                },
                ["Historical"] = new()
                {
                    ("Sovereign Imperium Realm", "Empire", "Locations",
                     "Expansive historical empire covering vast valleys, provinces, and trade ports.",
                     "A mighty historical empire governed by imperial decrees and standing armies.",
                     "Active", "Critical", "crown", "https://images.unsplash.com/photo-1461360370896-922624d12aa1?w=800&auto=format&fit=crop&q=80", "Empire, Imperium, Historical, Sovereign"),

                    ("Valerius Imperial Dynasty", "Dynasty", "People Groups",
                     "Hereditary imperial lineage known for conquest and legal codices.",
                     "A centuries-old royal bloodline maintaining order through iron discipline.",
                     "Active", "Major", "tree-pine", "https://images.unsplash.com/photo-1541872703-74c5e44368f9?w=800&auto=format&fit=crop&q=80", "Dynasty, Imperial, History"),

                    ("Royal Imperial Guard", "Military", "Organizations",
                     "Elite military regiment protecting imperial borders and royal palaces.",
                     "Disciplined legion clad in traditional armor, guarding the Emperor and Senate.",
                     "Active", "Major", "shield", "https://images.unsplash.com/photo-1541872703-74c5e44368f9?w=800&auto=format&fit=crop&q=80", "Military, Guard, Army, Empire")
                }
            };

            int addedCount = 0;
            if (starterEntitiesMap.TryGetValue(template.Genre, out var entitiesToSeed))
            {
                foreach (var se in entitiesToSeed)
                {
                    bool exists = await _context.WorldEntities
                        .AnyAsync(e => e.StoryId == storyId && e.Name == se.Name && e.ActiveStatus);
                    if (!exists)
                    {
                        var matchingType = allTypes.FirstOrDefault(t => t.Name.Equals(se.TypeName, StringComparison.OrdinalIgnoreCase))
                                        ?? allTypes.FirstOrDefault(t => t.Category.Equals(se.Category, StringComparison.OrdinalIgnoreCase))
                                        ?? allTypes.First();

                        var newEntity = new WorldEntity
                        {
                            Id = Guid.NewGuid(),
                            StoryId = storyId,
                            EntityTypeId = matchingType.Id,
                            Name = se.Name,
                            Summary = se.Summary,
                            Description = se.Description,
                            Status = se.Status,
                            Importance = se.Importance,
                            Icon = se.Icon,
                            CoverImage = se.CoverImage,
                            Tags = se.Tags,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedDate = DateTime.UtcNow,
                            CreatedBy = user.UserName,
                            ActiveStatus = true
                        };

                        _context.WorldEntities.Add(newEntity);
                        addedCount++;
                    }
                }

                if (addedCount > 0)
                {
                    await _context.SaveChangesAsync();
                }
            }

            TempData["Success"] = $"✨ \"{template.Name}\" template applied! {addedCount} world entities created for your story.";
            return RedirectToAction(nameof(Index), new { storyId });
        }


        // POST: WorldBuilding/ToggleFavorite
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(Guid id)
        {
            var entity = await _context.WorldEntities.FindAsync(id);
            if (entity == null) return NotFound();

            entity.IsFavorite = !entity.IsFavorite;
            entity.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Json(new { success = true, isFavorite = entity.IsFavorite });
        }

        // POST: WorldBuilding/DeleteEntity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEntity(Guid id)
        {
            var entity = await _context.WorldEntities.FindAsync(id);
            if (entity == null) return NotFound();

            var storyId = entity.StoryId;
            entity.ActiveStatus = false;
            entity.DeletedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Entity deleted successfully.";
            return RedirectToAction(nameof(Index), new { storyId });
        }

        // Helper: Fetch detailed view model
        private async Task<WorldEntityDetailViewModel?> FetchEntityDetailViewModel(Guid entityId)
        {
            var entity = await _context.WorldEntities
                .Include(e => e.EntityType)
                    .ThenInclude(t => t.Fields)
                .Include(e => e.FieldValues)
                    .ThenInclude(fv => fv.Field)
                .Include(e => e.SourceRelationships)
                    .ThenInclude(r => r.TargetEntity)
                        .ThenInclude(te => te.EntityType)
                .Include(e => e.CharacterLinks)
                    .ThenInclude(cl => cl.Character)
                .Include(e => e.TimelineLinks)
                .FirstOrDefaultAsync(e => e.Id == entityId);

            if (entity == null) return null;

            var tagsList = !string.IsNullOrEmpty(entity.Tags)
                ? entity.Tags.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList()
                : new List<string>();

            // Default fields from type
            var fieldValuesList = new List<FieldValueItemViewModel>();
            if (entity.EntityType.Fields != null)
            {
                foreach (var f in entity.EntityType.Fields.OrderBy(x => x.DisplayOrder))
                {
                    var valObj = entity.FieldValues.FirstOrDefault(v => v.FieldId == f.Id);
                    fieldValuesList.Add(new FieldValueItemViewModel
                    {
                        FieldId = f.Id,
                        FieldName = f.FieldName,
                        FieldType = f.FieldType,
                        Value = valObj?.Value ?? "—",
                        Icon = GetFieldIcon(f.FieldName)
                    });
                }
            }

            // Additional dynamic values not covered by entity fields
            foreach (var fv in entity.FieldValues)
            {
                if (!fieldValuesList.Any(fvVm => fvVm.FieldId == fv.FieldId) && fv.Field != null)
                {
                    fieldValuesList.Add(new FieldValueItemViewModel
                    {
                        FieldId = fv.FieldId,
                        FieldName = fv.Field.FieldName,
                        FieldType = fv.Field.FieldType,
                        Value = fv.Value ?? "—",
                        Icon = GetFieldIcon(fv.Field.FieldName)
                    });
                }
            }

            var relationshipsList = entity.SourceRelationships.Select(r => new RelationshipItemViewModel
            {
                TargetEntityId = r.TargetEntityId,
                TargetEntityName = r.TargetEntity?.Name ?? "Unknown Entity",
                TargetTypeName = r.TargetEntity?.EntityType?.Name ?? "Entity",
                TargetIcon = r.TargetEntity?.Icon ?? r.TargetEntity?.EntityType?.Icon ?? "link",
                RelationshipType = r.RelationshipType,
                Description = r.Description ?? ""
            }).ToList();

            var characterLinksList = entity.CharacterLinks.Select(cl => new CharacterLinkItemViewModel
            {
                CharacterId = cl.CharacterId,
                CharacterName = cl.Character?.Name ?? "Unknown Character",
                AvatarUrl = cl.Character?.AvatarUrl ?? "/images/default-avatar.png",
                RelationshipRole = cl.RelationshipRole
            }).ToList();

            var timelineEventsList = entity.TimelineLinks.Select(tl => new TimelineLinkItemViewModel
            {
                Id = tl.Id,
                EventTitle = tl.EventTitle,
                EventDate = tl.EventDate ?? "",
                Description = tl.Description ?? ""
            }).ToList();

            return new WorldEntityDetailViewModel
            {
                Id = entity.Id,
                StoryId = entity.StoryId,
                Name = entity.Name,
                TypeName = entity.EntityType.Name,
                Category = entity.EntityType.Category,
                Icon = !string.IsNullOrEmpty(entity.Icon) ? entity.Icon : entity.EntityType.Icon,
                CoverImage = !string.IsNullOrEmpty(entity.CoverImage) ? entity.CoverImage : GetDefaultCoverImage(entity.EntityType.Category),
                Summary = entity.Summary ?? "",
                Description = entity.Description ?? "",
                Status = entity.Status,
                Importance = entity.Importance,
                IsFavorite = entity.IsFavorite,
                Quote = GetRandomQuoteForEntity(entity.Name, entity.EntityType.Category),
                FieldValues = fieldValuesList,
                Relationships = relationshipsList,
                Characters = characterLinksList,
                TimelineEvents = timelineEventsList,
                Tags = tagsList,
                UpdatedDate = entity.UpdatedDate
            };
        }

        private static string GetFieldIcon(string fieldName)
        {
            var lower = fieldName.ToLower();
            if (lower.Contains("ruler") || lower.Contains("leader") || lower.Contains("chief")) return "crown";
            if (lower.Contains("capital") || lower.Contains("city")) return "building-2";
            if (lower.Contains("government") || lower.Contains("jurisdiction")) return "landmark";
            if (lower.Contains("population")) return "users";
            if (lower.Contains("currency")) return "coins";
            if (lower.Contains("religion") || lower.Contains("faith")) return "sparkles";
            if (lower.Contains("climate") || lower.Contains("weather")) return "sun-medium";
            if (lower.Contains("coordinates") || lower.Contains("location")) return "map-pin";
            return "info";
        }

        private static string GetDefaultCoverImage(string category)
        {
            return category switch
            {
                "Locations" => "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=800&auto=format&fit=crop&q=80",
                "Organizations" => "https://images.unsplash.com/photo-1541872703-74c5e44368f9?w=800&auto=format&fit=crop&q=80",
                "Religions" => "https://images.unsplash.com/photo-1519817650390-64a93db51149?w=800&auto=format&fit=crop&q=80",
                "Species" => "https://images.unsplash.com/photo-1534447677768-be436bb09401?w=800&auto=format&fit=crop&q=80",
                "Historical Events" => "https://images.unsplash.com/photo-1461360370896-922624d12aa1?w=800&auto=format&fit=crop&q=80",
                _ => "https://images.unsplash.com/photo-1451187580459-43490279c0fa?w=800&auto=format&fit=crop&q=80"
            };
        }

        private static string GetRandomQuoteForEntity(string name, string category)
        {
            return category switch
            {
                "Locations" => $"\"Where the boundaries of {name} lie, the fate of empires is decided.\"",
                "Organizations" => $"\"Strength in unity. Honor in rule.\"",
                "Religions" => $"\"Followers believe in light, honor, and divine justice.\"",
                "Species" => $"\"Ancient bloodlines carrying untold power and wisdom.\"",
                _ => $"\"Every landmark carries a story waiting to be told.\""
            };
        }

        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;
            if (timeSpan.TotalMinutes < 1) return "just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 30) return $"{(int)timeSpan.TotalDays}d ago";
            return dateTime.ToString("MMM d");
        }
    }
}
