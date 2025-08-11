# Phase 2.1 Implementation Plan: Foundation & Basic Multi-Agent Chat

## Objective
Establish core multi-agent infrastructure without orchestration. Build the foundation for agent-to-agent conversations with visual distinction and real-time streaming.

## Success Criteria
- User can navigate to Agent Teams section
- Two agents can have a conversation visible in real-time
- Each agent has distinct visual identity
- Messages stream in real-time via SignalR

## Implementation Steps

### Step 1: Navigation & Basic UI Structure
**Goal**: Add new section to navigation and create basic multi-agent chat page

**Tasks**:
1. Add "Agent Teams" menu item to `Components/Layout/NavMenu.razor`
2. Create `Components/Pages/MultiAgentChat.razor` with basic layout
3. Add routing for `/agent-teams` page
4. Create placeholder UI structure (header, chat area, input)

**Files to Create/Modify**:
- `Components/Layout/NavMenu.razor` (modify)
- `Components/Pages/MultiAgentChat.razor` (new)

**Acceptance Criteria**:
- Navigation shows "Agent Teams" option
- Clicking navigates to new page with basic layout
- Page shows "Multi-Agent Chat" heading and placeholder content

### Step 2: Agent Configuration Models
**Goal**: Create data models for agent definitions and visual styling

**Tasks**:
1. Create `Models/AgentConfiguration.cs` with agent properties
2. Create `Models/AgentMessage.cs` for multi-agent message structure
3. Define agent styling properties (avatar, color scheme)
4. Create sample agent configurations for testing

**Files to Create**:
- `Models/AgentConfiguration.cs`
- `Models/AgentMessage.cs`

**Model Structure**:
```csharp
public class AgentConfiguration
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Instructions { get; set; }
    public string AvatarEmoji { get; set; } // Simple emoji avatars for now
    public string ColorScheme { get; set; } // CSS class name
}

public class AgentMessage
{
    public string Id { get; set; }
    public string Content { get; set; }
    public string AgentName { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsComplete { get; set; }
    public bool IsStreaming { get; set; }
}
```

**Acceptance Criteria**:
- Models compile without errors
- Sample agent configurations can be instantiated
- AgentMessage supports streaming scenarios

### Step 3: AgentService Implementation
**Goal**: Create service to manage multiple ChatCompletionAgents

**Tasks**:
1. Create `Services/AgentService.cs`
2. Implement agent initialization from configurations
3. Add method to get agent response given conversation history
4. Handle agent-to-agent conversation flow
5. Register service in `Program.cs`

**Files to Create/Modify**:
- `Services/AgentService.cs` (new)
- `Program.cs` (modify)

**Service Interface**:
```csharp
public class AgentService
{
    public void InitializeAgents(List<AgentConfiguration> configurations);
    public IAsyncEnumerable<string> GetAgentResponseAsync(string agentName, ChatHistory history);
    public List<AgentConfiguration> GetAvailableAgents();
    public ChatCompletionAgent GetAgent(string name);
}
```

**Acceptance Criteria**:
- Service can initialize multiple agents with different configurations
- Service can generate responses from specific agents
- Service integrates with existing Kernel dependency injection

### Step 4: SignalR Hub for Multi-Agent Communication
**Goal**: Create dedicated hub for multi-agent real-time messaging

**Tasks**:
1. Create `Hubs/MultiAgentHub.cs`
2. Implement methods for starting agent conversations
3. Add streaming support for agent-to-agent messages
4. Handle agent response coordination
5. Register hub in `Program.cs`

**Files to Create/Modify**:
- `Hubs/MultiAgentHub.cs` (new)
- `Program.cs` (modify)

**Hub Methods**:
```csharp
public async Task StartAgentConversation(string userMessage, string agentTeam);
public async Task SendAgentMessage(string agentName, string message);
// Stream methods for real-time agent responses
```

**Acceptance Criteria**:
- Hub can coordinate communication between multiple agents
- Streaming works for agent-to-agent conversations
- Client receives distinct messages from different agents

### Step 5: Multi-Agent UI Components
**Goal**: Build UI components that visually distinguish between agents

**Tasks**:
1. Create agent avatar system (emoji-based initially)
2. Implement color-coded message bubbles
3. Add agent identification in message headers
4. Create agent roster/status display
5. Style agent messages differently from user messages

**Files to Modify**:
- `Components/Pages/MultiAgentChat.razor`
- Add CSS classes for agent styling

**UI Requirements**:
- Each agent has unique avatar and color
- Agent names clearly visible in message headers
- Message bubbles styled consistently per agent
- Easy to follow conversation flow between agents

**Acceptance Criteria**:
- Two agents visually distinguishable in conversation
- Messages clearly attributed to correct agent
- Conversation flow easy to follow
- Responsive design works on different screen sizes

### Step 6: Basic Two-Agent Conversation Flow
**Goal**: Implement simple back-and-forth between two test agents

**Tasks**:
1. Create two test agent configurations (Agent A, Agent B)
2. Implement simple conversation starter logic
3. Add basic turn-taking mechanism
4. Test agent-to-agent communication end-to-end
5. Add conversation termination after N exchanges

**Test Scenario**:
- User provides topic/question
- Agent A responds with initial thoughts
- Agent B responds to Agent A
- Continue for 3-4 exchanges
- Terminate conversation

**Acceptance Criteria**:
- Two agents can successfully have a conversation
- Messages stream in real-time
- Conversation follows logical turn-taking
- User can observe full agent deliberation

## Technical Considerations

### Dependencies to Add
Check if we need additional Semantic Kernel packages for ChatCompletionAgent:
```xml
<PackageReference Include="Microsoft.SemanticKernel.Agents.Abstractions" Version="1.61.0" />
<PackageReference Include="Microsoft.SemanticKernel.Agents.Core" Version="1.61.0" />
```

### Performance Considerations
- Ensure agent initialization doesn't block UI
- Implement proper disposal of agents
- Monitor memory usage with multiple agents

### Error Handling
- Handle agent initialization failures gracefully
- Implement timeouts for agent responses
- Add fallback for failed agent communications

## Testing Strategy
1. **Unit Tests**: AgentService methods work correctly
2. **Integration Tests**: SignalR hub communication
3. **Manual Testing**: Full user flow with two agents
4. **Performance Testing**: Multiple concurrent agent conversations

## Definition of Done
- [ ] User can navigate to Agent Teams section
- [ ] Two distinct agents visible in UI with unique styling
- [ ] Agents can have basic conversation triggered by user input
- [ ] Real-time streaming works for all agent messages
- [ ] Code follows project conventions and is properly documented
- [ ] No breaking changes to existing chat functionality
- [ ] Ready for Phase 2.2 orchestration layer

## Estimated Effort
**Total**: ~2-3 days of focused development

**Breakdown**:
- Step 1 (Navigation/UI): 0.5 day
- Step 2 (Models): 0.5 day
- Step 3 (AgentService): 1 day
- Step 4 (SignalR Hub): 0.5 day
- Step 5 (UI Components): 0.5 day
- Step 6 (Integration/Testing): 0.5 day

## Next Phase Preparation
This phase builds the foundation for Phase 2.2 where we'll add:
- GroupChatOrchestration integration
- HumanInTheLoopChatManager
- User input/feedback mechanisms
- Workflow termination logic

---
*Created: August 11, 2025*
*Status: Ready for Implementation*
