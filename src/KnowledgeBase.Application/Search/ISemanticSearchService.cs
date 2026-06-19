namespace KnowledgeBase.Application.Search;

public interface ISemanticSearchService
{
    Task<IReadOnlyList<SearchResultDto>> SearchAsync(
        string query,
        int? topK,
        CancellationToken cancellationToken);
}
