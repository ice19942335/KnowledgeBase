namespace KnowledgeBase.Document.Application;

public sealed class DocumentOptions
{
    public const string SectionName = "Document";

    public int MaxBatchFileCount { get; set; } = 20;
}
