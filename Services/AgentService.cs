using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticChatDemo.Models;

namespace SemanticChatDemo.Services;

/// <summary>
/// Service for managing multiple AI agents and their conversations
/// </summary>
public class AgentService(Kernel kernel, ILogger<AgentService> logger)
{
    private readonly Dictionary<string, ChatCompletionAgent> agents = new();
    private readonly Dictionary<string, AgentConfiguration> agentConfigurations = new();

    /// <summary>
    /// Initialize agents from configurations
    /// </summary>
    public void InitializeAgents(List<AgentConfiguration> configurations)
    {
        try
        {
            // Clear existing agents
            agents.Clear();
            agentConfigurations.Clear();

            foreach (var config in configurations)
            {
                // Create ChatCompletionAgent for each configuration
                var agent = new ChatCompletionAgent()
                {
                    Name = config.Name,
                    Description = config.Description,
                    Instructions = config.Instructions,
                    Kernel = kernel
                };

                agents[config.Name] = agent;
                agentConfigurations[config.Name] = config;
                
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

        // Get streaming enumerable first, handle errors separately
        var streamingResponse = TryStartAgentStreaming(agent, history, agentName);

        if (streamingResponse == null)
        {
            yield return $"Error: Failed to get response from {agentName}.";
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

    private IAsyncEnumerable<StreamingChatMessageContent>? TryStartAgentStreaming(ChatCompletionAgent agent, ChatHistory history, string agentName)
    {
        try
        {
            // Get the chat completion service from the kernel  
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            // Create a new ChatHistory with agent's instructions as system message
            var agentHistory = new ChatHistory(agent.Instructions ?? "You are a helpful assistant.");
            
            // Add recent conversation history (last few messages to provide context)
            var recentMessages = history.TakeLast(6); // Keep last 6 messages for context
            foreach (var message in recentMessages)
            {
                agentHistory.Add(message);
            }

            // Get streaming response
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
    /// Get list of available agents
    /// </summary>
    public List<AgentConfiguration> GetAvailableAgents()
    {
        return agentConfigurations.Values.ToList();
    }

    /// <summary>
    /// Get specific agent by name
    /// </summary>
    public ChatCompletionAgent? GetAgent(string name)
    {
        agents.TryGetValue(name, out var agent);
        return agent;
    }

    /// <summary>
    /// Get agent configuration by name
    /// </summary>
    public AgentConfiguration? GetAgentConfiguration(string name)
    {
        agentConfigurations.TryGetValue(name, out var config);
        return config;
    }

    /// <summary>
    /// Check if an agent exists
    /// </summary>
    public bool HasAgent(string name)
    {
        return agents.ContainsKey(name);
    }
}
