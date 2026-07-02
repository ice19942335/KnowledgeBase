using KnowledgeBase.Ai;
using KnowledgeBase.Contracts;
using KnowledgeBase.Ingestion.Application.Abstractions;
using KnowledgeBase.SharedKernel.Storage;

namespace KnowledgeBase.Ingestion.Application;

/// <summary>
/// Stateless pipeline: read the stored file, extract text, chunk it, and produce
/// embeddings. The result is published downstream to the Search service.
/// </summary>
public sealed class IngestionService
{
    private readonly IFileStorage fileStorage;
    private readonly ITextExtractionService textExtractionService;
    private readonly ITextChunker textChunker;
    private readonly IEmbeddingGenerator embeddingGenerator;
    private readonly IDocumentSummaryGenerator documentSummaryGenerator;
    private readonly IContextualEmbeddingFormatter contextualEmbeddingFormatter;
    private readonly IAiAvailabilityState aiAvailabilityState;

    public IngestionService(
        IFileStorage fileStorage,
        ITextExtractionService textExtractionService,
        ITextChunker textChunker,
        IEmbeddingGenerator embeddingGenerator,
        IDocumentSummaryGenerator documentSummaryGenerator,
        IContextualEmbeddingFormatter contextualEmbeddingFormatter,
        IAiAvailabilityState aiAvailabilityState)
    {
        this.fileStorage = fileStorage;
        this.textExtractionService = textExtractionService;
        this.textChunker = textChunker;
        this.embeddingGenerator = embeddingGenerator;
        this.documentSummaryGenerator = documentSummaryGenerator;
        this.contextualEmbeddingFormatter = contextualEmbeddingFormatter;
        this.aiAvailabilityState = aiAvailabilityState;
    }

    public async Task<IReadOnlyList<GeneratedChunk>> IngestAsync(
        DocumentUploaded message,
        CancellationToken cancellationToken)
    {
        if (!aiAvailabilityState.IsConfigured)
        {
            throw new AiNotConfiguredException();
        }

        await using var stream = await fileStorage.OpenAsync(message.StoragePath, cancellationToken);

        var text = await textExtractionService.ExtractAsync(
            stream,
            message.FileName,
            message.ContentType,
            cancellationToken);

        var chunks = textChunker.Split(text);
        if (chunks.Count == 0)
        {
            return Array.Empty<GeneratedChunk>();
        }

        var summary = await documentSummaryGenerator.GenerateAsync(
            message.DocumentName,
            text,
            cancellationToken);

        var embeddingInputs = chunks
            .Select((chunk, index) => contextualEmbeddingFormatter.Format(new ContextualEmbeddingRequest(
                message.DocumentName,
                message.FileName,
                index,
                chunks.Count,
                chunk.Content,
                chunk.SectionTitle,
                summary)))
            .ToList();

        var embeddings = await embeddingGenerator.GenerateAsync(embeddingInputs, cancellationToken);

        var generated = new List<GeneratedChunk>(chunks.Count);
        for (var index = 0; index < chunks.Count; index++)
        {
            generated.Add(new GeneratedChunk(
                index,
                chunks[index].Content,
                embeddings[index].Values,
                embeddings[index].TokenCount));
        }

        return generated;
    }
}
