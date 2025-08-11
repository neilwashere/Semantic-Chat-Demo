using System.Text.Json;
using SemanticChatDemo.Models;

namespace SemanticChatDemo.Services;

/// <summary>
/// Service for persisting and loading conversation data to/from JSON files
/// </summary>
public class ConversationPersistenceService(ILogger<ConversationPersistenceService> logger)
{
    private readonly string conversationsDirectory = Path.Combine("Data", "Conversations");
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Load conversation data for a specific user
    /// </summary>
    public async Task<ConversationData?> LoadConversationAsync(string userId)
    {
        try
        {
            var filePath = GetConversationFilePath(userId);
            
            if (!File.Exists(filePath))
            {
                logger.LogInformation("No existing conversation found for user {UserId}", userId);
                return null;
            }

            var jsonContent = await File.ReadAllTextAsync(filePath);
            var conversationData = JsonSerializer.Deserialize<ConversationData>(jsonContent, jsonOptions);
            
            logger.LogInformation("Loaded conversation for user {UserId} with {MessageCount} messages", 
                userId, conversationData?.Messages.Count ?? 0);
            
            return conversationData;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading conversation for user {UserId}", userId);
            return null;
        }
    }

    /// <summary>
    /// Save conversation data for a specific user
    /// </summary>
    public async Task SaveConversationAsync(ConversationData conversationData)
    {
        try
        {
            // Ensure directory exists
            Directory.CreateDirectory(conversationsDirectory);

            var filePath = GetConversationFilePath(conversationData.UserId);
            conversationData.LastUpdated = DateTime.UtcNow;

            var jsonContent = JsonSerializer.Serialize(conversationData, jsonOptions);
            await File.WriteAllTextAsync(filePath, jsonContent);

            logger.LogInformation("Saved conversation for user {UserId} with {MessageCount} messages",
                conversationData.UserId, conversationData.Messages.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving conversation for user {UserId}", conversationData.UserId);
            throw;
        }
    }

    /// <summary>
    /// Delete conversation data for a specific user
    /// </summary>
    public async Task DeleteConversationAsync(string userId)
    {
        try
        {
            var filePath = GetConversationFilePath(userId);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                logger.LogInformation("Deleted conversation for user {UserId}", userId);
            }
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting conversation for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get all user IDs that have saved conversations
    /// </summary>
    public async Task<List<string>> GetAllUserIdsAsync()
    {
        try
        {
            if (!Directory.Exists(conversationsDirectory))
            {
                return new List<string>();
            }

            var files = Directory.GetFiles(conversationsDirectory, "*.json");
            var userIds = files
                .Select(Path.GetFileNameWithoutExtension)
                .Where(fileName => !string.IsNullOrEmpty(fileName))
                .ToList();

            await Task.CompletedTask;
            return userIds;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all user IDs");
            return new List<string>();
        }
    }

    private string GetConversationFilePath(string userId)
    {
        // Sanitize userId for file system
        var sanitizedUserId = string.Join("_", userId.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(conversationsDirectory, $"{sanitizedUserId}.json");
    }
}
