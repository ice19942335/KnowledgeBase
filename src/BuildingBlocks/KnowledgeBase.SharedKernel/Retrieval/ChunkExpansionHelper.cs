namespace KnowledgeBase.SharedKernel.Retrieval;

public static class ChunkExpansionHelper
{
    public const double NeighborScoreFactor = 0.85;

    public static IReadOnlyList<ChunkLocator> CollectNeighborLocators(
        IEnumerable<ChunkLocator> seeds,
        int radius)
    {
        if (radius <= 0)
        {
            return Array.Empty<ChunkLocator>();
        }

        var seen = new HashSet<(Guid DocumentId, int ChunkIndex)>();
        var locators = new List<ChunkLocator>();

        foreach (var seed in seeds)
        {
            for (var index = seed.ChunkIndex - radius; index <= seed.ChunkIndex + radius; index++)
            {
                if (index < 0)
                {
                    continue;
                }

                var key = (seed.DocumentId, index);
                if (seen.Add(key))
                {
                    locators.Add(new ChunkLocator(seed.DocumentId, index));
                }
            }
        }

        return locators;
    }

    public static IReadOnlyList<RankedChunkHit> Expand(
        IReadOnlyList<RankedChunkHit> seeds,
        IReadOnlyList<RankedChunkHit> neighbors,
        int radius)
    {
        if (radius <= 0 || seeds.Count == 0)
        {
            return seeds;
        }

        var seedScores = seeds.ToDictionary(
            hit => (hit.DocumentId, hit.ChunkIndex),
            hit => hit.Score);

        var merged = seeds.ToDictionary(
            hit => (hit.DocumentId, hit.ChunkIndex),
            hit => hit);

        foreach (var neighbor in neighbors)
        {
            var key = (neighbor.DocumentId, neighbor.ChunkIndex);
            if (merged.ContainsKey(key))
            {
                continue;
            }

            var parentScore = FindParentScore(seedScores, neighbor.DocumentId, neighbor.ChunkIndex, radius);
            if (parentScore <= 0)
            {
                continue;
            }

            merged[key] = neighbor with { Score = parentScore * NeighborScoreFactor };
        }

        return merged.Values
            .OrderByDescending(hit => hit.Score)
            .ThenBy(hit => hit.ChunkIndex)
            .ToList();
    }

    public static IReadOnlyList<RankedChunkHit> FillContiguousGaps(
        IReadOnlyList<RankedChunkHit> selected,
        IReadOnlyList<RankedChunkHit> pool,
        int maxTotal)
    {
        if (selected.Count == 0 || pool.Count == 0 || maxTotal <= 0)
        {
            return selected;
        }

        var poolByKey = pool.ToDictionary(hit => (hit.DocumentId, hit.ChunkIndex));
        var merged = selected.ToDictionary(hit => (hit.DocumentId, hit.ChunkIndex));

        foreach (var group in selected.GroupBy(hit => hit.DocumentId))
        {
            var minIndex = group.Min(hit => hit.ChunkIndex);
            var maxIndex = group.Max(hit => hit.ChunkIndex);

            for (var index = minIndex; index <= maxIndex; index++)
            {
                var key = (group.Key, index);
                if (merged.ContainsKey(key) || !poolByKey.TryGetValue(key, out var pooled))
                {
                    continue;
                }

                merged[key] = pooled with { Score = group.Min(hit => hit.Score) * NeighborScoreFactor };
            }
        }

        return merged.Values
            .OrderByDescending(hit => hit.Score)
            .ThenBy(hit => hit.ChunkIndex)
            .Take(maxTotal)
            .ToList();
    }

    private static double FindParentScore(
        IReadOnlyDictionary<(Guid DocumentId, int ChunkIndex), double> seedScores,
        Guid documentId,
        int chunkIndex,
        int radius)
    {
        var best = 0.0;

        for (var offset = -radius; offset <= radius; offset++)
        {
            if (offset == 0)
            {
                continue;
            }

            var candidateIndex = chunkIndex + offset;
            if (candidateIndex < 0)
            {
                continue;
            }

            if (seedScores.TryGetValue((documentId, candidateIndex), out var score))
            {
                best = Math.Max(best, score);
            }
        }

        return best;
    }
}
