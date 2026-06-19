using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KnowledgeBase.Auth;

public static class AuthExtensions
{
    public static IServiceCollection AddKnowledgeBaseAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>()
            ?? new AuthOptions();

        var useDevelopmentAuth = IsDevelopment(configuration)
            && string.IsNullOrWhiteSpace(options.Authority);

        var authBuilder = services.AddAuthentication(authenticationOptions =>
        {
            if (useDevelopmentAuth)
            {
                authenticationOptions.DefaultAuthenticateScheme = DevelopmentAuthenticationDefaults.Scheme;
                authenticationOptions.DefaultChallengeScheme = DevelopmentAuthenticationDefaults.Scheme;
            }
        });

        if (useDevelopmentAuth)
        {
            authBuilder.AddScheme<AuthenticationSchemeOptions, DevelopmentAuthenticationHandler>(
                DevelopmentAuthenticationDefaults.Scheme,
                _ => { });
        }
        else
        {
            authBuilder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwt =>
            {
                jwt.Authority = options.Authority;
                jwt.Audience = options.Audience;
                jwt.RequireHttpsMetadata = options.RequireHttpsMetadata;
                jwt.TokenValidationParameters.ValidateAudience = !string.IsNullOrEmpty(options.Audience);
                jwt.TokenValidationParameters.RoleClaimType = "role";
                jwt.TokenValidationParameters.NameClaimType = "name";
            });
        }

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthPolicies.TenantAdministration, policy =>
                policy.RequireRole(Roles.Admin))
            .AddPolicy(AuthPolicies.ContentManagement, policy =>
                policy.RequireRole(Roles.Admin, Roles.Manager))
            .AddPolicy(AuthPolicies.Member, policy =>
                policy.RequireRole(Roles.Admin, Roles.Manager, Roles.Employee));

        return services;
    }

    private static bool IsDevelopment(IConfiguration configuration) =>
        string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], Environments.Development, StringComparison.OrdinalIgnoreCase)
        || string.Equals(configuration["DOTNET_ENVIRONMENT"], Environments.Development, StringComparison.OrdinalIgnoreCase);
}
