# Semantic Kernel Learning Roadmap

## Current State (August 11, 2025)
- Basic Blazor Server application (.NET 9) with standard template components
- No Semantic Kernel integration yet
- No SignalR integration yet
- Using default Bootstrap styling (to be replaced with Tailwind)

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
- [ ] Create ChatHub (SignalR)
- [ ] Build Chat Razor component
- [ ] Replace Home.razor with Chat interface
- [ ] Implement basic request/response flow

### 1.3 Core Chat Functionality
- [ ] Implement Kernel & Services setup
- [ ] Create basic chat loop (non-streaming)
- [ ] Add streaming chat responses via SignalR
- [ ] Basic error handling and validation

### 1.4 First Plugin Integration
- [ ] Create simple native C# plugin
- [ ] Import plugin into Kernel
- [ ] Test function calling from chat

### 1.5 Enhanced Chat Features
- [ ] Implement prompt templating (Handlebars)
- [ ] Add chat history management
- [ ] Persist conversation state

## Next Phases (Planned)
- **Phase 2**: AI Orchestration (Function calling, RAG, External search)
- **Phase 3**: Agent Architecture (Single agent, Multi-agent collaboration)
- **Phase 4**: Advanced Capabilities (Multi-modal, Processes, Production features)

---
*Last Updated: August 11, 2025*
