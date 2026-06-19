using KnowledgeBase.Auth;
using KnowledgeBase.Search.Application;
using KnowledgeBase.Search.Infrastructure;
using KnowledgeBase.Search.Infrastructure.Persistence;
using KnowledgeBase.Tenancy;
using KnowledgeBase.Web;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddKnowledgeBaseControllers();
builder.Services.AddKnowledgeBaseSwagger("Search Service API");

builder.Services.AddTenancy();
builder.Services.AddKnowledgeBaseAuth(builder.Configuration);

builder.Services.AddSearchApplication(builder.Configuration);
builder.Services.AddSearchInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseKnowledgeBaseSwagger();

    await using (var scope = app.Services.CreateAsyncScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<SearchDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}

app.UseTenantContext();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultEndpoints();

await app.RunAsync();
