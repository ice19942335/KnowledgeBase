using Microsoft.AspNetCore.Http;

namespace KnowledgeBase.Tenancy;

/// <summary>
/// Reads the tenant id from the <c>X-Tenant-Id</c> header (the single source of
/// truth) and populates the scoped <see cref="ITenantContext"/>.
/// </summary>
public sealed class TenantContextMiddleware
{
    private readonly RequestDelegate next;

    public TenantContextMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.Request.Headers.TryGetValue(TenantConstants.TenantHeader, out var headerValue)
            && Guid.TryParse(headerValue.ToString(), out var tenantId)
            && tenantId != Guid.Empty)
        {
            tenantContext.SetTenant(tenantId);
        }

        await next(context);
    }
}
