using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using StoryVerse.Core.Entities;

namespace StoryVerse.Web.Services
{
    public interface IActiveStoryService
    {
        Task<Guid?> GetActiveStoryIdAsync(HttpContext httpContext, string userId, Guid? requestedStoryId = null);
        Task<Story?> GetActiveStoryAsync(HttpContext httpContext, string userId, Guid? requestedStoryId = null);
        void SetActiveStoryId(HttpContext httpContext, Guid storyId);
        Task<List<Story>> GetUserStoriesAsync(string userId);
    }
}
