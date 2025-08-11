namespace SemanticChatDemo.Features.Orchestration.Models;

/// <summary>
/// Represents the current stage in a Human-in-the-Loop workflow
/// </summary>
public enum WorkflowStage
{
    /// <summary>
    /// End user is providing the initial task or request
    /// </summary>
    UserRequest,

    /// <summary>
    /// Agent couple is collaborating on the task
    /// </summary>
    AgentCollaboration,

    /// <summary>
    /// Human reviewer is evaluating the agent output
    /// </summary>
    HumanReview,

    /// <summary>
    /// Decision point - continue, complete, or revise
    /// </summary>
    Decision,

    /// <summary>
    /// Workflow has been completed successfully
    /// </summary>
    Completed
}

/// <summary>
/// Represents the different roles in the HITL workflow
/// </summary>
public enum ParticipantRole
{
    /// <summary>
    /// The person who initiates the task and provides requirements
    /// </summary>
    EndUser,

    /// <summary>
    /// First agent in the collaboration couple
    /// </summary>
    Agent1,

    /// <summary>
    /// Second agent in the collaboration couple
    /// </summary>
    Agent2,

    /// <summary>
    /// The human reviewer who evaluates agent output (same person as EndUser, different context)
    /// </summary>
    HumanReviewer
}

/// <summary>
/// Manages the state of a Human-in-the-Loop orchestration workflow
/// </summary>
public class WorkflowState
{
    /// <summary>
    /// Unique identifier for this workflow instance
    /// </summary>
    public string WorkflowId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Current stage in the workflow
    /// </summary>
    public WorkflowStage CurrentStage { get; set; } = WorkflowStage.UserRequest;

    /// <summary>
    /// The role currently active in the workflow
    /// </summary>
    public ParticipantRole ActiveRole { get; set; } = ParticipantRole.EndUser;

    /// <summary>
    /// Whether the workflow is currently waiting for human input
    /// </summary>
    public bool AwaitingHumanInput { get; set; } = false;

    /// <summary>
    /// The current task or request being worked on
    /// </summary>
    public string? CurrentTask { get; set; }

    /// <summary>
    /// Names of the agents involved in this workflow
    /// </summary>
    public List<string> AgentNames { get; set; } = new();

    /// <summary>
    /// Current iteration number (for workflows that involve multiple rounds)
    /// </summary>
    public int Iteration { get; set; } = 1;

    /// <summary>
    /// Timestamp when the workflow was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp of the last activity in the workflow
    /// </summary>
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The selected agent team for this workflow
    /// </summary>
    public string AgentTeam { get; set; } = string.Empty;

    /// <summary>
    /// Update the last activity timestamp
    /// </summary>
    public void UpdateActivity()
    {
        LastActivity = DateTime.UtcNow;
    }

    /// <summary>
    /// Transition to a new workflow stage
    /// </summary>
    public void TransitionTo(WorkflowStage newStage, ParticipantRole? newRole = null)
    {
        CurrentStage = newStage;
        if (newRole.HasValue)
        {
            ActiveRole = newRole.Value;
        }
        UpdateActivity();
    }
}
