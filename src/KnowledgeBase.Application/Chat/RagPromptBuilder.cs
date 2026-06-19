using System.Text;
using KnowledgeBase.Application.Abstractions;

namespace KnowledgeBase.Application.Chat;

/// <summary>
/// Builds the grounding prompt for retrieval-augmented generation.
/// Kept free of infrastructure so the prompt contract can be unit tested.
/// </summary>
public static class RagPromptBuilder
{
    public const string SystemPrompt =
        "You are a knowledge base assistant. Answer the user's question using only the provided context. " +
        "Do not invent information. If the answer is not contained in the context, reply exactly with \"I don't know.\". " +
        "Be concise and factual.";

    public static string BuildUserPrompt(string question, IReadOnlyList<ChunkMatch> context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);
        ArgumentNullException.ThrowIfNull(context);

        var builder = new StringBuilder();
        builder.AppendLine("Context:");

        for (var index = 0; index < context.Count; index++)
        {
            var chunk = context[index];
            builder.AppendLine($"[{index + 1}] (source: {chunk.FileName})");
            builder.AppendLine(chunk.Content);
            builder.AppendLine();
        }

        builder.AppendLine("Question:");
        builder.AppendLine(question);

        return builder.ToString().TrimEnd();
    }
}
