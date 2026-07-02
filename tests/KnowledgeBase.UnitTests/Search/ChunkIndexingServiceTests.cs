using KnowledgeBase.Contracts;
using KnowledgeBase.Search.Application;
using KnowledgeBase.Search.Domain;
using NSubstitute;
using Pgvector;

namespace KnowledgeBase.UnitTests.Search;

public sealed class ChunkIndexingServiceTests
{
    private readonly IChunkRepository chunkRepository = Substitute.For<IChunkRepository>();
    private readonly Guid tenantId = Guid.NewGuid();
    private readonly Guid documentId = Guid.NewGuid();

    private ChunkIndexingService CreateSut() => new(chunkRepository);

    [Fact]
    public async Task IndexAsync_RemovesExistingChunksBeforeAddingNewOnes()
    {
        var message = new ChunksGenerated(
            documentId,
            tenantId,
            "Policy",
            "policy.md",
            [
                new GeneratedChunk(0, "Chunk A", [0.1f, 0.2f], 10),
            ]);

        var sut = CreateSut();

        await sut.IndexAsync(message, CancellationToken.None);

        await chunkRepository.Received(1).RemoveByDocumentAsync(tenantId, documentId, Arg.Any<CancellationToken>());
        await chunkRepository.Received(1).AddRangeAsync(
            Arg.Is<IEnumerable<SearchableChunk>>(chunks => chunks.Count() == 1),
            Arg.Any<CancellationToken>());
        await chunkRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAsync_DeletesChunksForDocument()
    {
        var sut = CreateSut();

        await sut.RemoveAsync(tenantId, documentId, CancellationToken.None);

        await chunkRepository.Received(1).RemoveByDocumentAsync(tenantId, documentId, Arg.Any<CancellationToken>());
        await chunkRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
