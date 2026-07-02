namespace KnowledgeBase.Document.Application;

public sealed record UploadFileRequest(
    Stream Content,
    string Name,
    string FileName,
    string ContentType,
    long ContentLength);
