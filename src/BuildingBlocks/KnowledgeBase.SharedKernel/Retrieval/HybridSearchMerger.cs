namespace KnowledgeBase.SharedKernel.Retrieval;

public static class HybridSearchMerger
{
    public static IReadOnlyList<RankedChunkHit> Merge(
        IReadOnlyList<RankedChunkHit> vectorResults,
        IReadOnlyList<RankedChunkHit> keywordResults,
        int rrfK = 60)
    {
        var scores = new Dictionary<(Guid DocumentId, int ChunkIndex), (RankedChunkHit Hit, double Score)>();

        ApplyRankContribution(vectorResults, rrfK, scores);
        ApplyRankContribution(keywordResults, rrfK, scores);

        return scores.Values
            .OrderByDescending(entry => entry.Score)
            .ThenBy(entry => entry.Hit.ChunkIndex)
            .Select(entry => entry.Hit with { Score = entry.Score })
            .ToList();
    }

    private static void ApplyRankContribution(
        IReadOnlyList<RankedChunkHit> results,
        int rrfK,
        Dictionary<(Guid DocumentId, int ChunkIndex), (RankedChunkHit Hit, double Score)> scores)
    {
        for (var rank = 0; rank < results.Count; rank++)
        {
            var hit = results[rank];
            var key = (hit.DocumentId, hit.ChunkIndex);
            var contribution = 1.0 / (rrfK + rank + 1);

            if (scores.TryGetValue(key, out var existing))
            {
                scores[key] = (PreferRicherMetadata(existing.Hit, hit), existing.Score + contribution);
            }
            else
            {
                scores[key] = (hit, contribution);
            }
        }
    }

    private static RankedChunkHit PreferRicherMetadata(RankedChunkHit left, RankedChunkHit right)
    {
        if (left.EmbeddingTokenCount == 0 && right.EmbeddingTokenCount > 0)
        {
            return right with { Score = left.Score };
        }

        if (string.IsNullOrWhiteSpace(left.FileName) && !string.IsNullOrWhiteSpace(right.FileName))
        {
            return right with { Score = left.Score };
        }

        return left;
    }
}
