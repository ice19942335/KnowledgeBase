namespace KnowledgeBase.Tenant.Domain;

public sealed class TenantMembership
{
    private TenantMembership()
    {
    }

    public TenantMembership(Guid tenantId, string userId, MemberRole role)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        UserId = userId;
        Role = role;
        JoinedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string UserId { get; private set; } = string.Empty;

    public MemberRole Role { get; private set; }

    public DateTime JoinedAt { get; private set; }

    public void ChangeRole(MemberRole newRole)
    {
        Role = newRole;
    }
}
