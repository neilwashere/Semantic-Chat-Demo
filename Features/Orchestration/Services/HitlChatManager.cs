using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticChatDemo.Features.Orchestration.Hubs;
using SemanticChatDemo.Features.Orchestration.Models;
using System.Collections.Concurrent;

namespace SemanticChatDemo.Features.Orchestration.Services;

/// <summary>
/// Custom chat manager that integrates Human-in-the-Loop workflows with Semantic Kernel orchestration
/// </summary>
#pragma warning disable SKEXP0001, SKEXP0110 // Type is for evaluation purposes only
public sealed class HitlChatManager : GroupChatManager
{
    private readonly IHubContext<OrchestrationHub> hubContext;
    private readonly string connectionId;
    private readonly string writerAgentName;
    private readonly string reviewerAgentName;
    private readonly OrchestrationService orchestrationService;
    private readonly ILogger<HitlChatManager> logger;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<ChatMessageContent>> pendingInputs = new();

    /// <summary>
    /// Initialize the HITL chat manager with SignalR integration
    /// </summary>
    public HitlChatManager(
        IHubContext<OrchestrationHub> hubContext,
        string connectionId,
        string writerAgentName,
        string reviewerAgentName,
        OrchestrationService orchestrationService,
        ILogger<HitlChatManager> logger) : base()
    {
        this.hubContext = hubContext;
        this.connectionId = connectionId;
        this.writerAgentName = writerAgentName;
        this.reviewerAgentName = reviewerAgentName;
        this.orchestrationService = orchestrationService;
        this.logger = logger;

        // Configure the group chat manager
        MaximumInvocationCount = 10; // Allow up to 10 iterations

        // Set up human input callback
        InteractiveCallback = RequestHumanInputAsync;
    }

    /// <summary>
    /// Request human input through SignalR
    /// </summary>
    private async ValueTask<ChatMessageContent> RequestHumanInputAsync()
    {
        var requestId = Guid.NewGuid().ToString();
        var promptMessage = "Please review the agent responses and provide your decision.";

        try
        {
            var tcs = new TaskCompletionSource<ChatMessageContent>();
            pendingInputs[requestId] = tcs;

            // Send request for human input via SignalR
            await hubContext.Clients.Client(connectionId).SendAsync("RequestHumanInput", requestId, promptMessage);

            logger.LogInformation("Requested human input for request {RequestId}", requestId);

            // Wait for human response with timeout
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            var humanResponse = await tcs.Task.WaitAsync(timeoutCts.Token);

            logger.LogInformation("Received human input for request {RequestId}", requestId);
            return humanResponse;
        }
        catch (TimeoutException)
        {
            logger.LogWarning("Human input request {RequestId} timed out", requestId);
            return new ChatMessageContent(AuthorRole.User, "Timeout - proceeding with current output.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error requesting human input for request {RequestId}", requestId);
            return new ChatMessageContent(AuthorRole.User, "Error - proceeding with current output.");
        }
        finally
        {
            pendingInputs.TryRemove(requestId, out _);
        }
    }

    /// <summary>
    /// Handle human input response
    /// </summary>
    public bool TryCompleteHumanInput(string requestId, HumanReviewDecision decision)
    {
        if (pendingInputs.TryRemove(requestId, out var tcs))
        {
            var messageContent = ConvertDecisionToMessage(decision);
            tcs.SetResult(messageContent);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Required implementation for agent selection
    /// </summary>
    public override ValueTask<GroupChatManagerResult<string>> SelectNextAgent(ChatHistory history, GroupChatTeam team, CancellationToken cancellationToken = default)
    {
        // Simple round-robin selection for now - just return first agent name
        // In a real implementation, you'd have access to team members
        return ValueTask.FromResult(new GroupChatManagerResult<string>(writerAgentName)
        {
            Reason = "Selected writer agent for next turn."
        });
    }

    /// <summary>
    /// Determine if human input is needed
    /// </summary>
    public override ValueTask<GroupChatManagerResult<bool>> ShouldRequestUserInput(
        ChatHistory history,
        CancellationToken cancellationToken = default)
    {
        // Request human input after certain conditions
        var lastMessage = history.LastOrDefault();

        if (lastMessage?.AuthorName?.Contains("Reviewer", StringComparison.OrdinalIgnoreCase) == true ||
            lastMessage?.AuthorName?.Contains("Critic", StringComparison.OrdinalIgnoreCase) == true)
        {
            // After reviewer/critic, request human input
            return ValueTask.FromResult(new GroupChatManagerResult<bool>(true)
            {
                Reason = "Human review required after agent collaboration."
            });
        }

        return ValueTask.FromResult(new GroupChatManagerResult<bool>(false)
        {
            Reason = "No human input required at this time."
        });
    }

    /// <summary>
    /// Filter or modify agent results
    /// </summary>
    public override ValueTask<GroupChatManagerResult<string>> FilterResults(
        ChatHistory history,
        CancellationToken cancellationToken = default)
    {
        // For now, just return the last message content as-is
        var lastMessage = history.LastOrDefault();
        var content = lastMessage?.Content ?? "No content to filter";

        return ValueTask.FromResult(new GroupChatManagerResult<string>(content)
        {
            Reason = "No filtering applied."
        });
    }

    /// <summary>
    /// Convert human decision to chat message
    /// </summary>
    private static ChatMessageContent ConvertDecisionToMessage(HumanReviewDecision decision)
    {
        var content = decision.Decision switch
        {
            ReviewDecision.Approve => $"APPROVED: {decision.Feedback}",
            ReviewDecision.Revise => $"REVISION NEEDED: {decision.Feedback}",
            ReviewDecision.Continue => $"CONTINUE: {decision.Feedback}",
            _ => decision.Feedback ?? "No feedback provided"
        };

        return new ChatMessageContent(AuthorRole.User, content);
    }
}
#pragma warning restore SKEXP0001, SKEXP0110
