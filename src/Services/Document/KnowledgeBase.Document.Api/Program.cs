using KnowledgeBase.Auth;
using KnowledgeBase.Document.Application;
using KnowledgeBase.Document.Infrastructure;
using KnowledgeBase.Document.Infrastructure.Persistence;
using KnowledgeBase.Tenancy;
using KnowledgeBase.Web;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddKnowledgeBaseControllers();
builder.Services.AddKnowledgeBaseSwagger("Document Service API");

builder.Services.AddTenancy();
builder.Services.AddKnowledgeBaseAuth(builder.Configuration);

builder.Services.AddDocumentApplication();
builder.Services.AddDocumentInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseKnowledgeBaseSwagger();

    await using (var scope = app.Services.CreateAsyncScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<DocumentDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}

app.UseTenantContext();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultEndpoints();

await app.RunAsync();
