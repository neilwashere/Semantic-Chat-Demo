# HITL Orchestration Implementation Plan

## Overview
Implementation of Human-in-the-Loop (HITL) orchestration using Semantic Kernel's GroupChatOrchestration with real-time streaming capabilities. This feature enables a 4-entity workflow: End User → Agent Couple → Human Reviewer → Decision Point, where the same person switches between End User and Human Reviewer roles.

## Architecture Goals
- ✅ Use SK's GroupChatOrchestration as core orchestration engine
- ✅ Maintain real-time streaming for all agent responses
- ✅ Clear role separation (End User vs Human Reviewer) in UI
- ✅ Proper workflow state management
- ✅ Coexist with existing multi-agent chat feature
- ✅ Handle async coordination between SK orchestration and SignalR

## Technical Challenges & Solutions

### Challenge 1: Async Coordination for Human Input
**Problem**: SK's `InteractiveCallback` expects synchronous return, but SignalR is async
**Solution**: Use `TaskCompletionSource` to bridge async gap:

```csharp
private readonly ConcurrentDictionary<string, TaskCompletionSource<ChatMessageContent>>
    pendingHumanInputs = new();

// In InteractiveCallback
var tcs = new TaskCompletionSource<ChatMessageContent>();
pendingHumanInputs[requestId] = tcs;
await hubContext.Clients.Client(connectionId).SendAsync("RequestHumanInput", requestId);
return await tcs.Task; // Wait for SignalR response

// In SignalR hub method
public async Task SubmitHumanInput(string requestId, string input)
{
    if (pendingHumanInputs.TryRemove(requestId, out var tcs))
    {
        tcs.SetResult(new ChatMessageContent(AuthorRole.User, input));
    }
}
```

### Challenge 2: Streaming Integration with GroupChatOrchestration
**Problem**: SK's orchestration doesn't inherently support streaming
**Solution**: Use `ResponseCallback` to capture agent responses and forward to SignalR

### Challenge 3: Role Management in UI
**Problem**: Same person needs clear context switching between End User and Human Reviewer
**Solution**: Explicit role state management with UI indicators

## Implementation Steps

### Step 1: Package Dependencies & Architecture Setup ✅
**Objective**: Add required SK orchestration packages and plan the streaming integration

**Tasks**:
- ✅ Add `Microsoft.SemanticKernel.Agents.Orchestration` package
- ✅ Create folder structure for HITL feature
- ✅ Document streaming integration approach

**New Package Requirements**:
```xml
<PackageReference Include="Microsoft.SemanticKernel.Agents.Orchestration" Version="1.61.0-preview" />
```

### Step 2: Shared Component Extraction ✅
**Objective**: Refactor existing components for reuse across Multi-Agent and HITL features

**Completed**:
- ✅ Moved models to `Features/Shared/Models/` (AgentConfiguration, AgentMessage, AgentStreamingMessage)
- ✅ Created `BaseAgentService` with common functionality
- ✅ Updated `MultiAgent.AgentService` to inherit from `BaseAgentService`
- ✅ Updated all imports and namespaces
- ✅ Verified build and runtime functionality

**Components Extracted**:
```
Features/Shared/
  Models/
    AgentConfiguration.cs         ✅ (moved from MultiAgent)
    AgentMessage.cs              ✅ (moved from MultiAgent)
    AgentStreamingMessage.cs     ✅ (moved from MultiAgent)
  Services/
    BaseAgentService.cs          ✅ (extracted common functionality)
```

### Step 3: HITL Feature Structure Creation ✅
**Objective**: Create dedicated HITL orchestration feature

**Completed**:
- ✅ Created comprehensive workflow models (`WorkflowState`, `HumanReviewDecision`, `OrchestrationMessage`)
- ✅ Implemented `OrchestrationHub` with SignalR coordination
- ✅ Built `OrchestrationService` with workflow management and simulation
- ✅ Created `OrchestrationChat.razor` UI with role-aware interface
- ✅ Added navigation link and service registration
- ✅ Verified build and runtime functionality

**New Files Created**:
```
Features/Orchestration/
  Components/
    OrchestrationChat.razor      ✅ (HITL UI with role separation)
  Models/
    WorkflowState.cs             ✅ (workflow state management)
    OrchestrationMessage.cs      ✅ (extends AgentMessage for HITL)
    HumanReviewDecision.cs       ✅ (approve/revise/complete)
  Services/
    OrchestrationService.cs      ✅ (core HITL workflow logic with simulation)
  Hubs/
    OrchestrationHub.cs          ✅ (SignalR hub for HITL)
```

**Key Features Implemented**:
- ✅ **Workflow State Management**: Complete state machine for HITL workflows
- ✅ **Role Separation**: Clear UI distinction between End User and Human Reviewer roles
- ✅ **Decision Framework**: Approve/Revise/Continue/Cancel decision options
- ✅ **Real-time Coordination**: SignalR-based workflow progression
- ✅ **Simulation Mode**: Basic workflow simulation for testing (replaces with SK in Step 4-7)
- ✅ **Navigation Integration**: Accessible via "HITL Orchestration" menu

### Step 4: Custom Streaming-Enabled HITL Chat Manager ⏳
**Objective**: Refactor existing components for reuse across Multi-Agent and HITL features

**Components to Extract**:
```
Features/Shared/
  Models/
    AgentConfiguration.cs         (move from MultiAgent)
    AgentMessage.cs              (move from MultiAgent)
    AgentStreamingMessage.cs     (move from MultiAgent)
  Services/
    BaseAgentService.cs          (extract common functionality)
  Components/
    AgentMessageBubble.razor     (extract from MultiAgentChat)
    AgentStatusBar.razor         (extract from MultiAgentChat)
```

### Step 3: HITL Feature Structure Creation
**Objective**: Create dedicated HITL orchestration feature

**New Files**:
```
Features/Orchestration/
  Components/
    OrchestrationChat.razor      (new HITL UI)
  Models/
    WorkflowState.cs             (workflow state management)
    OrchestrationMessage.cs      (extends AgentMessage for HITL)
    HumanReviewDecision.cs       (approve/revise/complete)
  Services/
    OrchestrationService.cs      (core HITL workflow logic)
    HitlChatManager.cs           (custom HumanInTheLoopChatManager)
    StreamingOrchestrationMonitor.cs (bridge SK orchestration + SignalR streaming)
  Hubs/
    OrchestrationHub.cs          (SignalR hub for HITL)
```

### Step 4: Custom Streaming-Enabled HITL Chat Manager
**Objective**: Create SK-compliant chat manager that supports streaming callbacks

**Key Implementation**:
```csharp
public sealed class HitlChatManager(
    IHubContext<OrchestrationHub> hubContext,
    string connectionId,
    string writerName,
    string reviewerName) : HumanInTheLoopChatManager
{
    // Set InteractiveCallback to request user input via SignalR
    // Override methods to enable streaming via ResponseCallback
}
```

### Step 5: Streaming Integration via OrchestrationMonitor
**Objective**: Bridge SK's ResponseCallback with SignalR streaming

### Step 6: OrchestrationHub - SignalR Coordination
**Objective**: Create hub that coordinates between UI, SK orchestration, and human input

**Key Methods**:
```csharp
public class OrchestrationHub : Hub
{
    public async Task StartHitlWorkflow(string task, string agentTeam);
    public async Task SubmitHumanInput(string decision, string feedback);
    public async Task GetWorkflowState();
}
```

### Step 7: OrchestrationService - Core Business Logic
**Objective**: Implement the main orchestration logic using SK's GroupChatOrchestration

### Step 8: UI Implementation - Role-Aware Interface
**Objective**: Create intuitive HITL interface with clear role separation

**UI Components**:
- **Workflow Progress Indicator**: Shows current stage
- **Role Context Panel**: Clear indication of current role
- **Human Input Modal**: Appears when human input needed
- **Decision Panel**: Approve/Revise/Complete options

### Step 9: Navigation & Integration
**Objective**: Add HITL orchestration to navigation and ensure coexistence

## Code Style Guidelines
- ✅ Use primary constructor syntax where appropriate
- ✅ Avoid underscore prefix for private fields (use standard naming)
- ✅ Follow existing project conventions
- ✅ Maintain consistent namespace structure

## Workflow Design (4-Entity Pattern)

```
End User (Initial Request)
    ↓
Agent Couple (Collaboration)
    ↓
Human Reviewer (Same person, different role)
    ↓
Decision Point (Continue/Complete/Revise)
    ↓
[Loop back to Agent Couple if revision needed]
```

## Success Criteria
- ✅ Uses SK's GroupChatOrchestration as the core orchestration engine
- ✅ Maintains real-time streaming for all agent responses
- ✅ Clear role separation (End User vs Human Reviewer) in UI
- ✅ Proper workflow state management
- ✅ Coexists with existing multi-agent chat feature
- ✅ Handles async coordination between SK orchestration and SignalR

## Estimated Timeline
- **Step 1**: 0.5 day (Dependencies & Research)
- **Step 2**: 1 day (Shared Component Extraction)
- **Step 3**: 0.5 day (Feature Structure)
- **Step 4**: 1.5 days (Custom HITL Chat Manager)
- **Step 5**: 1 day (Streaming Integration)
- **Step 6**: 1 day (OrchestrationHub)
- **Step 7**: 1.5 days (OrchestrationService)
- **Step 8**: 1 day (UI Implementation)
- **Step 9**: 0.5 day (Integration & Testing)

**Total**: ~7-8 days

---
*Created: August 11, 2025*
*Status: Step 1 - Ready to Begin*
