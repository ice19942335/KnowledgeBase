using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeBase.Tenant.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTenantApplication(this IServiceCollection services)
    {
        services.AddScoped<TenantAppService>();
        return services;
    }
}
