namespace SemanticChatDemo.Features.Shared.Models;

/// <summary>
/// Represents a message from an AI agent in a multi-agent conversation
/// </summary>
public class AgentMessage
{
    /// <summary>
    /// Unique identifier for this message
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The content of the message
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Name of the agent who sent this message
    /// </summary>
    public string AgentName { get; set; } = string.Empty;

    /// <summary>
    /// When this message was created
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether this message has finished streaming
    /// </summary>
    public bool IsComplete { get; set; } = false;

    /// <summary>
    /// Whether this message is currently being streamed
    /// </summary>
    public bool IsStreaming { get; set; } = false;

    /// <summary>
    /// Type of message (agent, user, system)
    /// </summary>
    public string MessageType { get; set; } = "agent";
}
