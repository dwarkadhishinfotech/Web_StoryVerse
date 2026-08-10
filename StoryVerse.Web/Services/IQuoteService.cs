namespace StoryVerse.Web.Services
{
    public class QuoteModel
    {
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
    }

    public interface IQuoteService
    {
        QuoteModel GetDailyQuote();
        QuoteModel GetRandomQuote(string? currentContent = null);
    }
}
