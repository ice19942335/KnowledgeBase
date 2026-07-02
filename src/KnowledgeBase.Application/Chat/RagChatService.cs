using KnowledgeBase.Ai;
using KnowledgeBase.Application.Abstractions;
using KnowledgeBase.Application.Common.Options;
using KnowledgeBase.Application.Search;
using Microsoft.Extensions.Options;

namespace KnowledgeBase.Application.Chat;

public sealed class RagChatService : IRagChatService
{
    private readonly IEmbeddingGenerator embeddingGenerator;
    private readonly ChunkRetrievalPipeline retrievalPipeline;
    private readonly IChatCompletionService chatCompletionService;
    private readonly RagOptions options;

    public RagChatService(
        IEmbeddingGenerator embeddingGenerator,
        ChunkRetrievalPipeline retrievalPipeline,
        IChatCompletionService chatCompletionService,
        IOptions<RagOptions> options)
    {
        this.embeddingGenerator = embeddingGenerator;
        this.retrievalPipeline = retrievalPipeline;
        this.chatCompletionService = chatCompletionService;
        this.options = options.Value;
    }

    public async Task<ChatAnswerDto> AskAsync(string question, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException("Question cannot be empty.", nameof(question));
        }

        var questionEmbedding = await embeddingGenerator.GenerateAsync(question, cancellationToken);
        var matches = await retrievalPipeline.RetrieveAsync(
            question,
            questionEmbedding.Values,
            options.ContextChunkCount,
            cancellationToken);

        if (matches.Count == 0)
        {
            return new ChatAnswerDto(options.NoAnswerResponse, Array.Empty<SourceReferenceDto>());
        }

        var userPrompt = RagPromptBuilder.BuildUserPrompt(question, matches);
        var completion = await chatCompletionService.CompleteAsync(
            RagPromptBuilder.SystemPrompt,
            userPrompt,
            cancellationToken);

        var sources = matches
            .GroupBy(match => match.DocumentId)
            .Select(group => group.First())
            .Select(match => new SourceReferenceDto(match.DocumentId, match.DocumentName, match.FileName))
            .ToList();

        return new ChatAnswerDto(completion.Text, sources);
    }
}
