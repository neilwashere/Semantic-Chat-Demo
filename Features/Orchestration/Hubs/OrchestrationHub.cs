using Microsoft.AspNetCore.SignalR;
using SemanticChatDemo.Features.Orchestration.Models;
using SemanticChatDemo.Features.Orchestration.Services;
using SemanticChatDemo.Features.Shared.Models;

namespace SemanticChatDemo.Features.Orchestration.Hubs;

/// <summary>
/// SignalR hub for managing Human-in-the-Loop orchestration workflows
/// </summary>
public class OrchestrationHub(
    OrchestrationService orchestrationService,
    ILogger<OrchestrationHub> logger) : Hub
{
    /// <summary>
    /// Start a new Human-in-the-Loop workflow
    /// </summary>
    public async Task StartHitlWorkflow(string task, string agentTeam = "copywriter")
    {
        try
        {
            logger.LogInformation("Starting HITL workflow for task: {Task} with team: {Team} for connection: {ConnectionId}",
                task, agentTeam, Context.ConnectionId);

            // Start the orchestration workflow
            await orchestrationService.StartWorkflowAsync(task, agentTeam, Context.ConnectionId);

            logger.LogInformation("Successfully started HITL workflow for connection: {ConnectionId}", Context.ConnectionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting HITL workflow for connection: {ConnectionId}", Context.ConnectionId);

            var errorMessage = OrchestrationMessage.CreateSystemMessage(
                new WorkflowState { CurrentStage = WorkflowStage.UserRequest },
                $"Failed to start workflow: {ex.Message}",
                "error"
            );

            await Clients.Caller.SendAsync("ReceiveOrchestrationMessage", errorMessage);
        }
    }

    /// <summary>
    /// Submit human input/decision during the workflow
    /// </summary>
    public async Task SubmitHumanInput(string requestId, string decision, string? feedback = null)
    {
        try
        {
            logger.LogInformation("Received human input for request: {RequestId} with decision: {Decision} from connection: {ConnectionId}",
                requestId, decision, Context.ConnectionId);

            // Parse the decision
            if (!Enum.TryParse<ReviewDecision>(decision, true, out var reviewDecision))
            {
                logger.LogWarning("Invalid decision received: {Decision}", decision);
                return;
            }

            var humanDecision = new HumanReviewDecision
            {
                Decision = reviewDecision,
                Feedback = feedback,
                RequestId = requestId
            };

            // Submit the decision to the orchestration service
            await orchestrationService.SubmitHumanDecisionAsync(requestId, humanDecision, Context.ConnectionId);

            logger.LogInformation("Successfully processed human input for request: {RequestId}", requestId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing human input for request: {RequestId} from connection: {ConnectionId}",
                requestId, Context.ConnectionId);

            var errorMessage = OrchestrationMessage.CreateSystemMessage(
                new WorkflowState(),
                $"Failed to process human input: {ex.Message}",
                "error"
            );

            await Clients.Caller.SendAsync("ReceiveOrchestrationMessage", errorMessage);
        }
    }

    /// <summary>
    /// Get the current state of the workflow
    /// </summary>
    public async Task GetWorkflowState()
    {
        try
        {
            var workflowState = await orchestrationService.GetWorkflowStateAsync(Context.ConnectionId);

            if (workflowState != null)
            {
                await Clients.Caller.SendAsync("WorkflowStateUpdate", workflowState);
            }
            else
            {
                logger.LogInformation("No active workflow found for connection: {ConnectionId}", Context.ConnectionId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting workflow state for connection: {ConnectionId}", Context.ConnectionId);
        }
    }

    /// <summary>
    /// Reset/cancel the current workflow
    /// </summary>
    public async Task ResetWorkflow()
    {
        try
        {
            logger.LogInformation("Resetting workflow for connection: {ConnectionId}", Context.ConnectionId);

            await orchestrationService.ResetWorkflowAsync(Context.ConnectionId);

            var resetMessage = OrchestrationMessage.CreateSystemMessage(
                new WorkflowState(),
                "🔄 Workflow has been reset. Ready to start a new collaboration.",
                "reset"
            );

            await Clients.Caller.SendAsync("ReceiveOrchestrationMessage", resetMessage);

            logger.LogInformation("Successfully reset workflow for connection: {ConnectionId}", Context.ConnectionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resetting workflow for connection: {ConnectionId}", Context.ConnectionId);
        }
    }

    /// <summary>
    /// Get available agent teams for workflow selection
    /// </summary>
    public async Task GetAvailableTeams()
    {
        try
        {
            var teams = orchestrationService.GetAvailableTeams();
            await Clients.Caller.SendAsync("AvailableTeamsUpdate", teams);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting available teams for connection: {ConnectionId}", Context.ConnectionId);
        }
    }

    /// <summary>
    /// Handle client disconnection
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("HITL orchestration connection {ConnectionId} disconnected", Context.ConnectionId);

        // Clean up any active workflows for this connection
        try
        {
            await orchestrationService.CleanupConnectionAsync(Context.ConnectionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cleaning up workflow for disconnected connection: {ConnectionId}", Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Handle client connection
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        logger.LogInformation("HITL orchestration connection {ConnectionId} connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }
}
