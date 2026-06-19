using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeBase.SharedKernel.Storage;

public static class FileStorageExtensions
{
    public static IServiceCollection AddLocalFileStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<FileStorageOptions>()
            .Bind(configuration.GetSection(FileStorageOptions.SectionName));

        services.AddSingleton<IFileStorage, LocalFileStorage>();

        return services;
    }
}
