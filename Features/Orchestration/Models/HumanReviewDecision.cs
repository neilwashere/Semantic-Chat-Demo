namespace SemanticChatDemo.Features.Orchestration.Models;

/// <summary>
/// Possible decisions a human reviewer can make during workflow evaluation
/// </summary>
public enum ReviewDecision
{
    /// <summary>
    /// Approve the current output and complete the workflow
    /// </summary>
    Approve,

    /// <summary>
    /// Request revisions from the agents with specific feedback
    /// </summary>
    Revise,

    /// <summary>
    /// Continue the collaboration for more iterations
    /// </summary>
    Continue,

    /// <summary>
    /// Cancel the workflow entirely
    /// </summary>
    Cancel
}

/// <summary>
/// Represents a human reviewer's decision during the HITL workflow
/// </summary>
public class HumanReviewDecision
{
    /// <summary>
    /// The decision made by the human reviewer
    /// </summary>
    public ReviewDecision Decision { get; set; }

    /// <summary>
    /// Optional feedback or instructions accompanying the decision
    /// </summary>
    public string? Feedback { get; set; }

    /// <summary>
    /// Timestamp when the decision was made
    /// </summary>
    public DateTime DecisionTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The workflow iteration this decision applies to
    /// </summary>
    public int IterationNumber { get; set; }

    /// <summary>
    /// Request ID that this decision responds to
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Create an approval decision
    /// </summary>
    public static HumanReviewDecision Approve(string? feedback = null, int iteration = 1)
    {
        return new HumanReviewDecision
        {
            Decision = ReviewDecision.Approve,
            Feedback = feedback,
            IterationNumber = iteration
        };
    }

    /// <summary>
    /// Create a revision request with feedback
    /// </summary>
    public static HumanReviewDecision Revise(string feedback, int iteration = 1)
    {
        return new HumanReviewDecision
        {
            Decision = ReviewDecision.Revise,
            Feedback = feedback,
            IterationNumber = iteration
        };
    }

    /// <summary>
    /// Create a continue decision
    /// </summary>
    public static HumanReviewDecision Continue(string? feedback = null, int iteration = 1)
    {
        return new HumanReviewDecision
        {
            Decision = ReviewDecision.Continue,
            Feedback = feedback,
            IterationNumber = iteration
        };
    }

    /// <summary>
    /// Create a cancel decision
    /// </summary>
    public static HumanReviewDecision Cancel(string? reason = null)
    {
        return new HumanReviewDecision
        {
            Decision = ReviewDecision.Cancel,
            Feedback = reason
        };
    }
}
