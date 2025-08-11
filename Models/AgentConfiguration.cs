namespace SemanticChatDemo.Models;

/// <summary>
/// Configuration for an AI agent including persona and visual styling
/// </summary>
public class AgentConfiguration
{
    /// <summary>
    /// Unique name for the agent
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Brief description of the agent's role
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Detailed instructions defining the agent's behavior and personality
    /// </summary>
    public string Instructions { get; set; } = string.Empty;

    /// <summary>
    /// Specific personality traits and speaking style to maintain consistency
    /// </summary>
    public string PersonalityAnchoring { get; set; } = string.Empty;

    /// <summary>
    /// Emoji used as the agent's avatar
    /// </summary>
    public string AvatarEmoji { get; set; } = string.Empty;

    /// <summary>
    /// CSS class name for the agent's color scheme
    /// </summary>
    public string ColorScheme { get; set; } = string.Empty;

    /// <summary>
    /// Whether this agent is currently active
    /// </summary>
    public bool IsActive { get; set; } = false;
}
