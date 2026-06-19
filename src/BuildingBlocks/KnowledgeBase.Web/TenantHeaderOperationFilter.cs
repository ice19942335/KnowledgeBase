using KnowledgeBase.Tenancy;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace KnowledgeBase.Web;

/// <summary>
/// Adds the <c>X-Tenant-Id</c> header to every operation in Swagger UI so the
/// tenant (single source of truth) can be supplied while testing.
/// </summary>
public sealed class TenantHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<IOpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = TenantConstants.TenantHeader,
            In = ParameterLocation.Header,
            Required = false,
            Description = "Tenant identifier (GUID). Single source of truth for tenant resolution.",
            Schema = new OpenApiSchema { Type = JsonSchemaType.String }
        });
    }
}
