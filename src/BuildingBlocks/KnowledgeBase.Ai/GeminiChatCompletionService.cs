using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Options;

namespace KnowledgeBase.Ai;

public sealed class GeminiChatCompletionService : IChatCompletionService
{
    private readonly Client client;
    private readonly IAiAvailabilityState availabilityState;
    private readonly string chatModel;

    public GeminiChatCompletionService(
        Client client,
        IAiAvailabilityState availabilityState,
        IOptions<GeminiOptions> options)
    {
        this.client = client;
        this.availabilityState = availabilityState;
        chatModel = options.Value.ChatModel;
    }

    public async Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken)
    {
        if (!availabilityState.IsConfigured)
        {
            throw new AiNotConfiguredException();
        }

        var config = new GenerateContentConfig
        {
            SystemInstruction = new Content
            {
                Parts = [new Part { Text = systemPrompt }]
            }
        };

        var response = await client.Models.GenerateContentAsync(
            chatModel,
            userPrompt,
            config,
            cancellationToken);

        var candidate = response.Candidates?.FirstOrDefault();
        var text = candidate?.Content?.Parts?.FirstOrDefault()?.Text;

        return string.IsNullOrWhiteSpace(text) ? string.Empty : text;
    }
}
