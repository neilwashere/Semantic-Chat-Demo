# HITL Orchestration Architecture - Step 1 Analysis

## GroupChatOrchestration Integration Points

Based on the SK example and our requirements, here's the integration approach:

### 1. Core SK Components We'll Use
- `GroupChatOrchestration` - Main orchestration engine
- `HumanInTheLoopChatManager` - Base class for custom manager (extends `RoundRobinGroupChatManager`)
- `OrchestrationResult<T>` - Result wrapper for orchestration outcomes
- `InProcessRuntime` - Agent runtime for orchestration

### 2. Streaming Integration Strategy

**Challenge**: SK's GroupChatOrchestration operates with complete responses, but we need real-time streaming.

**Solution**: Custom ResponseCallback + SignalR Bridge
```csharp
// In GroupChatOrchestration configuration
GroupChatOrchestration orchestration = new(
    customHitlManager,
    writerAgent,
    reviewerAgent)
{
    LoggerFactory = loggerFactory,
    ResponseCallback = streamingMonitor.ResponseCallback, // ← Key integration point
};
```

### 3. Async Coordination Architecture

**Problem**: SK's `InteractiveCallback` is synchronous, SignalR is async
**Solution**: TaskCompletionSource bridge pattern

```csharp
public sealed class HitlChatManager : HumanInTheLoopChatManager
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<ChatMessageContent>>
        pendingInputs = new();

    public HitlChatManager(IHubContext<OrchestrationHub> hubContext, string connectionId)
    {
        InteractiveCallback = async () =>
        {
            var requestId = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<ChatMessageContent>();
            pendingInputs[requestId] = tcs;

            await hubContext.Clients.Client(connectionId)
                .SendAsync("RequestHumanInput", requestId, GetCurrentWorkflowState());

            return await tcs.Task; // Wait for SignalR response
        };
    }

    public void ResolveHumanInput(string requestId, string userInput)
    {
        if (pendingInputs.TryRemove(requestId, out var tcs))
        {
            tcs.SetResult(new ChatMessageContent(AuthorRole.User, userInput));
        }
    }
}
```

### 4. Workflow State Management

```csharp
public enum WorkflowStage
{
    UserRequest,        // End user providing initial task
    AgentCollaboration, // Agent couple working together
    HumanReview,        // Human reviewer (same person, different role)
    Decision,           // Continue/Complete/Revise decision point
    Completed           // Workflow finished
}

public enum ParticipantRole
{
    EndUser,           // Task requester
    Agent1,            // First agent in couple (e.g., CopyWriter)
    Agent2,            // Second agent in couple (e.g., Reviewer)
    HumanReviewer      // Quality reviewer (same person as EndUser)
}

public class WorkflowState
{
    public string WorkflowId { get; set; } = Guid.NewGuid().ToString();
    public WorkflowStage CurrentStage { get; set; }
    public ParticipantRole ActiveRole { get; set; }
    public bool AwaitingHumanInput { get; set; }
    public string? CurrentTask { get; set; }
    public List<string> AgentNames { get; set; } = new();
    public int Iteration { get; set; } = 1;
}
```

### 5. SignalR Hub Design

```csharp
public class OrchestrationHub : Hub
{
    // Start the HITL workflow
    public async Task StartHitlWorkflow(string task, string agentTeam);

    // Handle human input responses (bridges back to SK)
    public async Task SubmitHumanInput(string requestId, string decision, string? feedback = null);

    // Get current workflow state
    public async Task GetWorkflowState();

    // Cancel/reset workflow
    public async Task ResetWorkflow();
}
```

### 6. Integration with Existing Architecture

**Reuse from MultiAgent Feature**:
- `AgentConfiguration` models (move to Shared)
- `AgentMessage` models (move to Shared)
- Agent styling/avatar logic (extract to shared components)
- `AgentService` for agent creation (extract common parts)

**New HITL-Specific Components**:
- `OrchestrationService` - Manages SK GroupChatOrchestration lifecycle
- `HitlChatManager` - Custom SK chat manager with SignalR integration
- `StreamingOrchestrationMonitor` - Captures SK responses for streaming
- `OrchestrationHub` - SignalR coordination layer

### 7. Next Steps (Ready for Step 2)

1. Extract shared components from MultiAgent to Features/Shared
2. Create basic models for HITL workflow state management
3. Build the custom HitlChatManager with async coordination
4. Implement StreamingOrchestrationMonitor for ResponseCallback
5. Create OrchestrationService to tie everything together
6. Build the UI with clear role separation

---
*Status: Step 1 Complete - Architecture Documented*
*Next: Step 2 - Shared Component Extraction*
