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
- ✅ **Phase 1.1, 1.2, 1.3, 1.4 & 1.5 complete** - streaming chat with persistent user conversations
- ✅ Real-time streaming responses with visual indicators
- ✅ Conversation history management with JSON persistence
- ✅ First native C# plugin integrated and working
- ✅ Persistent user IDs using browser localStorage
- ✅ JSON-based conversation storage keyed by user ID

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

## Next Phases (Planned)
- **Phase 2**: AI Orchestration (Function calling, RAG, External search)
- **Phase 3**: Agent Architecture (Single agent, Multi-agent collaboration)
- **Phase 4**: Advanced Capabilities (Multi-modal, Processes)
- **Phase 5**: Production Ready Features (Login, Database, etc.)

## Phase 5: Production Ready Features
- [ ] Implement user authentication with ASP.NET Core Identity
- [ ] Replace JSON persistence with a database (e.g., LiteDB or EF Core)
- [ ] Add robust error logging and monitoring
- [ ] Implement unit and integration tests
- [ ] Create a CI/CD pipeline for deployment

---
*Last Updated: August 11, 2025*
