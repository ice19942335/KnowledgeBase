using KnowledgeBase.SharedKernel.TextProcessing;

namespace KnowledgeBase.Application.Abstractions;

/// <summary>
/// Splits a body of text into overlapping, search-friendly chunks.
/// </summary>
public interface ITextChunker
{
    IReadOnlyList<TextChunk> Split(string text);
}
