using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StoryVerse.Core.Entities;
using StoryVerse.Infrastructure.Data;

namespace StoryVerse.Web.Services
{
    public class ActiveStoryService : IActiveStoryService
    {
        public const string CookieName = "StoryVerse_ActiveStoryId";
        private readonly ApplicationDbContext _context;

        public ActiveStoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid?> GetActiveStoryIdAsync(HttpContext httpContext, string userId, Guid? requestedStoryId = null)
        {
            if (string.IsNullOrEmpty(userId)) return null;

            // 1. Check HttpContext items cache first
            if (httpContext.Items.TryGetValue("ActiveStoryId", out var cachedObj) && cachedObj is Guid cachedGuid)
            {
                if (!requestedStoryId.HasValue || requestedStoryId.Value == Guid.Empty || requestedStoryId.Value == cachedGuid)
                {
                    return cachedGuid;
                }
            }

            // 2. If explicit requestedStoryId is provided
            if (requestedStoryId.HasValue && requestedStoryId.Value != Guid.Empty)
            {
                var storyExists = await _context.Stories.AsNoTracking().AnyAsync(s => s.Id == requestedStoryId.Value && s.UserId == userId);
                if (storyExists)
                {
                    SetActiveStoryId(httpContext, requestedStoryId.Value);
                    return requestedStoryId.Value;
                }
            }

            // 3. Check Cookie
            if (httpContext.Request.Cookies.TryGetValue(CookieName, out var cookieVal) && Guid.TryParse(cookieVal, out var cookieGuid))
            {
                var storyExists = await _context.Stories.AsNoTracking().AnyAsync(s => s.Id == cookieGuid && s.UserId == userId);
                if (storyExists)
                {
                    httpContext.Items["ActiveStoryId"] = cookieGuid;
                    return cookieGuid;
                }
            }

            // 4. Fallback: Find in-progress story or latest updated story
            var activeStoryId = await _context.Stories
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.Status == "InProgress")
                .ThenByDescending(s => s.UpdatedAt)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            if (activeStoryId != Guid.Empty)
            {
                SetActiveStoryId(httpContext, activeStoryId);
                return activeStoryId;
            }

            return null;
        }

        public async Task<Story?> GetActiveStoryAsync(HttpContext httpContext, string userId, Guid? requestedStoryId = null)
        {
            var storyId = await GetActiveStoryIdAsync(httpContext, userId, requestedStoryId);
            if (!storyId.HasValue) return null;

            return await _context.Stories
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == storyId.Value && s.UserId == userId);
        }

        public void SetActiveStoryId(HttpContext httpContext, Guid storyId)
        {
            httpContext.Items["ActiveStoryId"] = storyId;
            httpContext.Response.Cookies.Append(CookieName, storyId.ToString(), new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(365),
                HttpOnly = false,
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });
        }

        public async Task<List<Story>> GetUserStoriesAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return new List<Story>();

            return await _context.Stories
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();
        }
    }
}
