using KnowledgeBase.Tenant.Domain;

namespace KnowledgeBase.Tenant.Application;

public sealed class TenantAppService
{
    private readonly ITenantRepository repository;

    public TenantAppService(ITenantRepository repository)
    {
        this.repository = repository;
    }

    public async Task<TenantDto> CreateAsync(string name, string ownerUserId, CancellationToken cancellationToken)
    {
        var tenant = new TenantEntity(name, ownerUserId);
        await repository.AddAsync(tenant, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return TenantDto.From(tenant);
    }

    public async Task<IReadOnlyList<TenantDto>> ListByUserAsync(string userId, CancellationToken cancellationToken)
    {
        var tenants = await repository.ListByUserAsync(userId, cancellationToken);
        return tenants.Select(TenantDto.From).ToList();
    }

    public async Task<TenantDto?> GetAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var tenant = await repository.GetAsync(tenantId, cancellationToken);
        return tenant is null ? null : TenantDto.From(tenant);
    }

    public async Task<MembershipDto> AddMemberAsync(
        Guid tenantId,
        string userId,
        MemberRole role,
        CancellationToken cancellationToken)
    {
        var tenant = await repository.GetAsync(tenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Tenant '{tenantId}' not found.");

        var membership = tenant.AddMember(userId, role);
        await repository.SaveChangesAsync(cancellationToken);

        return MembershipDto.From(membership);
    }

    public async Task RemoveMemberAsync(
        Guid tenantId,
        string userId,
        CancellationToken cancellationToken)
    {
        var tenant = await repository.GetAsync(tenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Tenant '{tenantId}' not found.");

        tenant.RemoveMember(userId);
        await repository.SaveChangesAsync(cancellationToken);
    }
}

public sealed record TenantDto(Guid Id, string Name, DateTime CreatedAt, int MemberCount)
{
    public static TenantDto From(TenantEntity entity) =>
        new(entity.Id, entity.Name, entity.CreatedAt, entity.Memberships.Count);
}

public sealed record MembershipDto(Guid Id, Guid TenantId, string UserId, MemberRole Role, DateTime JoinedAt)
{
    public static MembershipDto From(TenantMembership m) =>
        new(m.Id, m.TenantId, m.UserId, m.Role, m.JoinedAt);
}
