using Google.GenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KnowledgeBase.Ai;

public static class AiExtensions
{
    public static IServiceCollection AddKnowledgeBaseAi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ConfigureGeminiOptions(services, configuration);
        RegisterGeminiClient(services, configuration);
        RegisterAiRuntimeOptions(services, configuration);
        RegisterAvailabilityState(services);

        RegisterContextualEmbeddingServices(services, configuration);

        services.AddSingleton<IEmbeddingGenerator, GeminiEmbeddingGenerator>();
        services.AddSingleton<IChatCompletionService, GeminiChatCompletionService>();

        return services;
    }

    /// <summary>
    /// Registers only the embedding generator (used by the Ingestion worker).
    /// </summary>
    public static IServiceCollection AddKnowledgeBaseEmbeddings(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ConfigureGeminiOptions(services, configuration);
        RegisterGeminiClient(services, configuration);
        RegisterAiRuntimeOptions(services, configuration);
        RegisterAvailabilityState(services);

        RegisterContextualEmbeddingServices(services, configuration);

        services.AddSingleton<IEmbeddingGenerator, GeminiEmbeddingGenerator>();

        return services;
    }

    private static void RegisterContextualEmbeddingServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<ContextualEmbeddingOptions>()
            .Bind(configuration.GetSection(ContextualEmbeddingOptions.SectionName));

        services.AddOptions<RerankingOptions>()
            .Bind(configuration.GetSection(RerankingOptions.SectionName));

        services.AddSingleton<IContextualEmbeddingFormatter, ContextualEmbeddingFormatter>();
        services.AddSingleton<IDocumentSummaryGenerator, DocumentSummaryGenerator>();
        services.AddSingleton<IChunkReranker, LlmChunkReranker>();
    }

    private static void RegisterAvailabilityState(IServiceCollection services)
    {
        services.AddSingleton<IAiAvailabilityState, AiAvailabilityState>();
    }

    private static void RegisterGeminiClient(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(_ =>
        {
            var options = configuration.GetSection(GeminiOptions.SectionName).Get<GeminiOptions>()
                ?? new GeminiOptions();
            return new Client(apiKey: options.ApiKey);
        });
    }

    private static void RegisterAiRuntimeOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IOptions<AiRuntimeOptions>>(_ =>
            Options.Create(new AiRuntimeOptions
            {
                EmbeddingDimensions = AiConfiguration.GetEmbeddingDimensions(configuration)
            }));
    }

    private static void ConfigureGeminiOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<GeminiOptions>()
            .Bind(configuration.GetSection(GeminiOptions.SectionName));

        if (!IsDevelopmentEnvironment(configuration))
        {
            services.AddOptions<GeminiOptions>()
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.ApiKey),
                    "Gemini:ApiKey must be configured.")
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }
    }

    private static bool IsDevelopmentEnvironment(IConfiguration configuration)
    {
        var environment = configuration["DOTNET_ENVIRONMENT"]
            ?? configuration["ASPNETCORE_ENVIRONMENT"];

        return string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);
    }
}
