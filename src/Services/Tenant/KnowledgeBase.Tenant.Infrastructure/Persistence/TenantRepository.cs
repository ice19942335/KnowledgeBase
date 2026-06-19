using KnowledgeBase.Tenant.Application;
using KnowledgeBase.Tenant.Domain;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Tenant.Infrastructure.Persistence;

public sealed class TenantRepository : ITenantRepository
{
    private readonly TenantDbContext dbContext;

    public TenantRepository(TenantDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task AddAsync(TenantEntity tenant, CancellationToken cancellationToken)
    {
        await dbContext.Tenants.AddAsync(tenant, cancellationToken);
    }

    public Task<TenantEntity?> GetAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return dbContext.Tenants
            .Include(t => t.Memberships)
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
    }

    public async Task<IReadOnlyList<TenantEntity>> ListByUserAsync(string userId, CancellationToken cancellationToken)
    {
        return await dbContext.Tenants
            .Where(t => t.Memberships.Any(m => m.UserId == userId))
            .Include(t => t.Memberships)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public Task RemoveAsync(TenantEntity tenant, CancellationToken cancellationToken)
    {
        dbContext.Tenants.Remove(tenant);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
