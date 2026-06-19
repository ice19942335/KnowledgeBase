using System.ComponentModel.DataAnnotations;

namespace KnowledgeBase.Api.Contracts;

public sealed class SearchRequest
{
    [Required]
    public string Query { get; init; } = string.Empty;

    public int? TopK { get; init; }
}
