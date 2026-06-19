namespace KnowledgeBase.Tenancy;

/// <summary>
/// Ambient, scoped tenant for the current request or message.
/// </summary>
public interface ITenantContext
{
    Guid? TenantId { get; }

    bool HasTenant { get; }

    void SetTenant(Guid tenantId);

    Guid RequireTenant();
}
