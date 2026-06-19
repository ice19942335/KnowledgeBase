using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace KnowledgeBase.Tenancy;

public sealed class DevelopmentTenantDefaultsMiddleware
{
    private readonly RequestDelegate next;
    private readonly IWebHostEnvironment environment;

    public DevelopmentTenantDefaultsMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        this.next = next;
        this.environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (environment.IsDevelopment()
            && !context.Request.Headers.ContainsKey(TenantConstants.TenantHeader))
        {
            context.Request.Headers[TenantConstants.TenantHeader] = DevelopmentTenancy.DevTenantId.ToString();
        }

        await next(context);
    }
}
