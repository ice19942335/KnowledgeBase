namespace KnowledgeBase.Tenancy;

public sealed class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }

    public bool HasTenant => TenantId.HasValue;

    public void SetTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id cannot be empty.", nameof(tenantId));
        }

        TenantId = tenantId;
    }

    public Guid RequireTenant()
    {
        return TenantId ?? throw new InvalidOperationException("No tenant is set for the current scope.");
    }
}
