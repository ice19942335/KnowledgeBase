using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace KnowledgeBase.Ai;

public class RerankingOptions
{
    public const string SectionName = "Search";

    public bool RerankingEnabled { get; set; } = true;

    public int RerankingCandidateLimit { get; set; } = 15;
}

public sealed partial class LlmChunkReranker : IChunkReranker
{
    private const string SystemPrompt =
        "You score how relevant each passage is to the user question. " +
        "Return JSON only: an array of objects with integer index (0-based) and score (0-10). " +
        "Higher score means more relevant. Use only passage content.";

    private readonly IChatCompletionService chatCompletionService;
    private readonly IAiAvailabilityState availabilityState;
    private readonly RerankingOptions options;

    public LlmChunkReranker(
        IChatCompletionService chatCompletionService,
        IAiAvailabilityState availabilityState,
        IOptions<RerankingOptions> options)
    {
        this.chatCompletionService = chatCompletionService;
        this.availabilityState = availabilityState;
        this.options = options.Value;
    }

    public async Task<IReadOnlyList<RankedChunkCandidate>> RerankAsync(
        string query,
        IReadOnlyList<RankedChunkCandidate> candidates,
        int finalTopK,
        CancellationToken cancellationToken)
    {
        if (!options.RerankingEnabled || !availabilityState.IsConfigured || candidates.Count == 0)
        {
            return candidates.Take(finalTopK).ToList();
        }

        var limited = candidates.Take(options.RerankingCandidateLimit).ToList();
        var userPrompt = BuildUserPrompt(query, limited);

        try
        {
            var response = await chatCompletionService.CompleteAsync(
                SystemPrompt,
                userPrompt,
                cancellationToken);

            var scores = ParseScores(response, limited.Count);
            if (scores.Count == 0)
            {
                return limited.Take(finalTopK).ToList();
            }

            return limited
                .Select((candidate, index) => new
                {
                    Candidate = candidate,
                    Score = scores.TryGetValue(index, out var score) ? score : 0
                })
                .OrderByDescending(entry => entry.Score)
                .ThenByDescending(entry => entry.Candidate.Score)
                .Take(finalTopK)
                .Select(entry => entry.Candidate with { Score = entry.Score / 10.0 })
                .ToList();
        }
        catch
        {
            return limited.Take(finalTopK).ToList();
        }
    }

    private static string BuildUserPrompt(string query, IReadOnlyList<RankedChunkCandidate> candidates)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine($"Question: {query}");
        builder.AppendLine("Passages:");

        for (var index = 0; index < candidates.Count; index++)
        {
            builder.AppendLine($"[{index}] {candidates[index].Content}");
        }

        return builder.ToString();
    }

    private static Dictionary<int, double> ParseScores(string response, int candidateCount)
    {
        var json = ExtractJsonArray(response);
        if (json is null)
        {
            return new Dictionary<int, double>();
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var scores = new Dictionary<int, double>();

            foreach (var element in document.RootElement.EnumerateArray())
            {
                if (!element.TryGetProperty("index", out var indexProperty) ||
                    !element.TryGetProperty("score", out var scoreProperty))
                {
                    continue;
                }

                var index = indexProperty.GetInt32();
                if (index < 0 || index >= candidateCount)
                {
                    continue;
                }

                scores[index] = scoreProperty.GetDouble();
            }

            return scores;
        }
        catch (JsonException)
        {
            return new Dictionary<int, double>();
        }
    }

    private static string? ExtractJsonArray(string response)
    {
        var match = JsonArrayRegex().Match(response);
        return match.Success ? match.Value : null;
    }

    [GeneratedRegex(@"\[[\s\S]*\]")]
    private static partial Regex JsonArrayRegex();
}
