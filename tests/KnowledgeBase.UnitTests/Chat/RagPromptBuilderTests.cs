using KnowledgeBase.Application.Abstractions;
using KnowledgeBase.Application.Chat;
using Xunit;

namespace KnowledgeBase.UnitTests.Chat;

public sealed class RagPromptBuilderTests
{
    [Fact]
    public void SystemPrompt_InstructsModelToAdmitWhenUnknown()
    {
        Assert.Contains("I don't know.", RagPromptBuilder.SystemPrompt);
        Assert.Contains("only", RagPromptBuilder.SystemPrompt);
    }

    [Fact]
    public void BuildUserPrompt_IncludesQuestionAndEveryContextChunk()
    {
        var context = new List<ChunkMatch>
        {
            new(Guid.NewGuid(), "HR Policy", "hr.pdf", 0, "25 vacation days", 0.95),
            new(Guid.NewGuid(), "Handbook", "handbook.pdf", 1, "annual leave rules", 0.80)
        };

        var prompt = RagPromptBuilder.BuildUserPrompt("How many vacation days?", context);

        Assert.Contains("How many vacation days?", prompt);
        Assert.Contains("25 vacation days", prompt);
        Assert.Contains("annual leave rules", prompt);
        Assert.Contains("hr.pdf", prompt);
        Assert.Contains("handbook.pdf", prompt);
    }

    [Fact]
    public void BuildUserPrompt_WithBlankQuestion_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => RagPromptBuilder.BuildUserPrompt(" ", Array.Empty<ChunkMatch>()));
    }
}
