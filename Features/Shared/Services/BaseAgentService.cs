using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticChatDemo.Features.Shared.Models;

namespace SemanticChatDemo.Features.Shared.Services;

/// <summary>
/// Base service for managing AI agents with common functionality
/// </summary>
public abstract class BaseAgentService(Kernel kernel, ILogger logger)
{
    protected readonly Dictionary<string, ChatCompletionAgent> agents = new();
    protected readonly Dictionary<string, AgentConfiguration> agentConfigurations = new();

    /// <summary>
    /// Initialize agents from configurations
    /// </summary>
    public virtual void InitializeAgents(List<AgentConfiguration> configurations)
    {
        try
        {
            // Clear existing agents
            agents.Clear();
            agentConfigurations.Clear();

            foreach (var config in configurations)
            {
                var agent = CreateAgent(config);
                agents[config.Name] = agent;

                // Create a new configuration with IsActive set to true
                var activeConfig = new AgentConfiguration
                {
                    Name = config.Name,
                    Description = config.Description,
                    Instructions = config.Instructions,
                    PersonalityAnchoring = config.PersonalityAnchoring,
                    AvatarEmoji = config.AvatarEmoji,
                    ColorScheme = config.ColorScheme,
                    IsActive = true
                };
                agentConfigurations[config.Name] = activeConfig;

                logger.LogInformation("Initialized agent: {AgentName}", config.Name);
            }
            logger.LogInformation("Successfully initialized {AgentCount} agents", configurations.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initializing agents");
            throw;
        }
    }

    /// <summary>
    /// Create a ChatCompletionAgent from configuration
    /// </summary>
    protected virtual ChatCompletionAgent CreateAgent(AgentConfiguration config)
    {
        var enhancedInstructions = EnhanceAgentInstructions(config);

        return new ChatCompletionAgent()
        {
            Name = config.Name,
            Description = config.Description,
            Instructions = enhancedInstructions,
            Kernel = kernel
        };
    }

    /// <summary>
    /// Enhance agent instructions with personality reinforcement
    /// Virtual method to allow customization in derived classes
    /// </summary>
    protected virtual string EnhanceAgentInstructions(AgentConfiguration config)
    {
        var instructions = config.Instructions;

        if (!string.IsNullOrEmpty(config.PersonalityAnchoring))
        {
            instructions += $"\n\n## Personality Guidelines:\n{config.PersonalityAnchoring}";
            instructions += "\n\nRemember to maintain your unique perspective and voice throughout the conversation.";
        }

        return instructions;
    }

    /// <summary>
    /// Get streaming response from a specific agent
    /// </summary>
    public async IAsyncEnumerable<string> GetAgentResponseAsync(string agentName, ChatHistory history)
    {
        if (!agents.TryGetValue(agentName, out var agent))
        {
            logger.LogError("Agent not found: {AgentName}", agentName);
            yield return $"Error: Agent '{agentName}' not found.";
            yield break;
        }

        logger.LogInformation("Getting response from agent: {AgentName}", agentName);

        var streamingResponse = TryStartAgentStreaming(agent, history, agentName);

        if (streamingResponse == null)
        {
            yield return $"Error: Failed to get response from {agentName}.";
            yield break;
        }

        await foreach (var chunk in streamingResponse)
        {
            var content = chunk.Content;
            if (!string.IsNullOrEmpty(content))
            {
                yield return content;
            }
        }
    }

    /// <summary>
    /// Start streaming from an agent with error handling
    /// </summary>
    protected virtual IAsyncEnumerable<StreamingChatMessageContent>? TryStartAgentStreaming(
        ChatCompletionAgent agent,
        ChatHistory history,
        string agentName)
    {
        try
        {
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            var agentHistory = CreateAgentHistory(agent, history);

            return chatCompletionService.GetStreamingChatMessageContentsAsync(
                agentHistory,
                kernel: kernel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting response from agent: {AgentName}", agentName);
            return null;
        }
    }

    /// <summary>
    /// Create chat history with agent context
    /// Virtual method to allow customization in derived classes
    /// </summary>
    protected virtual ChatHistory CreateAgentHistory(ChatCompletionAgent agent, ChatHistory history)
    {
        var agentHistory = new ChatHistory(agent.Instructions ?? "You are a helpful assistant.");

        // Add recent conversation history for context
        var recentMessages = history.TakeLast(6);
        foreach (var message in recentMessages)
        {
            agentHistory.Add(message);
        }

        return agentHistory;
    }

    /// <summary>
    /// Get list of available agents
    /// </summary>
    public virtual List<AgentConfiguration> GetAvailableAgents()
    {
        return agentConfigurations.Values.ToList();
    }

    /// <summary>
    /// Get specific agent by name
    /// </summary>
    public virtual ChatCompletionAgent? GetAgent(string name)
    {
        agents.TryGetValue(name, out var agent);
        return agent;
    }

    /// <summary>
    /// Get agent configuration by name
    /// </summary>
    public virtual AgentConfiguration? GetAgentConfiguration(string name)
    {
        agentConfigurations.TryGetValue(name, out var config);
        return config;
    }

    /// <summary>
    /// Check if an agent exists
    /// </summary>
    public virtual bool HasAgent(string name)
    {
        return agents.ContainsKey(name);
    }
}
