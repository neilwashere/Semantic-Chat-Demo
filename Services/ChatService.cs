using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Collections.Concurrent;
using System.Text;

namespace semantic_chat_demo.Services;

public class ChatService(Kernel kernel, ILogger<ChatService> logger)
{

    // Store chat history per connection ID
    private readonly ConcurrentDictionary<string, ChatHistory> _conversations = new();

    public ChatHistory GetOrCreateChatHistory(string connectionId)
    {
        return _conversations.GetOrAdd(connectionId, _ => new ChatHistory());
    }

    public async IAsyncEnumerable<string> StreamResponseAsync(string connectionId, string userMessage)
    {
        var chatHistory = GetOrCreateChatHistory(connectionId);

        // Add user message to history
        chatHistory.AddUserMessage(userMessage);

        logger.LogInformation("Streaming response for connection {ConnectionId}, message: {Message}",
            connectionId, userMessage);

        var prompt = BuildPromptFromHistory(chatHistory);
        var fullResponse = string.Empty;

        // Get streaming enumerable first, handle errors separately
        var streamingResponse = GetStreamingResponseSafely(prompt, connectionId);

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

    private async IAsyncEnumerable<string> GetStreamingResponseSafely(string prompt, string connectionId)
    {
        IAsyncEnumerable<StreamingKernelContent>? kernelStream = null;

        // Try to start streaming
        kernelStream = TryStartStreaming(prompt, connectionId);

        if (kernelStream == null)
        {
            yield return "ERROR:Sorry, I encountered an error while processing your message.";
            yield break;
        }

        // Process the stream
        await foreach (var chunk in kernelStream)
        {
            var content = chunk.ToString();
            if (!string.IsNullOrEmpty(content))
            {
                yield return content;
            }
        }
    }

    private IAsyncEnumerable<StreamingKernelContent>? TryStartStreaming(string prompt, string connectionId)
    {
        try
        {
            return kernel.InvokePromptStreamingAsync(prompt);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting streaming for connection {ConnectionId}", connectionId);
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

    private string BuildPromptFromHistory(ChatHistory chatHistory)
    {
        var prompt = new StringBuilder();

        foreach (var message in chatHistory)
        {
            var role = message.Role.ToString().ToLowerInvariant();
            prompt.AppendLine($"{role}: {message.Content}");
        }

        prompt.AppendLine("assistant:");
        return prompt.ToString();
    }

    public void ClearHistory(string connectionId)
    {
        _conversations.TryRemove(connectionId, out _);
        logger.LogInformation("Cleared chat history for connection {ConnectionId}", connectionId);
    }

    public int GetHistoryCount(string connectionId)
    {
        var history = GetOrCreateChatHistory(connectionId);
        return history.Count;
    }
}
