namespace KnowledgeBase.Document.Application;

public sealed record DocumentContent(
    string FileName,
    string ContentType,
    Stream Content);
