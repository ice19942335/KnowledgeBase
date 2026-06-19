namespace KnowledgeBase.SharedKernel.Storage;

/// <summary>
/// Abstraction over blob storage for uploaded document files. The MVP uses a
/// shared local directory; the same contract can be backed by S3/Azure Blob.
/// </summary>
public interface IFileStorage
{
    Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken);

    Task<Stream> OpenAsync(string storagePath, CancellationToken cancellationToken);

    Task DeleteAsync(string storagePath, CancellationToken cancellationToken);
}
