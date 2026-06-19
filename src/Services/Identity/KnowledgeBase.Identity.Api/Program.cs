using KnowledgeBase.Identity.Api;
using KnowledgeBase.Identity.Api.Data;
using KnowledgeBase.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddKnowledgeBaseControllers();
builder.Services.AddKnowledgeBaseSwagger("Identity Service API");

var connectionString = builder.Configuration.GetConnectionString("identitydb")
    ?? throw new InvalidOperationException("Connection string 'identitydb' is not configured.");

builder.Services.AddDbContext<IdentityDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.UseOpenIddict();
});

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();

var googleOptions = builder.Configuration
    .GetSection(GoogleOAuthOptions.SectionName)
    .Get<GoogleOAuthOptions>() ?? new GoogleOAuthOptions();

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = googleOptions.ClientId;
        options.ClientSecret = googleOptions.ClientSecret;
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.SaveTokens = true;
    });

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<IdentityDbContext>();
    })
    .AddServer(options =>
    {
        options
            .SetAuthorizationEndpointUris("connect/authorize")
            .SetTokenEndpointUris("connect/token");

        options
            .AllowAuthorizationCodeFlow()
            .AllowRefreshTokenFlow();

        options
            .AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableTokenEndpointPassthrough();

        options.RegisterScopes("openid", "profile", "email", "roles");
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseKnowledgeBaseSwagger();

    await using (var scope = app.Services.CreateAsyncScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await dbContext.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = [KnowledgeBase.Auth.Roles.Admin, KnowledgeBase.Auth.Roles.Manager, KnowledgeBase.Auth.Roles.Employee];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultEndpoints();

await app.RunAsync();
