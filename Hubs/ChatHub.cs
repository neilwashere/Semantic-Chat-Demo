using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using semantic_chat_demo.Models;
using semantic_chat_demo.Services;

namespace semantic_chat_demo.Hubs;

public class ChatHub(ChatService chatService, ILogger<ChatHub> logger) : Hub
{
    public async Task SendMessage(string message)
    {
        try
        {
            logger.LogInformation("Received message from {ConnectionId}: {Message}", Context.ConnectionId, message);

            // Echo the user message immediately
            var userMessage = new ChatMessage
            {
                Content = message,
                Role = "user"
            };

            await Clients.Caller.SendAsync("ReceiveMessage", userMessage);

            // Create assistant message placeholder for streaming
            var assistantMessageId = Guid.NewGuid().ToString();
            var assistantMessage = new ChatMessage
            {
                Id = assistantMessageId,
                Content = "",
                Role = "assistant",
                IsStreaming = true,
                IsComplete = false
            };

            // Send initial streaming message
            await Clients.Caller.SendAsync("ReceiveMessage", assistantMessage);

            // Stream the response
            var fullContent = "";
            await foreach (var chunk in chatService.StreamResponseAsync(Context.ConnectionId, message))
            {
                fullContent += chunk;

                // Send streaming chunk
                var streamingUpdate = new StreamingChatMessage
                {
                    MessageId = assistantMessageId,
                    Content = chunk,
                    IsComplete = false
                };

                await Clients.Caller.SendAsync("ReceiveStreamingChunk", streamingUpdate);
            }

            // Send completion signal
            var completionUpdate = new StreamingChatMessage
            {
                MessageId = assistantMessageId,
                Content = "",
                IsComplete = true
            };

            await Clients.Caller.SendAsync("ReceiveStreamingChunk", completionUpdate);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message from {ConnectionId}: {Message}", Context.ConnectionId, message);

            var errorMessage = new ChatMessage
            {
                Content = "Sorry, I encountered an error processing your message.",
                Role = "assistant"
            };

            await Clients.Caller.SendAsync("ReceiveMessage", errorMessage);
        }
    }

    public async Task ClearHistory()
    {
        chatService.ClearHistory(Context.ConnectionId);
        await Clients.Caller.SendAsync("HistoryCleared");
        logger.LogInformation("Cleared history for connection {ConnectionId}", Context.ConnectionId);
    }

    public async Task GetHistoryInfo()
    {
        var count = chatService.GetHistoryCount(Context.ConnectionId);
        await Clients.Caller.SendAsync("HistoryInfo", new { MessageCount = count });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        chatService.ClearHistory(Context.ConnectionId);
        logger.LogInformation("Connection {ConnectionId} disconnected, cleared history", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        logger.LogInformation("User {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        logger.LogInformation("User {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
    }
}
