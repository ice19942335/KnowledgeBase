using System.ComponentModel.DataAnnotations;

namespace KnowledgeBase.Api.Contracts;

public sealed class ChatRequest
{
    [Required]
    public string Question { get; init; } = string.Empty;
}
