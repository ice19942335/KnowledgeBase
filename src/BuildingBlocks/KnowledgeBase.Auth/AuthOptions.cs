namespace KnowledgeBase.Auth;

public class AuthOptions
{
    public const string SectionName = "Authentication";

    /// <summary>
    /// Token issuer (the Identity service authority), e.g. https://identity.
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Expected audience for access tokens.
    /// </summary>
    public string Audience { get; set; } = "knowledgebase-api";

    /// <summary>
    /// Allows HTTP metadata for local development (Aspire) where the Identity
    /// service is reachable over plain HTTP.
    /// </summary>
    public bool RequireHttpsMetadata { get; set; }
}
