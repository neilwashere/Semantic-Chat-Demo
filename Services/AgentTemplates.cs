using SemanticChatDemo.Models;

namespace SemanticChatDemo.Services;

/// <summary>
/// Provides predefined agent configurations for testing and demonstration
/// </summary>
public static class AgentTemplates
{
    /// <summary>
    /// Get a simple test agent team for basic conversation testing
    /// </summary>
    public static List<AgentConfiguration> GetTestAgentTeam()
    {
        return new List<AgentConfiguration>
        {
            new AgentConfiguration
            {
                Name = "CreativeAgent",
                Description = "A creative thinker who generates ideas",
                Instructions = """
                You are a creative and enthusiastic agent who loves generating ideas and proposals.
                You think outside the box and always try to come up with novel solutions.
                Be brief and focused - provide one clear idea per response.
                Show excitement for creative possibilities.
                """,
                AvatarEmoji = "🎨",
                ColorScheme = "agent-creative",
                IsActive = true
            },
            new AgentConfiguration
            {
                Name = "AnalyticalAgent", 
                Description = "A logical analyzer who evaluates ideas",
                Instructions = """
                You are an analytical and methodical agent who evaluates ideas critically.
                You examine proposals for feasibility, logic, and potential issues.
                Be constructive in your criticism and suggest specific improvements.
                Keep your analysis brief and actionable.
                """,
                AvatarEmoji = "⚖️",
                ColorScheme = "agent-analytical",
                IsActive = true
            }
        };
    }

    /// <summary>
    /// Get the CopyWriter + Reviewer team from the Semantic Kernel example
    /// </summary>
    public static List<AgentConfiguration> GetCopywriterReviewerTeam()
    {
        return new List<AgentConfiguration>
        {
            new AgentConfiguration
            {
                Name = "CopyWriter",
                Description = "A copywriter with ten years of experience",
                Instructions = """
                You are a copywriter with ten years of experience and are known for brevity and a dry humor.
                The goal is to refine and decide on the single best copy as an expert in the field.
                Only provide a single proposal per response.
                You're laser focused on the goal at hand.
                Don't waste time with chit chat.
                Consider suggestions when refining an idea.
                """,
                AvatarEmoji = "✍️",
                ColorScheme = "agent-copywriter",
                IsActive = true
            },
            new AgentConfiguration
            {
                Name = "Reviewer",
                Description = "An art director with strong opinions",
                Instructions = """
                You are an art director who has opinions about copywriting born of a love for David Ogilvy.
                The goal is to determine if the given copy is acceptable to print.
                If so, state: "I Approve".
                If not, provide insight on how to refine suggested copy without example.
                """,
                AvatarEmoji = "👔",
                ColorScheme = "agent-reviewer",
                IsActive = true
            }
        };
    }
}
