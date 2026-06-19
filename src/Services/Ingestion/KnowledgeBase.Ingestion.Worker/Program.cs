using KnowledgeBase.Ingestion.Application;
using KnowledgeBase.Ingestion.Infrastructure;
using KnowledgeBase.Ingestion.Worker;
using KnowledgeBase.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddIngestionApplication(builder.Configuration);
builder.Services.AddIngestionInfrastructure(builder.Configuration);

builder.Services.AddKnowledgeBaseMessaging(builder.Configuration, bus =>
{
    bus.AddConsumer<DocumentUploadedConsumer>();
});

var host = builder.Build();
host.Run();
