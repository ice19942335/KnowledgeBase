using Microsoft.AspNetCore.Identity;

namespace KnowledgeBase.Identity.Api.Data;

/// <summary>
/// Extends IdentityUser with display name and profile picture from Google.
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;

    public string? PictureUrl { get; set; }
}
