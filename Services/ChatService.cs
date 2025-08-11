using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Collections.Concurrent;

namespace SemanticChatDemo.Services;

public class ChatService(Kernel kernel, ILogger<ChatService> logger)
{
    // Store chat history per connection ID
    private readonly ConcurrentDictionary<string, ChatHistory> conversations = new();

    public async Task<ChatHistory> GetOrCreateChatHistoryAsync(string connectionId)
    {
        if (conversations.TryGetValue(connectionId, out var chatHistory))
        {
            return chatHistory;
        }

        var systemPromptTemplate = await File.ReadAllTextAsync("Prompts/SystemPrompt.txt");

        var prompt = await kernel.CreateFunctionFromPrompt(systemPromptTemplate)
            .InvokeAsync<string>(kernel, new() {
                { "current_date", DateTime.UtcNow.ToString("R") }
            });

        var newChatHistory = new ChatHistory(prompt);
        conversations.TryAdd(connectionId, newChatHistory);
        return newChatHistory;
    }

    public async IAsyncEnumerable<string> StreamResponseAsync(string connectionId, string userMessage)
    {
        var chatHistory = await GetOrCreateChatHistoryAsync(connectionId);

        // Add user message to history
        chatHistory.AddUserMessage(userMessage);

        logger.LogInformation("Streaming response for connection {ConnectionId}, message: {Message}",
            connectionId, userMessage);

        // Get streaming enumerable first, handle errors separately
        var streamingResponse = GetStreamingResponseSafely(chatHistory, connectionId);

        var fullResponse = string.Empty;
        await foreach (var chunk in streamingResponse)
        {
            if (chunk.StartsWith("ERROR:"))
            {
                yield return chunk.Substring(6); // Remove ERROR: prefix
                yield break;
            }

            fullResponse += chunk;
            yield return chunk;
        }

        // Add complete response to history if successful
        AddToHistorySafely(chatHistory, fullResponse, connectionId);
    }

    private async IAsyncEnumerable<string> GetStreamingResponseSafely(ChatHistory chatHistory, string connectionId)
    {
        IAsyncEnumerable<StreamingChatMessageContent>? streamingResponse = null;

        // Try to start streaming outside of yield context
        streamingResponse = TryStartStreamingChat(chatHistory, connectionId);

        if (streamingResponse == null)
        {
            yield return "ERROR:Sorry, I encountered an error while processing your message.";
            yield break;
        }

        // Process the stream
        await foreach (var chunk in streamingResponse)
        {
            var content = chunk.Content;
            if (!string.IsNullOrEmpty(content))
            {
                yield return content;
            }
        }
    }

    private IAsyncEnumerable<StreamingChatMessageContent>? TryStartStreamingChat(ChatHistory chatHistory, string connectionId)
    {
        try
        {
            // Get the chat completion service from the kernel
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            // Set up execution settings to enable automatic function calling
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            // Get streaming response with automatic function calling
            return chatCompletionService.GetStreamingChatMessageContentsAsync(
                chatHistory,
                executionSettings,
                kernel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting streaming chat completion for connection {ConnectionId}", connectionId);
            return null;
        }
    }

    private void AddToHistorySafely(ChatHistory chatHistory, string response, string connectionId)
    {
        if (!string.IsNullOrEmpty(response))
        {
            try
            {
                chatHistory.AddAssistantMessage(response);
                logger.LogInformation("Added response to history for connection {ConnectionId}", connectionId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding response to history for connection {ConnectionId}", connectionId);
            }
        }
    }

    public void ClearHistory(string connectionId)
    {
        conversations.TryRemove(connectionId, out _);
        logger.LogInformation("Cleared chat history for connection {ConnectionId}", connectionId);
    }

    public int GetHistoryCount(string connectionId)
    {
        if (conversations.TryGetValue(connectionId, out var history))
        {
            return history.Count;
        }
        return 0;
    }
}
