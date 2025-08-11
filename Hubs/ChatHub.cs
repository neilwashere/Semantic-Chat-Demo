using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using SemanticChatDemo.Models;
using SemanticChatDemo.Services;

namespace SemanticChatDemo.Hubs;

public class ChatHub(ChatService chatService, ILogger<ChatHub> logger) : Hub
{
    public async Task SendMessage(string userId, string message)
    {
        try
        {
            logger.LogInformation("Received message from user {UserId} (connection {ConnectionId}): {Message}",
                userId, Context.ConnectionId, message);

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
            await foreach (var chunk in chatService.StreamResponseAsync(userId, message))
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
            logger.LogError(ex, "Error processing message from user {UserId} (connection {ConnectionId}): {Message}",
                userId, Context.ConnectionId, message);

            var errorMessage = new ChatMessage
            {
                Content = "Sorry, I encountered an error processing your message.",
                Role = "assistant"
            };

            await Clients.Caller.SendAsync("ReceiveMessage", errorMessage);
        }
    }

    public async Task ClearHistory(string userId)
    {
        await chatService.ClearHistory(userId);
        await Clients.Caller.SendAsync("HistoryCleared");
        logger.LogInformation("Cleared history for user {UserId} (connection {ConnectionId})", userId, Context.ConnectionId);
    }

    public async Task GetHistoryInfo(string userId)
    {
        var count = chatService.GetHistoryCount(userId);
        await Clients.Caller.SendAsync("HistoryInfo", new { MessageCount = count });
    }

    public async Task LoadExistingMessages(string userId)
    {
        try
        {
            var existingMessages = await chatService.GetExistingMessagesAsync(userId);
            await Clients.Caller.SendAsync("ExistingMessagesLoaded", existingMessages);
            logger.LogInformation("Loaded {MessageCount} existing messages for user {UserId}", 
                existingMessages.Count, userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading existing messages for user {UserId}", userId);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Note: We no longer clear history on disconnect since it's tied to userId, not connectionId
        logger.LogInformation("Connection {ConnectionId} disconnected", Context.ConnectionId);
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
