using KnowledgeBase.Auth;
using KnowledgeBase.Tenant.Application;
using KnowledgeBase.Tenant.Infrastructure;
using KnowledgeBase.Tenant.Infrastructure.Persistence;
using KnowledgeBase.Web;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddKnowledgeBaseControllers();
builder.Services.AddKnowledgeBaseSwagger("Tenant Service API");

builder.Services.AddKnowledgeBaseAuth(builder.Configuration);

builder.Services.AddTenantApplication();
builder.Services.AddTenantInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseKnowledgeBaseSwagger();

    await using (var scope = app.Services.CreateAsyncScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultEndpoints();

await app.RunAsync();
