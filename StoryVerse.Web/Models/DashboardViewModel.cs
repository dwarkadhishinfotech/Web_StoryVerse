using System;
using System.Collections.Generic;
using StoryVerse.Core.Entities;

namespace StoryVerse.Web.Models
{
    public class DashboardViewModel
    {
        public Story ActiveStory { get; set; } = null!;
        public List<Story> RecentStories { get; set; } = new List<Story>();
        public List<ActivityLog> RecentActivities { get; set; } = new List<ActivityLog>();
        
        public int TotalWords { get; set; }
        public int ActiveStoriesCount { get; set; }
        public int CharactersCount { get; set; }
        public int LocationsCount { get; set; }
        
        public UserGoal UserGoal { get; set; } = null!;
        
        public int GoalProgressPercentage => UserGoal != null && UserGoal.DailyWordCountGoal > 0 
            ? (int)Math.Clamp(((double)UserGoal.WordsWrittenToday / UserGoal.DailyWordCountGoal) * 100, 0, 100)
            : 0;
            
        public int ActiveStoryProgressPercentage => ActiveStory != null && ActiveStory.TargetWordCount > 0 
            ? (int)Math.Clamp(((double)ActiveStory.CurrentWordCount / ActiveStory.TargetWordCount) * 100, 0, 100)
            : 0;
    }
}
