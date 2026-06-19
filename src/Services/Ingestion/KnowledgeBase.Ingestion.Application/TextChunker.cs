using System.Text;
using System.Text.RegularExpressions;
using KnowledgeBase.Ingestion.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace KnowledgeBase.Ingestion.Application;

/// <summary>
/// Word-aware chunker that produces overlapping, fixed-size text chunks.
/// The overlap preserves context across chunk boundaries for better retrieval.
/// </summary>
public sealed partial class TextChunker : ITextChunker
{
    private readonly int maxChunkSize;
    private readonly int overlapSize;

    public TextChunker(IOptions<ChunkingOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var value = options.Value;

        if (value.MaxChunkSize <= 0)
        {
            throw new ArgumentException("MaxChunkSize must be positive.", nameof(options));
        }

        if (value.OverlapSize < 0 || value.OverlapSize >= value.MaxChunkSize)
        {
            throw new ArgumentException("OverlapSize must be in the range [0, MaxChunkSize).", nameof(options));
        }

        maxChunkSize = value.MaxChunkSize;
        overlapSize = value.OverlapSize;
    }

    public IReadOnlyList<string> Split(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<string>();
        }

        var normalized = WhitespaceRegex().Replace(text, " ").Trim();

        if (normalized.Length <= maxChunkSize)
        {
            return new[] { normalized };
        }

        var words = SplitIntoBoundedWords(normalized);
        var chunks = new List<string>();
        var current = new StringBuilder();

        foreach (var word in words)
        {
            var separatorLength = current.Length > 0 ? 1 : 0;

            if (current.Length > 0 && current.Length + separatorLength + word.Length > maxChunkSize)
            {
                chunks.Add(current.ToString());

                var overlap = BuildOverlap(current.ToString());
                current.Clear();
                current.Append(overlap);
            }

            if (current.Length > 0)
            {
                current.Append(' ');
            }

            current.Append(word);
        }

        if (current.Length > 0)
        {
            chunks.Add(current.ToString());
        }

        return chunks;
    }

    private IEnumerable<string> SplitIntoBoundedWords(string normalized)
    {
        foreach (var word in normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (word.Length <= maxChunkSize)
            {
                yield return word;
                continue;
            }

            for (var start = 0; start < word.Length; start += maxChunkSize)
            {
                var length = Math.Min(maxChunkSize, word.Length - start);
                yield return word.Substring(start, length);
            }
        }
    }

    private string BuildOverlap(string chunk)
    {
        if (overlapSize == 0 || chunk.Length <= overlapSize)
        {
            return overlapSize == 0 ? string.Empty : chunk;
        }

        var tail = chunk[^overlapSize..];
        var firstSpace = tail.IndexOf(' ');

        return firstSpace >= 0 ? tail[(firstSpace + 1)..] : tail;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
