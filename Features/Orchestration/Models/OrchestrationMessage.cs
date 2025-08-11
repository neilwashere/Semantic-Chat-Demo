using SemanticChatDemo.Features.Shared.Models;

namespace SemanticChatDemo.Features.Orchestration.Models;

/// <summary>
/// Extended message type for orchestration workflows that includes workflow context
/// </summary>
public class OrchestrationMessage : AgentMessage
{
    /// <summary>
    /// The workflow this message belongs to
    /// </summary>
    public string WorkflowId { get; set; } = string.Empty;

    /// <summary>
    /// The workflow stage when this message was created
    /// </summary>
    public WorkflowStage WorkflowStage { get; set; }

    /// <summary>
    /// The participant role of the message sender
    /// </summary>
    public ParticipantRole SenderRole { get; set; }

    /// <summary>
    /// Whether this message requires human input/response
    /// </summary>
    public bool RequiresHumanInput { get; set; } = false;

    /// <summary>
    /// Associated request ID for human input scenarios
    /// </summary>
    public string? HumanInputRequestId { get; set; }

    /// <summary>
    /// The iteration number when this message was created
    /// </summary>
    public int Iteration { get; set; } = 1;

    /// <summary>
    /// Additional metadata for orchestration context
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Create an orchestration message from a regular agent message
    /// </summary>
    public static OrchestrationMessage FromAgentMessage(
        AgentMessage agentMessage,
        WorkflowState workflowState,
        ParticipantRole senderRole)
    {
        return new OrchestrationMessage
        {
            Id = agentMessage.Id,
            Content = agentMessage.Content,
            AgentName = agentMessage.AgentName,
            Timestamp = agentMessage.Timestamp,
            IsComplete = agentMessage.IsComplete,
            IsStreaming = agentMessage.IsStreaming,
            MessageType = agentMessage.MessageType,
            WorkflowId = workflowState.WorkflowId,
            WorkflowStage = workflowState.CurrentStage,
            SenderRole = senderRole,
            Iteration = workflowState.Iteration
        };
    }

    /// <summary>
    /// Create a human input request message
    /// </summary>
    public static OrchestrationMessage CreateHumanInputRequest(
        WorkflowState workflowState,
        string requestId,
        string promptMessage)
    {
        return new OrchestrationMessage
        {
            Content = promptMessage,
            AgentName = "System",
            MessageType = "human-input-request",
            WorkflowId = workflowState.WorkflowId,
            WorkflowStage = workflowState.CurrentStage,
            SenderRole = ParticipantRole.EndUser, // Transitioning to human reviewer
            RequiresHumanInput = true,
            HumanInputRequestId = requestId,
            Iteration = workflowState.Iteration,
            IsComplete = true
        };
    }

    /// <summary>
    /// Create a system workflow status message
    /// </summary>
    public static OrchestrationMessage CreateSystemMessage(
        WorkflowState workflowState,
        string content,
        string messageSubType = "status")
    {
        return new OrchestrationMessage
        {
            Content = content,
            AgentName = "System",
            MessageType = $"system-{messageSubType}",
            WorkflowId = workflowState.WorkflowId,
            WorkflowStage = workflowState.CurrentStage,
            SenderRole = ParticipantRole.EndUser,
            Iteration = workflowState.Iteration,
            IsComplete = true
        };
    }
}
