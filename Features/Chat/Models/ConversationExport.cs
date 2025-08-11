namespace SemanticChatDemo.Features.Chat.Models;

/// <summary>
/// Model for exporting agent conversations to JSON
/// </summary>
public class ConversationExport
{
    /// <summary>
    /// When the conversation was exported
    /// </summary>
    public DateTime ExportedAt { get; set; }

    /// <summary>
    /// Display name of the agent team
    /// </summary>
    public string Team { get; set; } = string.Empty;

    /// <summary>
    /// Internal team key
    /// </summary>
    public string AgentTeam { get; set; } = string.Empty;

    /// <summary>
    /// List of agents that participated
    /// </summary>
    public List<ExportedAgent> Agents { get; set; } = new();

    /// <summary>
    /// Total number of messages in the conversation
    /// </summary>
    public int MessageCount { get; set; }

    /// <summary>
    /// The actual conversation messages
    /// </summary>
    public List<ExportedMessage> Conversation { get; set; } = new();
}

/// <summary>
/// Agent information for export
/// </summary>
public class ExportedAgent
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AvatarEmoji { get; set; } = string.Empty;
}

/// <summary>
/// Message information for export
/// </summary>
public class ExportedMessage
{
    public string AgentName { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
