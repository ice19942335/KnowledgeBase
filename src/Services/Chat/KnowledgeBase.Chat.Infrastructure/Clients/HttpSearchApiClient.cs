using System.Net.Http.Json;
using KnowledgeBase.Chat.Application;
using KnowledgeBase.Tenancy;

namespace KnowledgeBase.Chat.Infrastructure.Clients;

/// <summary>
/// Calls the Search service's /api/search endpoint via HTTP (service discovery).
/// Propagates X-Tenant-Id on the outgoing request.
/// </summary>
public sealed class HttpSearchApiClient : ISearchApiClient
{
    private readonly HttpClient httpClient;

    public HttpSearchApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<SearchApiResult> SearchAsync(
        Guid tenantId,
        string query,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/search")
        {
            Content = JsonContent.Create(new { Query = query })
        };

        request.Headers.Add(TenantConstants.TenantHeader, tenantId.ToString());

        var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<SearchQueryApiResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Search response was empty.");

        return new SearchApiResult(
            MapResults(payload.Results),
            payload.TokenUsage);
    }

    public async Task<SearchTraceResponse> SearchWithTraceAsync(
        Guid tenantId,
        string query,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/search/trace")
        {
            Content = JsonContent.Create(new { Query = query })
        };

        request.Headers.Add(TenantConstants.TenantHeader, tenantId.ToString());

        var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var trace = await response.Content.ReadFromJsonAsync<SearchTraceApiResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Search trace response was empty.");

        return new SearchTraceResponse(
            trace.Query,
            MapResults(trace.Results),
            trace.Steps,
            trace.TotalDurationMs,
            trace.TokenUsage);
    }

    private static IReadOnlyList<SearchContextChunk> MapResults(IReadOnlyList<SearchResultApiModel> results)
    {
        return results
            .Select(result => new SearchContextChunk(
                result.DocumentId,
                result.DocumentName,
                result.ChunkIndex,
                result.Content,
                result.Score,
                result.EmbeddingTokenCount))
            .ToList();
    }

    private sealed record SearchQueryApiResponse(
        IReadOnlyList<SearchResultApiModel> Results,
        SharedKernel.Diagnostics.TokenUsageSummary TokenUsage);

    private sealed record SearchTraceApiResponse(
        string Query,
        IReadOnlyList<SearchResultApiModel> Results,
        IReadOnlyList<SharedKernel.Diagnostics.PipelineTraceStep> Steps,
        long TotalDurationMs,
        SharedKernel.Diagnostics.TokenUsageSummary TokenUsage);

    private sealed record SearchResultApiModel(
        Guid DocumentId,
        string DocumentName,
        int ChunkIndex,
        string Content,
        double Score,
        int EmbeddingTokenCount);
}
