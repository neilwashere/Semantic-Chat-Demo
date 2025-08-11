using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticChatDemo.Features.Orchestration.Hubs;
using SemanticChatDemo.Features.Orchestration.Models;
using SemanticChatDemo.Features.Shared.Models;

namespace SemanticChatDemo.Features.Orchestration.Services;

/// <summary>
/// Monitors Semantic Kernel orchestration execution and streams real-time updates via SignalR
/// </summary>
#pragma warning disable SKEXP0001, SKEXP0110 // Type is for evaluation purposes only
public class StreamingOrchestrationMonitor
{
    private readonly IHubContext<OrchestrationHub> hubContext;
    private readonly string connectionId;
    private readonly ILogger<StreamingOrchestrationMonitor> logger;
    private readonly WorkflowState workflowState;

    /// <summary>
    /// Initialize the streaming monitor for a specific connection and workflow
    /// </summary>
    public StreamingOrchestrationMonitor(
        IHubContext<OrchestrationHub> hubContext,
        string connectionId,
        WorkflowState workflowState,
        ILogger<StreamingOrchestrationMonitor> logger)
    {
        this.hubContext = hubContext;
        this.connectionId = connectionId;
        this.workflowState = workflowState;
        this.logger = logger;
    }

    /// <summary>
    /// Response callback for GroupChatOrchestration - captures agent responses and streams them
    /// This method is called by SK whenever an agent produces a response
    /// </summary>
    public async ValueTask ResponseCallback(ChatMessageContent response)
    {
        try
        {
            logger.LogInformation("Received agent response from: {AgentName} for connection: {ConnectionId}",
                response.AuthorName, connectionId);

            // Update workflow state based on which agent is responding
            UpdateWorkflowForAgentResponse(response);

            // Convert to orchestration message
            var orchestrationMessage = CreateOrchestrationMessage(response);

            // Stream the complete message to the client
            await hubContext.Clients.Client(connectionId)
                .SendAsync("ReceiveOrchestrationMessage", orchestrationMessage);

            // Update workflow state
            await hubContext.Clients.Client(connectionId)
                .SendAsync("WorkflowStateUpdate", workflowState);

            // TODO: In a future enhancement, we could implement token-by-token streaming
            // by capturing streaming responses from the underlying chat completion service
            // For now, we send complete responses as they come from SK orchestration

            logger.LogInformation("Successfully streamed agent response to connection: {ConnectionId}", connectionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error streaming agent response for connection: {ConnectionId}", connectionId);

            // Send error message to client
            var errorMessage = OrchestrationMessage.CreateSystemMessage(
                workflowState,
                $"Error processing agent response: {ex.Message}",
                "error"
            );

            try
            {
                await hubContext.Clients.Client(connectionId)
                    .SendAsync("ReceiveOrchestrationMessage", errorMessage);
            }
            catch (Exception sendEx)
            {
                logger.LogError(sendEx, "Failed to send error message to client: {ConnectionId}", connectionId);
            }
        }
    }

    /// <summary>
    /// Stream a custom message during orchestration (for system messages, progress updates, etc.)
    /// </summary>
    public async Task StreamSystemMessage(string content, string messageType = "progress")
    {
        try
        {
            var systemMessage = OrchestrationMessage.CreateSystemMessage(
                workflowState,
                content,
                messageType
            );

            await hubContext.Clients.Client(connectionId)
                .SendAsync("ReceiveOrchestrationMessage", systemMessage);

            logger.LogInformation("Streamed system message to connection: {ConnectionId}", connectionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error streaming system message for connection: {ConnectionId}", connectionId);
        }
    }

    /// <summary>
    /// Stream workflow stage transitions
    /// </summary>
    public async Task StreamStageTransition(WorkflowStage newStage, string description)
    {
        try
        {
            workflowState.TransitionTo(newStage);

            var transitionMessage = OrchestrationMessage.CreateSystemMessage(
                workflowState,
                $"🔄 {description}",
                "stage-transition"
            );

            await hubContext.Clients.Client(connectionId)
                .SendAsync("ReceiveOrchestrationMessage", transitionMessage);

            await hubContext.Clients.Client(connectionId)
                .SendAsync("WorkflowStateUpdate", workflowState);

            logger.LogInformation("Streamed stage transition to {Stage} for connection: {ConnectionId}",
                newStage, connectionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error streaming stage transition for connection: {ConnectionId}", connectionId);
        }
    }

    /// <summary>
    /// Stream agent collaboration start
    /// </summary>
    public async Task StreamCollaborationStart(List<string> agentNames)
    {
        try
        {
            workflowState.AgentNames = agentNames;
            workflowState.TransitionTo(WorkflowStage.AgentCollaboration, ParticipantRole.Agent1);

            var agentList = string.Join(" and ", agentNames);
            var startMessage = OrchestrationMessage.CreateSystemMessage(
                workflowState,
                $"🤖 Starting agent collaboration between {agentList}...",
                "collaboration-start"
            );

            await hubContext.Clients.Client(connectionId)
                .SendAsync("ReceiveOrchestrationMessage", startMessage);

            await hubContext.Clients.Client(connectionId)
                .SendAsync("WorkflowStateUpdate", workflowState);

            logger.LogInformation("Streamed collaboration start for agents: {Agents} to connection: {ConnectionId}",
                agentList, connectionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error streaming collaboration start for connection: {ConnectionId}", connectionId);
        }
    }

    /// <summary>
    /// Stream workflow completion
    /// </summary>
    public async Task StreamWorkflowCompletion(string finalResult)
    {
        try
        {
            workflowState.TransitionTo(WorkflowStage.Completed);

            var completionMessage = OrchestrationMessage.CreateSystemMessage(
                workflowState,
                $"✅ Workflow completed successfully!\n\nFinal Result: {finalResult}",
                "completion"
            );

            await hubContext.Clients.Client(connectionId)
                .SendAsync("ReceiveOrchestrationMessage", completionMessage);

            await hubContext.Clients.Client(connectionId)
                .SendAsync("WorkflowStateUpdate", workflowState);

            logger.LogInformation("Streamed workflow completion for connection: {ConnectionId}", connectionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error streaming workflow completion for connection: {ConnectionId}", connectionId);
        }
    }

    /// <summary>
    /// Update workflow state based on agent response
    /// </summary>
    private void UpdateWorkflowForAgentResponse(ChatMessageContent response)
    {
        // Determine current role based on agent name
        var agentName = response.AuthorName ?? "Unknown";

        if (workflowState.AgentNames.Count > 0)
        {
            var isFirstAgent = agentName.Equals(workflowState.AgentNames[0], StringComparison.OrdinalIgnoreCase);
            var currentRole = isFirstAgent ? ParticipantRole.Agent1 : ParticipantRole.Agent2;

            if (workflowState.ActiveRole != currentRole)
            {
                workflowState.ActiveRole = currentRole;
            }
        }

        // Ensure we're in collaboration stage during agent responses
        if (workflowState.CurrentStage != WorkflowStage.AgentCollaboration)
        {
            workflowState.TransitionTo(WorkflowStage.AgentCollaboration);
        }

        workflowState.UpdateActivity();
    }

    /// <summary>
    /// Create orchestration message from agent response
    /// </summary>
    private OrchestrationMessage CreateOrchestrationMessage(ChatMessageContent response)
    {
        // Determine sender role
        var senderRole = DetermineSenderRole(response.AuthorName);

        return new OrchestrationMessage
        {
            Content = response.Content ?? "",
            AgentName = response.AuthorName ?? "Unknown Agent",
            MessageType = "agent",
            WorkflowId = workflowState.WorkflowId,
            WorkflowStage = workflowState.CurrentStage,
            SenderRole = senderRole,
            Iteration = workflowState.Iteration,
            IsComplete = true,
            IsStreaming = false
        };
    }

    /// <summary>
    /// Determine participant role based on agent name
    /// </summary>
    private ParticipantRole DetermineSenderRole(string? agentName)
    {
        if (string.IsNullOrEmpty(agentName) || workflowState.AgentNames.Count == 0)
        {
            return ParticipantRole.Agent1;
        }

        // Check if it's the first agent in the list
        if (agentName.Equals(workflowState.AgentNames[0], StringComparison.OrdinalIgnoreCase))
        {
            return ParticipantRole.Agent1;
        }

        // Check if it's the second agent in the list
        if (workflowState.AgentNames.Count > 1 &&
            agentName.Equals(workflowState.AgentNames[1], StringComparison.OrdinalIgnoreCase))
        {
            return ParticipantRole.Agent2;
        }

        // Default fallback
        return ParticipantRole.Agent1;
    }
}
#pragma warning restore SKEXP0001, SKEXP0110
