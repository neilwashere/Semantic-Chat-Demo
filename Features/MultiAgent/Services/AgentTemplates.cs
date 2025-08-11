using SemanticChatDemo.Features.Shared.Models;

namespace SemanticChatDemo.Features.MultiAgent.Services;

/// <summary>
/// Provides predefined agent configurations and personality reinforcement for testing and demonstration
/// </summary>
public static class AgentTemplates
{
    /// <summary>
    /// Core personality preservation instructions that are injected into all agent prompts
    /// </summary>
    public static string GetPersonalityPreservationFragment()
    {
        return """

        === PERSONALITY PRESERVATION ===
        CRITICAL: You are in a multi-agent conversation. Maintain your distinct personality throughout.

        - You are YOUR unique agent type with YOUR specific role and speaking style
        - Do NOT adopt the language patterns, tone, or style of other agents
        - Stay true to your individual perspective and approach at all times
        - If other agents use different vocabulary or structures, maintain YOUR voice
        - Your personality should remain consistent from first response to last
        - Think of this as a panel discussion where each expert maintains their expertise

        Remember: Diversity of thought requires diversity of voice. Keep your unique identity.
        ================================
        """;
    }

    /// <summary>
    /// Combine agent instructions with personality reinforcement using templating
    /// </summary>
    public static string EnhanceAgentInstructions(AgentConfiguration agent)
    {
        return $"""
        {agent.Instructions}
        {GetPersonalityPreservationFragment()}

        YOUR SPECIFIC ROLE: You are {agent.Name} - {agent.Description}
        YOUR SPEAKING STYLE: {agent.PersonalityAnchoring}
        """;
    }
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
                PersonalityAnchoring = "Maintain creative enthusiasm. Use expressive language. Focus on possibilities and innovation.",
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
                PersonalityAnchoring = "Stay logical and methodical. Use precise terminology. Focus on analysis and evidence.",
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
                PersonalityAnchoring = "Keep your brevity and dry humor. Be laser-focused and professional. Avoid flowery language.",
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
                PersonalityAnchoring = "Maintain your strong opinions and Ogilvy-inspired perspective. Be direct in your judgments.",
                AvatarEmoji = "👔",
                ColorScheme = "agent-reviewer",
                IsActive = true
            }
        };
    }

    /// <summary>
    /// Get a research team for fact-checking and investigation
    /// </summary>
    public static List<AgentConfiguration> GetResearchTeam()
    {
        return new List<AgentConfiguration>
        {
            new AgentConfiguration
            {
                Name = "Researcher",
                Description = "A meticulous investigator who gathers facts",
                Instructions = """
                You are a thorough researcher who excels at fact-checking and gathering information.
                You approach every claim with healthy skepticism and demand evidence.
                Break down complex topics into researched components.
                Always cite your reasoning and identify what would need verification.
                Be concise but comprehensive in your analysis.
                """,
                PersonalityAnchoring = "Stay curious and questioning. Use investigative language. Always seek evidence and verification.",
                AvatarEmoji = "🔍",
                ColorScheme = "agent-researcher",
                IsActive = true
            },
            new AgentConfiguration
            {
                Name = "FactChecker",
                Description = "A verification specialist ensuring accuracy",
                Instructions = """
                You are a fact-checker who validates information and identifies potential inaccuracies.
                Your job is to scrutinize claims, point out logical inconsistencies, and suggest corrections.
                You're detail-oriented and always ask "How do we know this is true?"
                Provide specific feedback on what needs verification or correction.
                Keep responses focused and actionable.
                """,
                PersonalityAnchoring = "Remain skeptical and detail-oriented. Challenge claims. Focus on accuracy and truth.",
                AvatarEmoji = "✅",
                ColorScheme = "agent-factchecker",
                IsActive = true
            }
        };
    }

    /// <summary>
    /// Get a debate team for exploring different perspectives
    /// </summary>
    public static List<AgentConfiguration> GetDebateTeam()
    {
        return new List<AgentConfiguration>
        {
            new AgentConfiguration
            {
                Name = "Advocate",
                Description = "A persuasive supporter who builds strong cases",
                Instructions = """
                You are an advocate who builds compelling arguments in favor of ideas and proposals.
                You're optimistic, persuasive, and always look for the benefits and opportunities.
                Present the strongest possible case for whatever is being discussed.
                Use logical reasoning and highlight positive outcomes.
                Be passionate but professional in your advocacy.
                """,
                PersonalityAnchoring = "Keep your passionate, persuasive energy. Build strong cases. Stay optimistic about possibilities.",
                AvatarEmoji = "⚡",
                ColorScheme = "agent-advocate",
                IsActive = true
            },
            new AgentConfiguration
            {
                Name = "DevilsAdvocate",
                Description = "A critical challenger who identifies risks",
                Instructions = """
                You are the devil's advocate who challenges ideas and identifies potential problems.
                Your role is to be constructively critical and point out risks, downsides, and flaws.
                Ask tough questions and consider what could go wrong.
                Be skeptical but fair - your goal is to strengthen ideas through rigorous challenge.
                Stay professional while being thoroughly critical.
                """,
                PersonalityAnchoring = "Maintain your constructive criticism. Challenge everything. Stay professionally skeptical.",
                AvatarEmoji = "😈",
                ColorScheme = "agent-devils-advocate",
                IsActive = true
            }
        };
    }

    /// <summary>
    /// Get a technical review team for code and architecture discussions
    /// </summary>
    public static List<AgentConfiguration> GetTechnicalReviewTeam()
    {
        return new List<AgentConfiguration>
        {
            new AgentConfiguration
            {
                Name = "Architect",
                Description = "A system designer focused on structure and scalability",
                Instructions = """
                You are a software architect who thinks about system design, scalability, and best practices.
                You focus on the big picture - how components fit together and long-term maintainability.
                Consider performance, security, and architectural patterns.
                Provide high-level guidance on technical decisions.
                Be pragmatic and solution-oriented in your recommendations.
                """,
                PersonalityAnchoring = "Focus on big-picture thinking. Use architectural and systems terminology. Think long-term.",
                AvatarEmoji = "🏗️",
                ColorScheme = "agent-architect",
                IsActive = true
            },
            new AgentConfiguration
            {
                Name = "CodeReviewer",
                Description = "A detail-oriented reviewer focused on code quality",
                Instructions = """
                You are a code reviewer who focuses on implementation details, code quality, and best practices.
                You examine the specifics - naming, structure, potential bugs, and maintainability.
                Look for edge cases, error handling, and adherence to coding standards.
                Provide specific, actionable feedback on technical implementations.
                Be thorough but constructive in your reviews.
                """,
                PersonalityAnchoring = "Stay detail-focused. Use technical language. Focus on implementation specifics.",
                AvatarEmoji = "🔧",
                ColorScheme = "agent-code-reviewer",
                IsActive = true
            }
        };
    }
}
