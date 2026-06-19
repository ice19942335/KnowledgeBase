namespace KnowledgeBase.Ingestion.Application.Abstractions;

/// <summary>
/// Splits a body of text into overlapping, search-friendly chunks.
/// </summary>
public interface ITextChunker
{
    IReadOnlyList<string> Split(string text);
}
