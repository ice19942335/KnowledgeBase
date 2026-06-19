namespace KnowledgeBase.Auth;

/// <summary>
/// Application roles. Admin manages the tenant and members, Manager manages
/// content (documents), Employee consumes search and chat.
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Employee = "Employee";
}

public static class AuthPolicies
{
    /// <summary>Admin only.</summary>
    public const string TenantAdministration = "TenantAdministration";

    /// <summary>Admin or Manager.</summary>
    public const string ContentManagement = "ContentManagement";

    /// <summary>Any authenticated member (Admin, Manager, Employee).</summary>
    public const string Member = "Member";
}
