using Microsoft.Extensions.Options;

namespace KnowledgeBase.Ai;

public sealed class DocumentSummaryGenerator : IDocumentSummaryGenerator
{
    private const string SystemPrompt =
        "You write concise document summaries for search indexing. " +
        "Use only facts present in the provided text. Do not invent details. " +
        "Reply with plain text only, no markdown.";

    private readonly IChatCompletionService chatCompletionService;
    private readonly IAiAvailabilityState availabilityState;
    private readonly ContextualEmbeddingOptions options;

    public DocumentSummaryGenerator(
        IChatCompletionService chatCompletionService,
        IAiAvailabilityState availabilityState,
        IOptions<ContextualEmbeddingOptions> options)
    {
        this.chatCompletionService = chatCompletionService;
        this.availabilityState = availabilityState;
        this.options = options.Value;
    }

    public async Task<string> GenerateAsync(
        string documentName,
        string documentText,
        CancellationToken cancellationToken)
    {
        if (!options.Enabled)
        {
            return string.Empty;
        }

        var normalizedText = documentText.Trim();
        if (normalizedText.Length == 0)
        {
            return string.Empty;
        }

        if (normalizedText.Length <= options.ShortDocumentCharacterThreshold)
        {
            return normalizedText;
        }

        if (!availabilityState.IsConfigured)
        {
            return TruncateForSummary(normalizedText);
        }

        var input = normalizedText.Length > options.MaxSummaryInputCharacters
            ? normalizedText[..options.MaxSummaryInputCharacters]
            : normalizedText;

        var userPrompt =
            $"Document title: {documentName}\n" +
            $"Write a summary in at most {options.MaxSummaryWords} words.\n\n" +
            $"Text:\n{input}";

        var completion = await chatCompletionService.CompleteAsync(
            SystemPrompt,
            userPrompt,
            cancellationToken);

        return string.IsNullOrWhiteSpace(completion.Text)
            ? TruncateForSummary(normalizedText)
            : completion.Text.Trim();
    }

    private string TruncateForSummary(string text)
    {
        var maxLength = Math.Min(options.MaxSummaryInputCharacters, 500);
        return text.Length <= maxLength ? text : text[..maxLength];
    }
}
