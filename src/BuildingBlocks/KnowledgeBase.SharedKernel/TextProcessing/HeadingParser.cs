using System.Text;
using System.Text.RegularExpressions;

namespace KnowledgeBase.SharedKernel.TextProcessing;

public static partial class HeadingParser
{
    private const int MaxUppercaseHeadingLength = 80;

    public static IReadOnlyList<TextSection> SplitIntoSections(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<TextSection>();
        }

        var sections = new List<TextSection>();
        var currentTitle = (string?)null;
        var body = new StringBuilder();

        foreach (var rawLine in text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            var line = rawLine.Trim();

            if (line.Length == 0)
            {
                continue;
            }

            if (TryParseHeading(line, out var heading))
            {
                FlushSection(sections, currentTitle, body);
                currentTitle = heading;
                continue;
            }

            if (body.Length > 0)
            {
                body.Append('\n');
            }

            body.Append(line);
        }

        FlushSection(sections, currentTitle, body);

        if (sections.Count == 0)
        {
            return [new TextSection(null, text.Trim())];
        }

        return sections;
    }

    private static void FlushSection(List<TextSection> sections, string? title, StringBuilder body)
    {
        var content = body.ToString().Trim();

        if (content.Length == 0 && title is null)
        {
            return;
        }

        sections.Add(new TextSection(title, content));
        body.Clear();
    }

    private static bool TryParseHeading(string line, out string heading)
    {
        var markdownMatch = MarkdownHeadingRegex().Match(line);
        if (markdownMatch.Success)
        {
            heading = markdownMatch.Groups[1].Value.Trim();
            return heading.Length > 0;
        }

        var numberedMatch = NumberedHeadingRegex().Match(line);
        if (numberedMatch.Success)
        {
            heading = line.Trim();
            return true;
        }

        if (IsUppercaseHeadingCandidate(line))
        {
            heading = line;
            return true;
        }

        heading = string.Empty;
        return false;
    }

    private static bool IsUppercaseHeadingCandidate(string line)
    {
        if (line.Length > MaxUppercaseHeadingLength || line.Length < 4)
        {
            return false;
        }

        var letters = line.Where(char.IsLetter).ToList();
        if (letters.Count < 3)
        {
            return false;
        }

        return letters.All(char.IsUpper);
    }

    [GeneratedRegex(@"^#{1,6}\s+(.+)$")]
    private static partial Regex MarkdownHeadingRegex();

    [GeneratedRegex(@"^\d+(?:\.\d+)*\.\s+\S")]
    private static partial Regex NumberedHeadingRegex();
}
