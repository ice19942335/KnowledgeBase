using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeBase.Tenancy;

public static class TenancyExtensions
{
    public static IServiceCollection AddTenancy(this IServiceCollection services)
    {
        services.AddScoped<ITenantContext, TenantContext>();
        return services;
    }

    public static IApplicationBuilder UseTenantContext(this IApplicationBuilder app)
    {
        return app
            .UseMiddleware<DevelopmentTenantDefaultsMiddleware>()
            .UseMiddleware<TenantContextMiddleware>();
    }
}
