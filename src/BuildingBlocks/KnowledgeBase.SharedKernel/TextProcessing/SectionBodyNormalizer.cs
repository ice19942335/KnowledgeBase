namespace KnowledgeBase.SharedKernel.TextProcessing;

public static class SectionBodyNormalizer
{
    public static string PreserveLines(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var lines = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Length > 0);

        return string.Join('\n', lines);
    }
}
