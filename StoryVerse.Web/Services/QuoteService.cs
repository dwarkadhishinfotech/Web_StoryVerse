using System;
using System.Collections.Generic;
using System.Linq;

namespace StoryVerse.Web.Services
{
    public class QuoteService : IQuoteService
    {
        private static readonly List<QuoteModel> Quotes = new List<QuoteModel>
        {
            new QuoteModel { Content = "You don't have to be great to start, but you have to start to be great.", Author = "Zig Ziglar" },
            new QuoteModel { Content = "You can always edit a bad page. You can't edit a blank page.", Author = "Jodi Picoult" },
            new QuoteModel { Content = "Start before you're ready.", Author = "Steven Pressfield" },
            new QuoteModel { Content = "A word after a word after a word is power.", Author = "Margaret Atwood" },
            new QuoteModel { Content = "Tear the mask off your imagination and show the world your vision.", Author = "Ray Bradbury" },
            new QuoteModel { Content = "Write what should not be forgotten.", Author = "Isabel Allende" },
            new QuoteModel { Content = "The secret of getting ahead is getting started.", Author = "Mark Twain" },
            new QuoteModel { Content = "There is no greater agony than bearing an untold story inside you.", Author = "Maya Angelou" },
            new QuoteModel { Content = "First, find a subject you care about and which you in your heart feel others should care about.", Author = "Kurt Vonnegut" },
            new QuoteModel { Content = "Either write something worth reading or do something worth writing.", Author = "Benjamin Franklin" },
            new QuoteModel { Content = "You don't start with perfection. You start with a sentence.", Author = "StoryVerse Wisdom" },
            new QuoteModel { Content = "Fill your paper with the breathings of your heart.", Author = "William Wordsworth" },
            new QuoteModel { Content = "If there's a book that you want to read, but it hasn't been written yet, then you must write it.", Author = "Toni Morrison" },
            new QuoteModel { Content = "Lock up your libraries if you like; but there is no gate, no lock, no bolt that you can set upon the freedom of my mind.", Author = "Virginia Woolf" },
            new QuoteModel { Content = "Storytelling is the most powerful way to put ideas into the world today.", Author = "Robert McKee" },
            new QuoteModel { Content = "Ideas are like rabbits. You get a couple and learn how to handle them, and pretty soon you have a dozen.", Author = "John Steinbeck" },
            new QuoteModel { Content = "The scariest moment is always just before you start.", Author = "Stephen King" },
            new QuoteModel { Content = "The art of writing is the art of discovering what you believe.", Author = "Gustave Flaubert" },
            new QuoteModel { Content = "You write to communicate to the hearts and minds of others what's burning inside your own.", Author = "Arthur Polot" },
            new QuoteModel { Content = "Every secret of a writer's soul, every experience of his life, every quality of his mind is written large in his works.", Author = "Virginia Woolf" }
        };

        private readonly Random _random = new Random();

        public QuoteModel GetDailyQuote()
        {
            // Deterministic selection based on current UTC date
            var epoch = new DateTime(2026, 1, 1);
            var dayIndex = (int)(DateTime.UtcNow.Date - epoch).TotalDays;
            var index = Math.Abs(dayIndex) % Quotes.Count;
            return Quotes[index];
        }

        public QuoteModel GetRandomQuote(string? currentContent = null)
        {
            var candidates = Quotes.Where(q => string.IsNullOrEmpty(currentContent) || 
                                              !q.Content.Equals(currentContent, StringComparison.OrdinalIgnoreCase))
                                   .ToList();

            if (!candidates.Any())
            {
                candidates = Quotes;
            }

            var index = _random.Next(candidates.Count);
            return candidates[index];
        }
    }
}
