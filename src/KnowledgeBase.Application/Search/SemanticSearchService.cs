using KnowledgeBase.Ai;
using KnowledgeBase.Application.Abstractions;
using KnowledgeBase.Application.Common.Options;
using Microsoft.Extensions.Options;

namespace KnowledgeBase.Application.Search;

public sealed class SemanticSearchService : ISemanticSearchService
{
    private readonly IEmbeddingGenerator embeddingGenerator;
    private readonly ChunkRetrievalPipeline retrievalPipeline;

    public SemanticSearchService(
        IEmbeddingGenerator embeddingGenerator,
        ChunkRetrievalPipeline retrievalPipeline)
    {
        this.embeddingGenerator = embeddingGenerator;
        this.retrievalPipeline = retrievalPipeline;
    }

    public async Task<IReadOnlyList<SearchResultDto>> SearchAsync(
        string query,
        int? topK,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query cannot be empty.", nameof(query));
        }

        var queryEmbedding = await embeddingGenerator.GenerateAsync(query, cancellationToken);
        var matches = await retrievalPipeline.RetrieveAsync(
            query,
            queryEmbedding,
            topK,
            cancellationToken);

        return matches
            .Select(match => new SearchResultDto(
                match.DocumentId,
                match.DocumentName,
                match.FileName,
                match.ChunkIndex,
                match.Content,
                match.Score))
            .ToList();
    }
}
