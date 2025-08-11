namespace SemanticChatDemo.Models;

/// <summary>
/// Represents a persisted conversation for a user
/// </summary>
public class ConversationData
{
    /// <summary>
    /// The user ID this conversation belongs to
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// When this conversation was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this conversation was last updated
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// The system prompt used for this conversation
    /// </summary>
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// All messages in this conversation
    /// </summary>
    public List<ConversationMessage> Messages { get; set; } = new();
}

/// <summary>
/// Represents a single message in a conversation
/// </summary>
public class ConversationMessage
{
    /// <summary>
    /// Unique identifier for this message
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The content of the message
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The role of the message sender (user, assistant, system)
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// When this message was created
    /// </summary>
    public DateTime Timestamp { get; set; }
}
