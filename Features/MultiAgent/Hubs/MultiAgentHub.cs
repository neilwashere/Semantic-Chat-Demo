using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticChatDemo.Features.MultiAgent.Models;
using SemanticChatDemo.Features.MultiAgent.Services;

namespace SemanticChatDemo.Features.MultiAgent.Hubs;

/// <summary>
/// SignalR hub for managing multi-agent conversations and real-time communication
/// </summary>
public class MultiAgentHub(AgentService agentService, ILogger<MultiAgentHub> logger) : Hub
{
    /// <summary>
    /// Load agents for a specific team without starting a conversation
    /// </summary>
    public async Task LoadTeamAgents(string agentTeam = "test")
    {
        try
        {
            logger.LogInformation("Loading agents for team: {Team}", agentTeam);

            // Get agent configurations based on team selection
            var agentConfigurations = agentTeam.ToLowerInvariant() switch
            {
                "copywriter" => AgentTemplates.GetCopywriterReviewerTeam(),
                "research" => AgentTemplates.GetResearchTeam(),
                "debate" => AgentTemplates.GetDebateTeam(),
                "technical" => AgentTemplates.GetTechnicalReviewTeam(),
                _ => AgentTemplates.GetTestAgentTeam()
            };

            // Initialize agents (but don't start conversation)
            agentService.InitializeAgents(agentConfigurations);

            // Send updated agent configurations to the client
            await Clients.Caller.SendAsync("AgentStatusUpdate", agentConfigurations);

            logger.LogInformation("Successfully loaded {AgentCount} agents for team: {Team}",
                agentConfigurations.Count, agentTeam);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading team agents for team: {Team}", agentTeam);

            // Send empty list on error
            await Clients.Caller.SendAsync("AgentStatusUpdate", new List<AgentConfiguration>());
        }
    }

    /// <summary>
    /// Start a multi-agent conversation with the specified task
    /// </summary>
    public async Task StartAgentConversation(string userTask, string agentTeam = "test")
    {
        try
        {
            logger.LogInformation("Starting agent conversation for task: {Task} with team: {Team}",
                userTask, agentTeam);

            // Initialize agents based on team selection
            var agentConfigurations = agentTeam.ToLowerInvariant() switch
            {
                "copywriter" => AgentTemplates.GetCopywriterReviewerTeam(),
                "research" => AgentTemplates.GetResearchTeam(),
                "debate" => AgentTemplates.GetDebateTeam(),
                "technical" => AgentTemplates.GetTechnicalReviewTeam(),
                _ => AgentTemplates.GetTestAgentTeam()
            };

            agentService.InitializeAgents(agentConfigurations);

            // Send updated agent configurations to the client
            await Clients.Caller.SendAsync("AgentStatusUpdate", agentConfigurations);

            // Send user task message
            var userMessage = new AgentMessage
            {
                Content = userTask,
                AgentName = "User",
                MessageType = "user",
                IsComplete = true
            };

            await Clients.Caller.SendAsync("ReceiveAgentMessage", userMessage);

            // Start multi-agent conversation
            await StartMultiAgentDiscussion(agentConfigurations, userTask);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting agent conversation");

            var errorMessage = new AgentMessage
            {
                Content = "Sorry, I encountered an error starting the agent conversation.",
                AgentName = "System",
                MessageType = "error",
                IsComplete = true
            };

            await Clients.Caller.SendAsync("ReceiveAgentMessage", errorMessage);
        }
    }

    /// <summary>
    /// Handle multi-agent discussion with turn-taking
    /// </summary>
    private async Task StartMultiAgentDiscussion(List<AgentConfiguration> agents, string initialTask)
    {
        var conversationHistory = new ChatHistory();
        var maxTurns = 4; // Limit conversation length for now
        var currentTurn = 0;

        // Add initial task to conversation history
        conversationHistory.AddUserMessage(initialTask);

        while (currentTurn < maxTurns && agents.Count >= 2)
        {
            // Alternate between agents
            var currentAgent = agents[currentTurn % agents.Count];

            logger.LogInformation("Turn {Turn}: Agent {AgentName} responding", currentTurn + 1, currentAgent.Name);

            // Get agent response
            await ProcessAgentResponse(currentAgent.Name, GetContextForAgent(conversationHistory, currentAgent), conversationHistory);

            currentTurn++;

            // Add a small delay between agents for better UX
            await Task.Delay(1000);
        }

        // Send completion message
        var completionMessage = new AgentMessage
        {
            Content = "🎯 Agent discussion completed. The agents have finished their deliberation.",
            AgentName = "System",
            MessageType = "system",
            IsComplete = true
        };

        await Clients.Caller.SendAsync("ReceiveAgentMessage", completionMessage);
    }

    /// <summary>
    /// Get appropriate context for the current agent based on conversation history
    /// </summary>
    private string GetContextForAgent(ChatHistory history, AgentConfiguration currentAgent)
    {
        if (history.Count <= 1)
        {
            // First turn - use original task
            return history.FirstOrDefault()?.Content ?? "";
        }

        // Subsequent turns - provide context about the previous agent's response
        var lastMessage = history.LastOrDefault();
        if (lastMessage != null)
        {
            return $"The previous agent said: \"{lastMessage.Content}\"\n\nPlease provide your perspective on this response.";
        }

        return "Please continue the discussion.";
    }

    /// <summary>
    /// Process response from a specific agent
    /// </summary>
    private async Task ProcessAgentResponse(string agentName, string input, ChatHistory conversationHistory)
    {
        try
        {
            // Add the input to conversation history
            conversationHistory.AddUserMessage(input);

            // Create agent message placeholder for streaming
            var agentMessageId = Guid.NewGuid().ToString();
            var agentMessage = new AgentMessage
            {
                Id = agentMessageId,
                Content = "",
                AgentName = agentName,
                MessageType = "agent",
                IsStreaming = true,
                IsComplete = false
            };

            // Send initial streaming message
            await Clients.Caller.SendAsync("ReceiveAgentMessage", agentMessage);

            // Stream the agent response
            var fullContent = "";
            await foreach (var chunk in agentService.GetAgentResponseAsync(agentName, conversationHistory))
            {
                fullContent += chunk;

                // Send streaming chunk
                var streamingUpdate = new AgentStreamingMessage
                {
                    MessageId = agentMessageId,
                    Content = chunk,
                    IsComplete = false
                };

                await Clients.Caller.SendAsync("ReceiveAgentStreamingChunk", streamingUpdate);
            }

            // Send completion signal
            var completionUpdate = new AgentStreamingMessage
            {
                MessageId = agentMessageId,
                Content = "",
                IsComplete = true
            };

            await Clients.Caller.SendAsync("ReceiveAgentStreamingChunk", completionUpdate);

            // Add agent response to conversation history
            conversationHistory.AddAssistantMessage(fullContent);

            // For now, just complete after first agent response
            // In Phase 2.2, this is where we'll add orchestration logic
            logger.LogInformation("Agent {AgentName} completed response", agentName);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing agent response for {AgentName}", agentName);

            var errorMessage = new AgentMessage
            {
                Content = $"Error getting response from {agentName}",
                AgentName = "System",
                MessageType = "error",
                IsComplete = true
            };

            await Clients.Caller.SendAsync("ReceiveAgentMessage", errorMessage);
        }
    }

    /// <summary>
    /// Get status of currently initialized agents
    /// </summary>
    public async Task GetAgentStatus()
    {
        try
        {
            var agents = agentService.GetAvailableAgents();
            await Clients.Caller.SendAsync("AgentStatusUpdate", agents);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting agent status");
            // Send empty list on error
            await Clients.Caller.SendAsync("AgentStatusUpdate", new List<AgentConfiguration>());
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("Multi-agent connection {ConnectionId} disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
