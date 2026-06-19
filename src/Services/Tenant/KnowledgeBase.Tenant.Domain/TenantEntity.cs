namespace KnowledgeBase.Tenant.Domain;

public sealed class TenantEntity
{
    private readonly List<TenantMembership> memberships = [];

    private TenantEntity()
    {
    }

    public TenantEntity(string name, string ownerUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tenant name is required.", nameof(name));
        }

        Id = Guid.NewGuid();
        Name = name;
        CreatedAt = DateTime.UtcNow;

        memberships.Add(new TenantMembership(Id, ownerUserId, MemberRole.Admin));
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public IReadOnlyList<TenantMembership> Memberships => memberships.AsReadOnly();

    public TenantMembership AddMember(string userId, MemberRole role)
    {
        if (memberships.Any(m => m.UserId == userId))
        {
            throw new InvalidOperationException($"User '{userId}' is already a member of this tenant.");
        }

        var membership = new TenantMembership(Id, userId, role);
        memberships.Add(membership);
        return membership;
    }

    public void RemoveMember(string userId)
    {
        var membership = memberships.FirstOrDefault(m => m.UserId == userId)
            ?? throw new InvalidOperationException($"User '{userId}' is not a member of this tenant.");

        memberships.Remove(membership);
    }
}
