using KnowledgeBase.Ai;
using KnowledgeBase.Application.Abstractions;
using KnowledgeBase.Domain.Documents;
using Microsoft.Extensions.Logging;

namespace KnowledgeBase.Application.Documents;

public sealed class DocumentService : IDocumentService
{
    private readonly IDocumentRepository repository;
    private readonly ITextExtractionService extractionService;
    private readonly ITextChunker chunker;
    private readonly IEmbeddingGenerator embeddingGenerator;
    private readonly ILogger<DocumentService> logger;

    public DocumentService(
        IDocumentRepository repository,
        ITextExtractionService extractionService,
        ITextChunker chunker,
        IEmbeddingGenerator embeddingGenerator,
        ILogger<DocumentService> logger)
    {
        this.repository = repository;
        this.extractionService = extractionService;
        this.chunker = chunker;
        this.embeddingGenerator = embeddingGenerator;
        this.logger = logger;
    }

    public async Task<DocumentDto> UploadAsync(UploadDocumentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var displayName = string.IsNullOrWhiteSpace(command.DisplayName)
            ? command.FileName
            : command.DisplayName;

        var document = new Document(displayName, command.FileName, command.ContentType);
        await repository.AddAsync(document, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        try
        {
            document.MarkProcessing();

            var text = await extractionService.ExtractAsync(
                command.Content,
                command.FileName,
                command.ContentType,
                cancellationToken);

            document.SetExtractedText(text);

            await BuildChunksAsync(document, text, cancellationToken);

            document.MarkProcessed();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process document {DocumentId}", document.Id);
            document.MarkFailed(ex.Message);
            await repository.SaveChangesAsync(cancellationToken);
            throw;
        }

        await repository.SaveChangesAsync(cancellationToken);

        return DocumentDto.FromDomain(document);
    }

    public async Task<IReadOnlyList<DocumentDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var documents = await repository.GetAllAsync(cancellationToken);
        return documents.Select(DocumentDto.FromDomain).ToList();
    }

    public async Task<DocumentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(id, cancellationToken);
        return document is null ? null : DocumentDto.FromDomain(document);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(id, cancellationToken);

        if (document is null)
        {
            return false;
        }

        repository.Remove(document);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task BuildChunksAsync(Document document, string text, CancellationToken cancellationToken)
    {
        var chunkTexts = chunker.Split(text);

        if (chunkTexts.Count == 0)
        {
            logger.LogWarning("Document {DocumentId} produced no chunks; nothing to embed.", document.Id);
            return;
        }

        var embeddings = await embeddingGenerator.GenerateAsync(chunkTexts, cancellationToken);

        if (embeddings.Count != chunkTexts.Count)
        {
            throw new InvalidOperationException(
                $"Embedding count ({embeddings.Count}) does not match chunk count ({chunkTexts.Count}).");
        }

        var chunks = new List<DocumentChunk>(chunkTexts.Count);

        for (var index = 0; index < chunkTexts.Count; index++)
        {
            var chunk = new DocumentChunk(document.Id, index, chunkTexts[index]);
            chunk.SetEmbedding(embeddings[index]);
            chunks.Add(chunk);
        }

        document.ReplaceChunks(chunks);
    }
}
