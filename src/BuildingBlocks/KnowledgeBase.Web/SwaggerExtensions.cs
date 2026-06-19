using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace KnowledgeBase.Web;

public static class SwaggerExtensions
{
    public static IServiceCollection AddKnowledgeBaseSwagger(
        this IServiceCollection services,
        string title)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = title,
                Version = "v1"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT access token issued by the Identity service."
            });

            options.OperationFilter<TenantHeaderOperationFilter>();
        });

        return services;
    }

    public static IApplicationBuilder UseKnowledgeBaseSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        return app;
    }
}
