using KnowledgeBase.Tenant.Application;
using KnowledgeBase.Tenant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeBase.Tenant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTenantInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("tenantdb")
            ?? throw new InvalidOperationException("Connection string 'tenantdb' is not configured.");

        services.AddDbContext<TenantDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITenantRepository, TenantRepository>();

        return services;
    }
}
