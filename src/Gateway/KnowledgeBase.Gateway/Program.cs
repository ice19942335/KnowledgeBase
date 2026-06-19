using KnowledgeBase.Auth;
using KnowledgeBase.Gateway;
using KnowledgeBase.Tenancy;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddKnowledgeBaseAuth(builder.Configuration);
builder.Services.AddTenancy();

builder.Services
    .AddOptions<CorsOptions>()
    .Bind(builder.Configuration.GetSection(CorsOptions.SectionName));

const string clientCorsPolicy = "client";
builder.Services.AddCors(options =>
{
    options.AddPolicy(clientCorsPolicy, policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return false;
                }

                return uri.Host is "localhost" or "127.0.0.1";
            });
        }
        else
        {
            var corsOptions = builder.Configuration
                .GetSection(CorsOptions.SectionName)
                .Get<CorsOptions>() ?? new CorsOptions();

            policy.WithOrigins(corsOptions.AllowedOrigins);
        }

        policy.AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

app.UseCors(clientCorsPolicy);
app.UseTenantContext();
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();
app.MapDefaultEndpoints();

await app.RunAsync();
