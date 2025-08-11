# Feature-Based Code Organization Proposal

## Overview
Reorganize the codebase to reflect the three main conceptual features: Chat, MultiAgent, and Orchestration. This makes the code easier to understand and navigate as a learning project.

## Proposed Structure

```
Features/
├── Chat/                           # Single-user chat (Phase 1)
│   ├── Components/
│   │   └── Chat.razor              # Move from Components/Pages/
│   ├── Models/
│   │   ├── ChatMessage.cs          # Move from Models/
│   │   └── ConversationData.cs     # Move from Models/
│   ├── Services/
│   │   ├── ChatService.cs          # Move from Services/
│   │   └── ConversationPersistenceService.cs  # Move from Services/
│   └── Hubs/
│       └── ChatHub.cs              # Move from Hubs/
│
├── MultiAgent/                     # Multi-agent conversations (Phase 2.1)
│   ├── Components/
│   │   └── MultiAgentChat.razor    # Move from Components/Pages/
│   ├── Models/
│   │   ├── AgentConfiguration.cs   # Move from Models/
│   │   ├── AgentMessage.cs         # Move from Models/
│   │   └── AgentStreamingMessage.cs # Move from Models/
│   ├── Services/
│   │   ├── AgentService.cs         # Move from Services/
│   │   ├── AgentTemplates.cs       # Move from Services/
│   │   └── PersonalityReinforcement.cs # Move from Services/
│   └── Hubs/
│       └── MultiAgentHub.cs        # Move from Hubs/
│
├── Orchestration/                  # Human-in-the-loop workflows (Phase 2.2+)
│   ├── Components/
│   │   └── OrchestrationChat.razor # Future component
│   ├── Models/
│   │   ├── OrchestrationMessage.cs # Future model
│   │   ├── WorkflowState.cs        # Future model
│   │   └── HumanInputRequest.cs    # Future model
│   ├── Services/
│   │   ├── OrchestrationService.cs # Future service
│   │   └── WorkflowManager.cs      # Future service
│   └── Hubs/
│       └── OrchestrationHub.cs     # Future hub
│
└── Shared/                         # Cross-feature components
    ├── Models/
    │   └── OpenAIConfig.cs          # Keep in Models/
    ├── Services/
    │   └── [Shared services if any emerge]
    ├── Components/
    │   ├── Layout/                  # Keep existing layout
    │   └── [Shared UI components]
    └── Plugins/
        └── WeatherFactsPlugin.cs    # Move from Plugins/
```

## Benefits

### 1. **Clear Mental Model**
- Each folder represents a distinct learning concept
- Easy to understand what each phase/feature contains
- Natural progression: Chat → MultiAgent → Orchestration

### 2. **Reduced Cognitive Load**
- When working on multi-agent features, only look in `Features/MultiAgent/`
- No need to hunt through generic `Services/` or `Models/` folders
- Related code lives together

### 3. **Better Learning Experience**
- Students can follow the roadmap by exploring folders in order
- Each feature is self-contained and understandable
- Clear boundaries between concepts

### 4. **Future-Proof for Reuse**
- When pieces get reused, they can be moved to `Shared/`
- Easy to see dependencies between features
- Refactoring decisions become more obvious
