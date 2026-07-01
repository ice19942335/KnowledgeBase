var builder = DistributedApplication.CreateBuilder(args);

// Shared PostgreSQL server with the pgvector extension image. Each service owns
// its own logical database (database-per-service).
var postgres = builder.AddPostgres("postgres")
    .WithImage("pgvector/pgvector", "pg17")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var identityDb = postgres.AddDatabase("identitydb");
var tenantDb = postgres.AddDatabase("tenantdb");
var documentDb = postgres.AddDatabase("documentdb");
var searchDb = postgres.AddDatabase("searchdb");
var chatDb = postgres.AddDatabase("chatdb");

// Asynchronous messaging backbone.
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent);

// Cache and tenant-context store.
var redis = builder.AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent);

// Shared blob storage location for uploaded files (Document writes, Ingestion reads).
var blobPath = Path.Combine(Path.GetTempPath(), "knowledgebase-blobs");

// Gemini API key is supplied via AppHost user-secrets / environment and propagated
// to the services that call the model.
var geminiApiKey = builder.Configuration["Gemini:ApiKey"] ?? string.Empty;
var googleClientId = builder.Configuration["Google:ClientId"] ?? string.Empty;
var googleClientSecret = builder.Configuration["Google:ClientSecret"] ?? string.Empty;

var identityApi = builder.AddProject<Projects.KnowledgeBase_Identity_Api>("identity-api")
    .WithReplicas(2)
    .WithReference(identityDb)
    .WithReference(redis)
    .WaitFor(postgres)
    .WithEnvironment("Google__ClientId", googleClientId)
    .WithEnvironment("Google__ClientSecret", googleClientSecret);

var tenantApi = builder.AddProject<Projects.KnowledgeBase_Tenant_Api>("tenant-api")
    .WithReplicas(2)
    .WithReference(tenantDb)
    .WithReference(redis)
    .WaitFor(postgres);

var documentApi = builder.AddProject<Projects.KnowledgeBase_Document_Api>("document-api")
    .WithReplicas(2)
    .WithReference(documentDb)
    .WithReference(rabbitmq)
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(rabbitmq)
    .WithEnvironment("FileStorage__RootPath", blobPath);

builder.AddProject<Projects.KnowledgeBase_Ingestion_Worker>("ingestion-worker")
    .WithReplicas(2)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithEnvironment("FileStorage__RootPath", blobPath)
    .WithEnvironment("Gemini__ApiKey", geminiApiKey);

var searchApi = builder.AddProject<Projects.KnowledgeBase_Search_Api>("search-api")
    .WithReplicas(2)
    .WithReference(searchDb)
    .WithReference(rabbitmq)
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(rabbitmq)
    .WithEnvironment("Gemini__ApiKey", geminiApiKey);

var chatApi = builder.AddProject<Projects.KnowledgeBase_Chat_Api>("chat-api")
    .WithReplicas(2)
    .WithReference(chatDb)
    .WithReference(searchApi)
    .WithReference(redis)
    .WaitFor(postgres)
    .WithEnvironment("Gemini__ApiKey", geminiApiKey);

var gateway = builder.AddProject<Projects.KnowledgeBase_Gateway>("gateway")
    .WithReplicas(2)
    .WithReference(identityApi)
    .WithReference(tenantApi)
    .WithReference(documentApi)
    .WithReference(searchApi)
    .WithReference(chatApi)
    .WithReference(redis)
    .WaitFor(identityApi)
    .WaitFor(tenantApi)
    .WaitFor(documentApi)
    .WaitFor(searchApi)
    .WaitFor(chatApi)
    .WithExternalHttpEndpoints();

builder.AddViteApp("client", "../client")
    .WithReference(gateway)
    .WaitFor(gateway);

builder.Build().Run();
