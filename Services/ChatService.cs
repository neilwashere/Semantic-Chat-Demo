using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticChatDemo.Models;
using System.Collections.Concurrent;

namespace SemanticChatDemo.Services;

public class ChatService(Kernel kernel, ConversationPersistenceService persistenceService, ILogger<ChatService> logger)
{
    // Store chat history per user ID in memory for active sessions
    private readonly ConcurrentDictionary<string, ChatHistory> conversations = new();

    public async Task<ChatHistory> GetOrCreateChatHistoryAsync(string userId)
    {
        if (conversations.TryGetValue(userId, out var chatHistory))
        {
            return chatHistory;
        }

        // Try to load existing conversation from persistence
        var conversationData = await persistenceService.LoadConversationAsync(userId);

        if (conversationData != null)
        {
            // Restore chat history from persisted data
            var restoredHistory = new ChatHistory(conversationData.SystemPrompt);

            foreach (var message in conversationData.Messages)
            {
                switch (message.Role.ToLowerInvariant())
                {
                    case "user":
                        restoredHistory.AddUserMessage(message.Content);
                        break;
                    case "assistant":
                        restoredHistory.AddAssistantMessage(message.Content);
                        break;
                    case "system":
                        restoredHistory.AddSystemMessage(message.Content);
                        break;
                }
            }

            conversations.TryAdd(userId, restoredHistory);
            logger.LogInformation("Restored conversation for user {UserId} with {MessageCount} messages",
                userId, conversationData.Messages.Count);
            return restoredHistory;
        }

        // Create new conversation if none exists
        var systemPromptTemplate = await File.ReadAllTextAsync("Prompts/SystemPrompt.txt");

        // Use Semantic Kernel's proper template variable substitution
        var prompt = await kernel.CreateFunctionFromPrompt(systemPromptTemplate)
            .InvokeAsync<string>(kernel, new KernelArguments {
                { "current_date", DateTime.UtcNow.ToString("R") }
            });

        var newChatHistory = new ChatHistory(prompt ?? systemPromptTemplate);
        conversations.TryAdd(userId, newChatHistory);

        // Save the initial conversation with system prompt
        await SaveConversationAsync(userId, newChatHistory, prompt ?? systemPromptTemplate);

        logger.LogInformation("Created new conversation for user {UserId}", userId);
        return newChatHistory;
    }

    public async IAsyncEnumerable<string> StreamResponseAsync(string userId, string userMessage)
    {
        var chatHistory = await GetOrCreateChatHistoryAsync(userId);

        // Add user message to history
        chatHistory.AddUserMessage(userMessage);

        logger.LogInformation("Streaming response for user {UserId}, message: {Message}",
            userId, userMessage);

        // Get streaming enumerable first, handle errors separately
        var streamingResponse = GetStreamingResponseSafely(chatHistory, userId);

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
        AddToHistorySafely(chatHistory, fullResponse, userId);

        // Save conversation after each message exchange
        await SaveConversationAfterMessage(userId, chatHistory);
    }

    private async IAsyncEnumerable<string> GetStreamingResponseSafely(ChatHistory chatHistory, string userId)
    {
        IAsyncEnumerable<StreamingChatMessageContent>? streamingResponse = null;

        // Try to start streaming outside of yield context
        streamingResponse = TryStartStreamingChat(chatHistory, userId);

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

    private IAsyncEnumerable<StreamingChatMessageContent>? TryStartStreamingChat(ChatHistory chatHistory, string userId)
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
            logger.LogError(ex, "Error starting streaming chat completion for user {UserId}", userId);
            return null;
        }
    }

    private void AddToHistorySafely(ChatHistory chatHistory, string response, string userId)
    {
        if (!string.IsNullOrEmpty(response))
        {
            try
            {
                chatHistory.AddAssistantMessage(response);
                logger.LogInformation("Added response to history for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding response to history for user {UserId}", userId);
            }
        }
    }

    public async Task ClearHistory(string userId)
    {
        conversations.TryRemove(userId, out _);

        // Also clear persisted data
        try
        {
            await persistenceService.DeleteConversationAsync(userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error clearing persisted conversation for user {UserId}", userId);
        }

        logger.LogInformation("Cleared chat history for user {UserId}", userId);
    }

    public int GetHistoryCount(string userId)
    {
        if (conversations.TryGetValue(userId, out var history))
        {
            return history.Count;
        }
        return 0;
    }

    /// <summary>
    /// Get existing conversation messages for display in UI
    /// </summary>
    public async Task<List<ConversationMessage>> GetExistingMessagesAsync(string userId)
    {
        try
        {
            var conversationData = await persistenceService.LoadConversationAsync(userId);
            return conversationData?.Messages ?? new List<ConversationMessage>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading existing messages for user {UserId}", userId);
            return new List<ConversationMessage>();
        }
    }

    /// <summary>
    /// Save conversation to persistence storage
    /// </summary>
    private async Task SaveConversationAsync(string userId, ChatHistory chatHistory, string systemPrompt)
    {
        try
        {
            var conversationData = new ConversationData
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                SystemPrompt = systemPrompt,
                Messages = ConvertChatHistoryToMessages(chatHistory)
            };

            await persistenceService.SaveConversationAsync(conversationData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving conversation for user {UserId}", userId);
        }
    }

    /// <summary>
    /// Save conversation after a message exchange
    /// </summary>
    private async Task SaveConversationAfterMessage(string userId, ChatHistory chatHistory)
    {
        try
        {
            // Load existing conversation to preserve creation date and system prompt
            var existingData = await persistenceService.LoadConversationAsync(userId);

            if (existingData == null)
            {
                // First time saving - create new conversation data
                var conversationData = new ConversationData
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    SystemPrompt = chatHistory.FirstOrDefault()?.Content ?? "",
                    Messages = ConvertChatHistoryToMessages(chatHistory)
                };

                await persistenceService.SaveConversationAsync(conversationData);
                return;
            }

            // Get the last two messages from chat history (user message + assistant response)
            var recentMessages = chatHistory
                .Where(m => m.Role != Microsoft.SemanticKernel.ChatCompletion.AuthorRole.System)
                .Where(m => m.Role != Microsoft.SemanticKernel.ChatCompletion.AuthorRole.Tool)
                .Where(m => !string.IsNullOrWhiteSpace(m.Content))
                .TakeLast(2)
                .ToList();

            // Only add new messages that aren't already in the persisted data
            foreach (var message in recentMessages)
            {
                var messageContent = message.Content?.Trim();
                if (string.IsNullOrEmpty(messageContent))
                    continue;

                // Check if this message content already exists in persisted data
                var isDuplicate = existingData.Messages.Any(existing =>
                    existing.Content.Trim() == messageContent &&
                    existing.Role == message.Role.ToString().ToLowerInvariant());

                if (!isDuplicate)
                {
                    existingData.Messages.Add(new ConversationMessage
                    {
                        Id = Guid.NewGuid().ToString(),
                        Content = messageContent,
                        Role = message.Role.ToString().ToLowerInvariant(),
                        Timestamp = DateTime.UtcNow
                    });
                }
            }

            await persistenceService.SaveConversationAsync(existingData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving conversation after message for user {UserId}", userId);
        }
    }

    /// <summary>
    /// Convert ChatHistory to ConversationMessage list
    /// </summary>
    private static List<ConversationMessage> ConvertChatHistoryToMessages(ChatHistory chatHistory)
    {
        var messages = new List<ConversationMessage>();

        foreach (var message in chatHistory)
        {
            // Skip system messages as they're stored separately
            if (message.Role == Microsoft.SemanticKernel.ChatCompletion.AuthorRole.System)
                continue;

            // Skip tool messages - they are intermediate results, not final messages
            if (message.Role == Microsoft.SemanticKernel.ChatCompletion.AuthorRole.Tool)
                continue;

            // Skip empty messages (streaming placeholders)
            if (string.IsNullOrWhiteSpace(message.Content))
                continue;

            messages.Add(new ConversationMessage
            {
                Id = Guid.NewGuid().ToString(),
                Content = message.Content,
                Role = message.Role.ToString().ToLowerInvariant(),
                Timestamp = DateTime.UtcNow
            });
        }

        return messages;
    }
}
