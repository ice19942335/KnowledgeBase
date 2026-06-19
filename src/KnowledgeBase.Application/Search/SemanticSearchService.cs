using KnowledgeBase.Ai;
using KnowledgeBase.Application.Abstractions;
using KnowledgeBase.Application.Common.Options;
using Microsoft.Extensions.Options;

namespace KnowledgeBase.Application.Search;

public sealed class SemanticSearchService : ISemanticSearchService
{
    private readonly IEmbeddingGenerator embeddingGenerator;
    private readonly IChunkSearchRepository searchRepository;
    private readonly SearchOptions options;

    public SemanticSearchService(
        IEmbeddingGenerator embeddingGenerator,
        IChunkSearchRepository searchRepository,
        IOptions<SearchOptions> options)
    {
        this.embeddingGenerator = embeddingGenerator;
        this.searchRepository = searchRepository;
        this.options = options.Value;
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

        var effectiveTopK = ResolveTopK(topK);
        var queryEmbedding = await embeddingGenerator.GenerateAsync(query, cancellationToken);
        var matches = await searchRepository.SearchAsync(queryEmbedding, effectiveTopK, cancellationToken);

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

    private int ResolveTopK(int? requested)
    {
        if (requested is null or <= 0)
        {
            return options.DefaultTopK;
        }

        return Math.Min(requested.Value, options.MaxTopK);
    }
}
