using KnowledgeBase.Tenant.Domain;

namespace KnowledgeBase.Tenant.Application;

public interface ITenantRepository
{
    Task AddAsync(TenantEntity tenant, CancellationToken cancellationToken);

    Task<TenantEntity?> GetAsync(Guid tenantId, CancellationToken cancellationToken);

    Task<IReadOnlyList<TenantEntity>> ListByUserAsync(string userId, CancellationToken cancellationToken);

    Task RemoveAsync(TenantEntity tenant, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
