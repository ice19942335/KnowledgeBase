using KnowledgeBase.Auth;
using KnowledgeBase.Chat.Application;
using KnowledgeBase.Chat.Infrastructure;
using KnowledgeBase.Chat.Infrastructure.Persistence;
using KnowledgeBase.Tenancy;
using KnowledgeBase.Web;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddKnowledgeBaseControllers();
builder.Services.AddKnowledgeBaseSwagger("Chat Service API");

builder.Services.AddTenancy();
builder.Services.AddKnowledgeBaseAuth(builder.Configuration);

builder.Services.AddChatApplication(builder.Configuration);
builder.Services.AddChatInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseKnowledgeBaseSwagger();

    await using (var scope = app.Services.CreateAsyncScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}

app.UseTenantContext();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultEndpoints();

await app.RunAsync();
