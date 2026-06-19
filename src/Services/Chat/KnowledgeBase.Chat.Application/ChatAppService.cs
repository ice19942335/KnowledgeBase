using System.Diagnostics;
using System.Text;
using System.Text.Json;
using KnowledgeBase.Ai;
using KnowledgeBase.Chat.Domain;
using KnowledgeBase.SharedKernel.Diagnostics;
using KnowledgeBase.Tenancy;
using Microsoft.Extensions.Options;

namespace KnowledgeBase.Chat.Application;

public sealed class ChatAppService
{
    private readonly IConversationRepository conversationRepository;
    private readonly ISearchApiClient searchApiClient;
    private readonly IChatCompletionService chatCompletion;
    private readonly ITenantContext tenantContext;
    private readonly RagOptions ragOptions;
    private readonly GeminiOptions geminiOptions;

    public ChatAppService(
        IConversationRepository conversationRepository,
        ISearchApiClient searchApiClient,
        IChatCompletionService chatCompletion,
        ITenantContext tenantContext,
        IOptions<RagOptions> ragOptions,
        IOptions<GeminiOptions> geminiOptions)
    {
        this.conversationRepository = conversationRepository;
        this.searchApiClient = searchApiClient;
        this.chatCompletion = chatCompletion;
        this.tenantContext = tenantContext;
        this.ragOptions = ragOptions.Value;
        this.geminiOptions = geminiOptions.Value;
    }

    public async Task<ChatAnswerDto> AskAsync(
        Guid conversationId,
        string question,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.RequireTenant();
        var (conversation, _) = await PrepareConversationAsync(tenantId, conversationId, question, cancellationToken);
        var contextChunks = await searchApiClient.SearchAsync(tenantId, question, cancellationToken);
        var userPrompt = BuildUserPrompt(question, contextChunks);
        var answer = await chatCompletion.CompleteAsync(ragOptions.SystemPrompt, userPrompt, cancellationToken);
        var sources = BuildSources(contextChunks);

        conversation.AddMessage(MessageRole.Assistant, answer, JsonSerializer.Serialize(sources));
        await conversationRepository.SaveChangesAsync(cancellationToken);

        return new ChatAnswerDto(conversation.Id, answer, sources);
    }

    public async Task<ChatTraceAnswerDto> AskWithTraceAsync(
        Guid conversationId,
        string question,
        CancellationToken cancellationToken)
    {
        var steps = new List<PipelineTraceStep>();
        var totalStopwatch = Stopwatch.StartNew();
        var tenantId = tenantContext.RequireTenant();

        steps.Add(new PipelineTraceStep(
            1,
            "tenant.resolve",
            "Resolve the current tenant from the request context.",
            0,
            Input: null,
            Output: new { TenantId = tenantId }));

        var conversationStopwatch = Stopwatch.StartNew();
        var (conversation, createdConversation) = await PrepareConversationAsync(
            tenantId,
            conversationId,
            question,
            cancellationToken);
        conversationStopwatch.Stop();

        steps.Add(new PipelineTraceStep(
            2,
            "conversation.prepare",
            "Load an existing conversation or create a new one and store the user message.",
            conversationStopwatch.ElapsedMilliseconds,
            Input: new { ConversationId = conversationId, Question = question },
            Output: new
            {
                ConversationId = conversation.Id,
                CreatedConversation = createdConversation,
                MessageCount = conversation.Messages.Count
            }));

        var searchTrace = await searchApiClient.SearchWithTraceAsync(tenantId, question, cancellationToken);

        steps.Add(new PipelineTraceStep(
            3,
            "search.retrieve_context",
            "Call the Search service to retrieve the most relevant chunks.",
            searchTrace.TotalDurationMs,
            Input: new { Question = question },
            Output: new
            {
                ResultCount = searchTrace.Results.Count,
                NestedSteps = searchTrace.Steps
            }));

        var promptStopwatch = Stopwatch.StartNew();
        var userPrompt = BuildUserPrompt(question, searchTrace.Results);
        promptStopwatch.Stop();

        steps.Add(new PipelineTraceStep(
            4,
            "rag.build_prompt",
            "Combine retrieved chunks with the user question for the LLM.",
            promptStopwatch.ElapsedMilliseconds,
            Input: new { Question = question, ChunkCount = searchTrace.Results.Count },
            Output: new
            {
                ragOptions.SystemPrompt,
                UserPrompt = userPrompt
            }));

        var llmStopwatch = Stopwatch.StartNew();
        var answer = await chatCompletion.CompleteAsync(ragOptions.SystemPrompt, userPrompt, cancellationToken);
        llmStopwatch.Stop();

        steps.Add(new PipelineTraceStep(
            5,
            "llm.complete",
            "Send the RAG prompt to the configured chat model.",
            llmStopwatch.ElapsedMilliseconds,
            Input: new
            {
                Provider = "Gemini",
                Model = geminiOptions.ChatModel
            },
            Output: new { Answer = answer }));

        var sources = BuildSources(searchTrace.Results);
        conversation.AddMessage(MessageRole.Assistant, answer, JsonSerializer.Serialize(sources));

        var persistStopwatch = Stopwatch.StartNew();
        await conversationRepository.SaveChangesAsync(cancellationToken);
        persistStopwatch.Stop();

        steps.Add(new PipelineTraceStep(
            6,
            "conversation.persist",
            "Save the assistant answer and source references.",
            persistStopwatch.ElapsedMilliseconds,
            Input: new { ConversationId = conversation.Id },
            Output: new { Sources = sources }));

        totalStopwatch.Stop();

        return new ChatTraceAnswerDto(
            conversation.Id,
            answer,
            sources,
            steps,
            totalStopwatch.ElapsedMilliseconds);
    }

    public async Task<IReadOnlyList<ConversationDto>> ListConversationsAsync(CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.RequireTenant();
        var conversations = await conversationRepository.ListAsync(tenantId, cancellationToken);

        return conversations.Select(c => new ConversationDto(
            c.Id, c.Title, c.CreatedAt, c.Messages.Count)).ToList();
    }

    public async Task<ConversationDetailDto?> GetConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.RequireTenant();
        var conversation = await conversationRepository.GetAsync(tenantId, conversationId, cancellationToken);

        if (conversation is null)
        {
            return null;
        }

        var messages = conversation.Messages.Select(m => new MessageDto(
            m.Id, m.Role.ToString(), m.Content, m.SourceReferences, m.CreatedAt)).ToList();

        return new ConversationDetailDto(conversation.Id, conversation.Title, conversation.CreatedAt, messages);
    }

    private async Task<(Conversation Conversation, bool Created)> PrepareConversationAsync(
        Guid tenantId,
        Guid conversationId,
        string question,
        CancellationToken cancellationToken)
    {
        var conversation = await conversationRepository.GetAsync(tenantId, conversationId, cancellationToken);
        var created = false;

        if (conversation is null)
        {
            conversation = new Conversation(tenantId, question.Length > 80 ? question[..80] + "..." : question);
            await conversationRepository.AddAsync(conversation, cancellationToken);
            created = true;
        }

        conversation.AddMessage(MessageRole.User, question);
        return (conversation, created);
    }

    private static IReadOnlyList<SourceReference> BuildSources(IReadOnlyList<SearchContextChunk> contextChunks)
    {
        return contextChunks
            .Select(chunk => new SourceReference(chunk.DocumentId, chunk.DocumentName, chunk.ChunkIndex))
            .DistinctBy(source => new { source.DocumentId, source.ChunkIndex })
            .ToList();
    }

    private static string BuildUserPrompt(string question, IReadOnlyList<SearchContextChunk> chunks)
    {
        if (chunks.Count == 0)
        {
            return $"Context: (no relevant documents found)\n\nQuestion: {question}";
        }

        var builder = new StringBuilder("Context:\n");
        for (var index = 0; index < chunks.Count; index++)
        {
            builder.AppendLine($"[{index + 1}] (Source: {chunks[index].DocumentName}) {chunks[index].Content}");
        }

        builder.AppendLine();
        builder.Append($"Question: {question}");

        return builder.ToString();
    }
}

public sealed record ChatAnswerDto(Guid ConversationId, string Answer, IReadOnlyList<SourceReference> Sources);

public sealed record SourceReference(Guid DocumentId, string DocumentName, int ChunkIndex);

public sealed record ConversationDto(Guid Id, string Title, DateTime CreatedAt, int MessageCount);

public sealed record ConversationDetailDto(
    Guid Id,
    string Title,
    DateTime CreatedAt,
    IReadOnlyList<MessageDto> Messages);

public sealed record MessageDto(
    Guid Id,
    string Role,
    string Content,
    string? SourceReferences,
    DateTime CreatedAt);
