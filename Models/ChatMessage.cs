namespace semantic_chat_demo.Models;

public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsStreaming { get; set; } = false;
    public bool IsComplete { get; set; } = true;
}

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
}

public class StreamingChatMessage
{
    public string MessageId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsComplete { get; set; } = false;
}
