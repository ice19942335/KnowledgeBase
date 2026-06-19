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

    public async Task<IReadOnlyList<SearchContextChunk>> SearchAsync(
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

        var results = await response.Content.ReadFromJsonAsync<List<SearchContextChunk>>(cancellationToken);
        return results ?? [];
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

        var results = trace.Results
            .Select(result => new SearchContextChunk(
                result.DocumentId,
                result.DocumentName,
                result.ChunkIndex,
                result.Content,
                result.Score))
            .ToList();

        return new SearchTraceResponse(trace.Query, results, trace.Steps, trace.TotalDurationMs);
    }

    private sealed record SearchTraceApiResponse(
        string Query,
        IReadOnlyList<SearchContextChunk> Results,
        IReadOnlyList<SharedKernel.Diagnostics.PipelineTraceStep> Steps,
        long TotalDurationMs);
}
