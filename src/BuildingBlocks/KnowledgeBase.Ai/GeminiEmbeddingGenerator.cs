using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Options;

namespace KnowledgeBase.Ai;

public sealed class GeminiEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly Client client;
    private readonly IAiAvailabilityState availabilityState;
    private readonly string embeddingModel;
    private readonly int embeddingDimensions;

    public GeminiEmbeddingGenerator(
        Client client,
        IAiAvailabilityState availabilityState,
        IOptions<GeminiOptions> options)
    {
        var value = options.Value;
        this.client = client;
        this.availabilityState = availabilityState;
        embeddingModel = value.EmbeddingModel;
        embeddingDimensions = value.EmbeddingDimensions;
    }

    public async Task<IReadOnlyList<EmbeddingResult>> GenerateAsync(
        IReadOnlyList<string> inputs,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();
        ArgumentNullException.ThrowIfNull(inputs);

        if (inputs.Count == 0)
        {
            return Array.Empty<EmbeddingResult>();
        }

        var documentConfig = CreateDocumentConfig();
        var embeddings = new List<EmbeddingResult>(inputs.Count);

        foreach (var input in inputs)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response = await client.Models.EmbedContentAsync(
                embeddingModel,
                input,
                documentConfig,
                cancellationToken);

            embeddings.Add(await ReadEmbeddingAsync(response, input, "document chunk", cancellationToken));
        }

        return embeddings;
    }

    public async Task<EmbeddingResult> GenerateAsync(string input, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        var queryConfig = CreateQueryConfig();

        var response = await client.Models.EmbedContentAsync(
            embeddingModel,
            input,
            queryConfig,
            cancellationToken);

        return await ReadEmbeddingAsync(response, input, "search query", cancellationToken);
    }

    private EmbedContentConfig CreateDocumentConfig()
    {
        return new EmbedContentConfig
        {
            OutputDimensionality = embeddingDimensions,
            TaskType = "RETRIEVAL_DOCUMENT"
        };
    }

    private EmbedContentConfig CreateQueryConfig()
    {
        return new EmbedContentConfig
        {
            OutputDimensionality = embeddingDimensions,
            TaskType = "RETRIEVAL_QUERY"
        };
    }

    private async Task<EmbeddingResult> ReadEmbeddingAsync(
        EmbedContentResponse response,
        string input,
        string purpose,
        CancellationToken cancellationToken)
    {
        var contentEmbedding = response.Embeddings?.FirstOrDefault();
        var values = ToFloatArray(contentEmbedding?.Values);
        EnsureValidEmbedding(values, purpose);

        var tokenCount = await ResolveTokenCountAsync(input, contentEmbedding, cancellationToken);
        return new EmbeddingResult(values, tokenCount);
    }

    private async Task<int> ResolveTokenCountAsync(
        string input,
        ContentEmbedding? embedding,
        CancellationToken cancellationToken)
    {
        var statisticsTokenCount = GeminiEmbeddingTokenCounts.ReadStatisticsTokenCount(embedding);
        if (statisticsTokenCount > 0)
        {
            return statisticsTokenCount;
        }

        var countResponse = await client.Models.CountTokensAsync(
            embeddingModel,
            input,
            cancellationToken: cancellationToken);

        return countResponse.TotalTokens ?? 0;
    }

    private void EnsureValidEmbedding(float[] embedding, string purpose)
    {
        if (embedding.Length == 0)
        {
            throw new InvalidOperationException(
                $"Gemini returned an empty embedding for {purpose}. Verify Gemini:ApiKey and {GeminiOptions.SectionName}:EmbeddingModel.");
        }

        if (embedding.Length != embeddingDimensions)
        {
            throw new InvalidOperationException(
                $"Gemini returned {embedding.Length} dimensions for {purpose}, expected {embeddingDimensions}.");
        }
    }

    private void EnsureConfigured()
    {
        if (!availabilityState.IsConfigured)
        {
            throw new AiNotConfiguredException();
        }
    }

    private static float[] ToFloatArray(IReadOnlyList<double>? values)
    {
        if (values is null || values.Count == 0)
        {
            return Array.Empty<float>();
        }

        var result = new float[values.Count];
        for (var index = 0; index < values.Count; index++)
        {
            result[index] = (float)values[index];
        }

        return result;
    }
}
