namespace KnowledgeBase.Chat.Application;

public class ChatOptions
{
    public const string SectionName = "Chat";

    /// <summary>
    /// Search API base URL. Aspire uses service discovery (<c>https+http://search-api</c>);
    /// Docker Compose must use an explicit HTTP endpoint (<c>http://search-api:8080</c>).
    /// </summary>
    public string SearchServiceBaseUrl { get; set; } = "https+http://search-api";
}
