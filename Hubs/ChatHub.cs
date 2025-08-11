using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using semantic_chat_demo.Models;

namespace semantic_chat_demo.Hubs;

public class ChatHub : Hub
{
    private readonly Kernel _kernel;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(Kernel kernel, ILogger<ChatHub> logger)
    {
        _kernel = kernel;
        _logger = logger;
    }

    public async Task SendMessage(string message)
    {
        try
        {
            _logger.LogInformation("Received message: {Message}", message);

            // Echo the user message to all clients
            var userMessage = new ChatMessage
            {
                Content = message,
                Role = "user"
            };

            await Clients.All.SendAsync("ReceiveMessage", userMessage);

            // Get AI response (basic, non-streaming for now)
            var response = await _kernel.InvokePromptAsync(message);

            var assistantMessage = new ChatMessage
            {
                Content = response.ToString(),
                Role = "assistant"
            };

            await Clients.All.SendAsync("ReceiveMessage", assistantMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message: {Message}", message);

            var errorMessage = new ChatMessage
            {
                Content = "Sorry, I encountered an error processing your message.",
                Role = "assistant"
            };

            await Clients.All.SendAsync("ReceiveMessage", errorMessage);
        }
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
    }
}
