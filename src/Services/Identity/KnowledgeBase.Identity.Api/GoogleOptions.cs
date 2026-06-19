namespace KnowledgeBase.Identity.Api;

public class GoogleOAuthOptions
{
    public const string SectionName = "Google";

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;
}
