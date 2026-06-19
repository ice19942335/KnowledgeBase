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
    private readonly IAiAvailabilityState aiAvailabilityState;

    public IngestionService(
        IFileStorage fileStorage,
        ITextExtractionService textExtractionService,
        ITextChunker textChunker,
        IEmbeddingGenerator embeddingGenerator,
        IAiAvailabilityState aiAvailabilityState)
    {
        this.fileStorage = fileStorage;
        this.textExtractionService = textExtractionService;
        this.textChunker = textChunker;
        this.embeddingGenerator = embeddingGenerator;
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

        var embeddings = await embeddingGenerator.GenerateAsync(chunks, cancellationToken);

        var generated = new List<GeneratedChunk>(chunks.Count);
        for (var index = 0; index < chunks.Count; index++)
        {
            generated.Add(new GeneratedChunk(index, chunks[index], embeddings[index]));
        }

        return generated;
    }
}
