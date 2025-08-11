# Semantic Kernel Learning Roadmap

## Current State (August 11, 2025)
- ✅ Basic Blazor Server application (.NET 9) with Semantic Kernel integration
- ✅ Real-time chat interface using SignalR
- ✅ Tailwind CSS styling with modern UI
- ✅ OpenAI GPT-4o-mini integration working
- ✅ HTTP-only development environment (avoiding certificate issues)
- ✅ Clean, focused application (removed template components)
- ✅ Professional home page with learning roadmap
- ✅ Polished chat UX with auto-focus and reactive controls
- ✅ **Phase 1.1, 1.2, 1.3, 1.4, 1.5, 2.1, 2.1.5 & 2.2 (Manual HITL) complete** - streaming chat with persistent user conversations and comprehensive multi-agent orchestration
- ✅ Real-time streaming responses with visual indicators
- ✅ Conversation history management with JSON persistence
- ✅ First native C# plugin integrated and working
- ✅ Persistent user IDs using browser localStorage
- ✅ JSON-based conversation storage keyed by user ID
- ✅ Multi-agent foundation with real-time agent-to-agent conversations
- ✅ Visual agent distinction with avatars and color schemes
- ✅ Configuration-driven agent display with responsive status bar
- ✅ Team switching with automatic conversation clearing
- ✅ Five distinct agent teams: Creative+Analytical, CopyWriter+Reviewer, Research, Debate, Technical
- ✅ Conversation export functionality with structured JSON download
- ✅ **Human-in-the-Loop orchestration with manual round-robin agent collaboration**

## Technical Architecture Decisions
- **Framework**: .NET 9 Blazor Server
- **Real-time Communication**: SignalR for chat functionality
- **Styling**: Tailwind CSS via CDN
- **Approach**: Server-side focused, minimal JavaScript interop
- **Code Style**: Functional programming where practical, conventional commits
- **Code Organization**: Feature-based folder structure (`Features/Chat`, `Features/MultiAgent`, `Features/Orchestration`)
- **HITL Strategy**: Manual round-robin orchestration (SK GroupChat deemed too problematic)

## Project Refinements

### Meta: Codebase Reorganization (August 2025)
- [x] Reorganize codebase into a feature-centric folder structure
    - [x] Create `Features/` directory for `Chat`, `MultiAgent`, `Orchestration`, and `Shared` concerns
    - [x] Move all related components, models, services, and hubs into their respective feature folders
    - [x] Update namespaces and `using` statements across the project to reflect new locations
    - [x] Introduce `ServiceCollectionExtensions` for cleaner service registration in `Program.cs`
    - [x] Clean up old, empty directories after refactoring


## Phase 1: Core Chat Application Foundation

### 1.1 Basic Infrastructure Setup
- [x] Add Semantic Kernel NuGet packages
- [x] Add SignalR services and hub
- [x] Integrate Tailwind CSS via CDN
- [x] Configure AI service (OpenAI/Azure OpenAI)
- [x] Set up basic dependency injection for Kernel

### 1.2 Basic Chat Interface
- [x] Create ChatHub (SignalR)
- [x] Build Chat Razor component
- [x] Replace Home.razor with Chat interface
- [x] Implement basic request/response flow
- [x] Fix layout and styling issues
- [x] Configure HTTP-only development environment
- [x] Test end-to-end chat functionality
- [x] Remove template components (Counter, Weather)
- [x] Enhance home page with project overview
- [x] Improve chat input UX (auto-focus, reactive send button)
- [x] Add focus management after sending/receiving messages

### 1.3 Core Chat Functionality
- [x] Implement Kernel & Services setup
- [x] Create ChatService with conversation management
- [x] Add streaming chat responses via SignalR
- [x] Implement real-time message chunking
- [x] Add conversation history per connection
- [x] Create streaming UI with typing indicators
- [x] Handle streaming completion and error states
- [x] Basic error handling and validation

### 1.4 First Plugin Integration
- [x] Create simple native C# plugin (`WeatherFactsPlugin`)
- [x] Register plugin with the Kernel
- [x] Enable auto-invoking function calls in `ChatService`
- [x] Test function calling from chat with bombastic weather facts
- [x] Update system prompt to guide function usage

### 1.5 Enhanced Chat Features
- [x] Implement prompt templating from external file
- [x] Ensure prompt files are copied to build output
- [x] Use templating to inject dynamic data (e.g., current date)
- [x] Implement persistent user ID using browser local storage
- [x] Persist conversation state to JSON, keyed by user ID
- [x] Load conversation history on user reconnect
- [x] Filter out streaming artifacts and tool messages from persistence
- [x] Implement incremental conversation saving to prevent duplication
- [x] Add clear chat button with confirmation dialog
- [x] Exclude conversation data from git tracking (.gitignore)

## Next Phases (Planned)
- **Phase 2**: AI Orchestration (Multi-agent collaboration, Human-in-the-loop)
- **Phase 3**: Agent Architecture (Custom agent teams, Advanced workflows)
- **Phase 4**: Advanced Capabilities (Multi-modal, Processes)
- **Phase 5**: Production Ready Features (Login, Database, etc.)

## Phase 2: AI Orchestration

### 2.1 Multi-Agent Foundation ✅ **COMPLETE**
- [x] Create "Agent Teams" navigation section
- [x] Build MultiAgentChat.razor component with distinct UI design
- [x] Implement AgentService for managing multiple ChatCompletionAgents
- [x] Create agent configuration models with visual styling
- [x] Add basic agent-to-agent conversation flow
- [x] Implement visual distinction system (avatars, colors, message styling)
- [x] Test with simple two-agent conversations
- [x] Add multi-agent SignalR hub for real-time communication
- [x] Implement turn-taking mechanism for agent deliberation
- [x] Dynamic agent status bar with responsive design
- [x] Configuration-driven agent display (avatars, colors, descriptions)
- [x] Team switching with conversation clearing
- [x] Enhanced UX with clear conversation button and team selection
- [x] Responsive agent status cards with hover effects and tooltips
- [x] Real-time agent loading on page initialization
- [x] **Quick Wins**: Multiple agent teams (Research, Debate, Technical teams)
- [x] **Quick Wins**: Conversation export functionality with JSON download

### 2.1.5 Agent Team Expansion ✅ **COMPLETE**
- [x] Research Team: Researcher + Fact Checker for investigation tasks
- [x] Debate Team: Advocate + Devil's Advocate for exploring perspectives
- [x] Technical Team: Architect + Code Reviewer for development discussions
- [x] Enhanced color schemes and visual distinction for all agent types
- [x] Conversation export with structured JSON including metadata and agents
- [x] Download functionality with timestamped filenames

### 2.2 Human-in-the-Loop Orchestration ✅ **MOSTLY COMPLETE** 
- [x] **SK GroupChat Investigation**: Attempted SK GroupChatOrchestration integration
- [x] **Strategic Pivot**: SK GroupChat marked as too problematic for our requirements  
- [x] **Manual Round-Robin Implementation**: User → Agent A → Agent B → Agent A → Agent B → Human Review
- [x] **Real AI Integration**: Semantic Kernel ChatCompletion service with agent-specific prompts
- [x] **Agent Streaming**: Implemented MultiAgent-style streaming with typing indicators
- [x] **Agent Styling**: Full visual distinction with avatars, colors, and role-based UI
- [x] **Workflow States**: Dynamic status showing current agent during collaboration
- [x] **Human Review UI**: Proper message types triggering approve/cancel/revise/continue actions
- [x] **Orchestration Architecture**: Complete HITL infrastructure with WorkflowState, HumanReviewDecision, OrchestrationMessage models
- [x] **Testing**: Successfully tested with CopyWriter+Reviewer team for political slogan generation

**Lessons Learned**:
- SK GroupChatOrchestration adds complexity without clear value for simple round-robin workflows
- Manual orchestration provides better control and simpler debugging
- MultiAgent chat pattern is proven and should be foundation for HITL workflows
- Real-time streaming and visual feedback are critical for engaging UX

**Known Issues (To Fix)**:
- [ ] **Duplicate System Messages**: HITL shows pointless system message at conversation start - remove it
- [ ] **UI Overflow**: Agent response bubbles extend past screen edge - add proper margins/padding  
- [ ] **Human Actions Non-Functional**: Review buttons (approve/revise/continue/cancel) need actual implementations

### 2.3 Human Review Actions Implementation
- [ ] **Approve Action**: Seal chat and prohibit further interaction
- [ ] **Revise Action**: Restart agents with revision instructions + original user request
- [ ] **Continue Action**: Allow agents to iterate once more in round-robin
- [ ] **Cancel Action**: Destroy chat (refresh/reset) 
- [ ] **Request from User Action**: Ask user a clarifying question to refine their prompt
- [ ] **UI Polish**: Fix conversation bubble overflow and remove duplicate system messages

### 2.4 Enhanced UX & Multiple Workflows
- [ ] Add agent team selection (multiple predefined teams)
- [ ] Enhance UI with progress indicators and workflow status  
- [ ] Implement different orchestration patterns (analyst/critic, researcher/fact-checker)
- [ ] Add workflow templates and task guidance
- [ ] Improve error handling and recovery mechanisms
- [ ] Add comprehensive logging and monitoring

## Technical Lessons Learned

### SK GroupChatOrchestration Assessment (August 11, 2025)
**Investigation Result**: Marked as too problematic for our use case

**Issues Encountered**:
- Complex abstraction that adds little value for simple round-robin workflows
- Experimental features (SKEXP0001, SKEXP0110) requiring pragma warnings
- Method signature mismatches and integration complexity
- Debugging difficulty due to abstraction layers

**Successful Alternative**: 
- Manual round-robin orchestration using proven MultiAgent patterns
- Direct Semantic Kernel ChatCompletion service integration
- Simple, debuggable workflow with clear control points
- Maintains all desired features (streaming, styling, human review) with less complexity

**Recommendation**: Stick with manual orchestration for simple workflows. Consider SK GroupChat only for complex multi-agent scenarios requiring sophisticated orchestration logic.

## Known Technical Challenges

### Agent Personality Convergence
**Issue**: Agents tend to adopt similar conversational styles during multi-turn conversations, losing their distinct personalities.

**Root Cause**: Context bleeding where agents see the full conversation history and gradually align their response styles.

**Potential Solutions**:
- Stronger persona reinforcement in system prompts
- Context isolation techniques (limit what each agent sees)
- Persona-specific few-shot examples in prompts
- Regular personality "reset" injections during conversations
- Separate context management per agent role
- Use of distinctive vocabulary and linguistic patterns per agent

**Current Status**: Identified during Phase 2.1 testing. Requires research and experimentation in Phase 2.2.

## Phase 3: Agent Architecture

### 3.1 Custom Agent Teams
- [ ] User interface for creating custom agent configurations
- [ ] Agent template library and sharing system
- [ ] Advanced agent personality and instruction management
- [ ] Custom workflow pattern designer

### 3.2 Multi-Agent Persistence
- [ ] Extend JSON storage to handle multi-agent conversations
- [ ] Agent team configuration persistence
- [ ] Multi-agent conversation history and replay

## Phase 4: Advanced Capabilities

## Phase 5: Production Ready Features
- [ ] Implement user authentication with ASP.NET Core Identity
- [ ] Replace JSON persistence with a database (e.g., LiteDB or EF Core)
- [ ] Add robust error logging and monitoring
- [ ] Implement unit and integration tests
- [ ] Create a CI/CD pipeline for deployment

---
*Last Updated: August 11, 2025*
