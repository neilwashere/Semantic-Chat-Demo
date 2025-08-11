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
- ✅ **Phase 1.1, 1.2, 1.3, 1.4, 1.5 & 2.1 complete** - streaming chat with persistent user conversations and multi-agent orchestration with enhanced UI
- ✅ Real-time streaming responses with visual indicators
- ✅ Conversation history management with JSON persistence
- ✅ First native C# plugin integrated and working
- ✅ Persistent user IDs using browser localStorage
- ✅ JSON-based conversation storage keyed by user ID
- ✅ Multi-agent foundation with real-time agent-to-agent conversations
- ✅ Visual agent distinction with avatars and color schemes
- ✅ Configuration-driven agent display with responsive status bar
- ✅ Team switching with automatic conversation clearing

## Technical Architecture Decisions
- **Framework**: .NET 9 Blazor Server
- **Real-time Communication**: SignalR for chat functionality
- **Styling**: Tailwind CSS via CDN
- **Approach**: Server-side focused, minimal JavaScript interop
- **Code Style**: Functional programming where practical, conventional commits

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

### 2.2 Human-in-the-Loop Orchestration
- [ ] Integrate Semantic Kernel's GroupChatOrchestration
- [ ] Implement custom HumanInTheLoopChatManager
- [ ] Create orchestration workflow service
- [ ] Add "Human Input Required" message types to UI
- [ ] Implement user response handling and workflow continuation
- [ ] Add termination logic based on user satisfaction
- [ ] Create CopyWriter + Reviewer agent team configuration
- [ ] Improve agent personality consistency (prevent analytical convergence)

### 2.3 Enhanced UX & Multiple Workflows
- [ ] Add agent team selection (multiple predefined teams)
- [ ] Enhance UI with progress indicators and workflow status
- [ ] Implement different orchestration patterns (analyst/critic, researcher/fact-checker)
- [ ] Add workflow templates and task guidance
- [ ] Improve error handling and recovery mechanisms
- [ ] Add comprehensive logging and monitoring

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
