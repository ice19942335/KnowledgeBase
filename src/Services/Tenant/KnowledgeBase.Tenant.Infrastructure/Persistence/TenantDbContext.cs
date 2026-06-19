using KnowledgeBase.Tenant.Domain;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Tenant.Infrastructure.Persistence;

public sealed class TenantDbContext : DbContext
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options)
        : base(options)
    {
    }

    public DbSet<TenantEntity> Tenants => Set<TenantEntity>();
    public DbSet<TenantMembership> Memberships => Set<TenantMembership>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TenantEntity>(entity =>
        {
            entity.ToTable("tenants");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).HasMaxLength(256).IsRequired();

            entity.HasMany(t => t.Memberships)
                .WithOne()
                .HasForeignKey(m => m.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TenantMembership>(entity =>
        {
            entity.ToTable("tenant_memberships");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.UserId).HasMaxLength(256).IsRequired();
            entity.Property(m => m.Role).HasConversion<int>();

            entity.HasIndex(m => new { m.TenantId, m.UserId }).IsUnique();
            entity.HasIndex(m => m.UserId);
        });
    }
}
