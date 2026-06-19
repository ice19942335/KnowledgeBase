namespace KnowledgeBase.Search.Application;

public static class KeywordSearchScorer
{
    public static IReadOnlyList<string> Tokenize(string query)
    {
        return query
            .Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(token => token.Trim('.', ',', '?', '!', ';', ':', '"', '\'', '(', ')'))
            .Where(token => token.Length > 2)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static double Score(string content, string query, IReadOnlyList<string> terms)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return 0;
        }

        var normalizedContent = content.ToLowerInvariant();
        var normalizedQuery = query.Trim().ToLowerInvariant();

        if (normalizedQuery.Length > 0 && normalizedContent.Contains(normalizedQuery, StringComparison.Ordinal))
        {
            return 1.0;
        }

        if (terms.Count == 0)
        {
            return 0;
        }

        var matchedTerms = terms.Count(term =>
            normalizedContent.Contains(term.ToLowerInvariant(), StringComparison.Ordinal));

        return matchedTerms / (double)terms.Count;
    }
}
