using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeBase.Web;

public static class WebExtensions
{
    /// <summary>
    /// Adds controllers (string enum serialization) and ProblemDetails-based
    /// global exception handling shared by every service API.
    /// </summary>
    public static IServiceCollection AddKnowledgeBaseControllers(this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }
}
