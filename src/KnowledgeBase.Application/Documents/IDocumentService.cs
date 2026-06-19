namespace KnowledgeBase.Application.Documents;

public interface IDocumentService
{
    Task<DocumentDto> UploadAsync(UploadDocumentCommand command, CancellationToken cancellationToken);

    Task<IReadOnlyList<DocumentDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<DocumentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
