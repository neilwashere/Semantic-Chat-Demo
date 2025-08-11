# Semantic Kernel Concepts Checklist

A progressive guide to learning Semantic Kernel by building a chat application.

### Phase 1: Core Chat Application
- [ ] **Kernel & Services**: Set up the `Kernel` and configure AI chat completion services.
- [ ] **Basic Chat Loop**: Implement a simple request/response chat loop.
- [ ] **Streaming Chat**: Update the chat loop to handle streaming responses for a better user experience.
- [ ] **Plugins & Functions**: Create and import a basic plugin with native C# functions.
- [ ] **Prompt Templating**: Use prompt templates (e.g., Handlebars) to create more dynamic and context-aware prompts.
- [ ] **Chat History**: Manage and inject conversation history into prompts.

### Phase 2: Enhancing with AI Orchestration
- [ ] **Function Calling**: Enable the AI model to autonomously invoke functions from your plugins.
- [ ] **Memory & RAG (Retrieval-Augmented Generation)**:
    - [ ] Generate embeddings for text data.
    - [ ] Ingest data into a vector store.
    - [ ] Implement RAG to answer questions using your private data.
- [ ] **External Search**: Integrate a web search plugin (e.g., Bing) to ground responses with real-time information.

### Phase 3: Building with Agents
- [ ] **Single Agent**: Refactor the chat loop to use a `ChatCompletionAgent`.
- [ ] **Agents with Tools**: Provide the agent with your existing plugins as tools.
- [ ] **Multi-Agent Collaboration (Agent-to-Agent)**:
    - [ ] Implement a group chat where multiple agents collaborate to solve a problem.
    - [ ] Introduce a "human-in-the-loop" for approval or feedback.

### Phase 4: Advanced Capabilities
- [ ] **Multi-Modality**:
    - [ ] **Text-to-Image**: Generate images from text prompts.
    - [ ] **Text-to-Audio**: Generate audio from text.
    - [ ] **Audio-to-Text**: Transcribe audio files into text.
    - [ ] **Vision**: Enable the application to understand and discuss images provided by the user.
- [ ] **Complex Workflows (Processes)**: For complex, long-running, or stateful tasks, orchestrate workflows using the Processes framework.
- [ ] **Production Readiness**:
    - [ ] **Dependency Injection**: Structure the application for testability and maintainability.
    - [ ] **Filtering & Telemetry**: Add filters for logging, telemetry, or implementing custom logic
