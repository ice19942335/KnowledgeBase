namespace KnowledgeBase.Tenancy;

public static class TenantConstants
{
    public const string TenantHeader = "X-Tenant-Id";

    /// <summary>
    /// Claim that carries the set of tenants a user belongs to.
    /// </summary>
    public const string TenantMembershipClaim = "tenant_membership";
}
