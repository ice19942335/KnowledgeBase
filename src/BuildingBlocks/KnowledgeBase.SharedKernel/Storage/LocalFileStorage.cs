using Microsoft.Extensions.Options;

namespace KnowledgeBase.SharedKernel.Storage;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly string rootPath;

    public LocalFileStorage(IOptions<FileStorageOptions> options)
    {
        rootPath = options.Value.RootPath;
        Directory.CreateDirectory(rootPath);
    }

    public async Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken)
    {
        var safeName = Path.GetFileName(fileName);
        var uniqueName = $"{Guid.NewGuid():N}_{safeName}";
        var fullPath = Path.Combine(rootPath, uniqueName);

        await using var target = File.Create(fullPath);
        await content.CopyToAsync(target, cancellationToken);

        return uniqueName;
    }

    public Task<Stream> OpenAsync(string storagePath, CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(rootPath, storagePath);
        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(rootPath, storagePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}
