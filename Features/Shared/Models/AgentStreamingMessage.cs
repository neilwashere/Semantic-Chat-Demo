namespace SemanticChatDemo.Features.Shared.Models;

/// <summary>
/// Represents a streaming chunk from an agent message
/// </summary>
public class AgentStreamingMessage
{
    /// <summary>
    /// ID of the message being streamed
    /// </summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Content chunk being streamed
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the final chunk
    /// </summary>
    public bool IsComplete { get; set; } = false;
}
