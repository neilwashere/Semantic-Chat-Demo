using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticChatDemo.Features.Orchestration.Hubs;
using SemanticChatDemo.Features.Orchestration.Models;
using SemanticChatDemo.Features.Shared.Models;
using SemanticChatDemo.Features.Shared.Services;
using System.Collections.Concurrent;

namespace SemanticChatDemo.Features.Orchestration.Services;

/// <summary>
/// Core service for managing Human-in-the-Loop orchestration workflows
/// </summary>
public class OrchestrationService(
    IHubContext<OrchestrationHub> hubContext,
    Kernel kernel,
    ILoggerFactory loggerFactory,
    ILogger<OrchestrationService> logger)
{
    private readonly ConcurrentDictionary<string, WorkflowState> activeWorkflows = new();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<HumanReviewDecision>> pendingHumanInputs = new();

    /// <summary>
    /// Start a new HITL workflow for the specified connection
    /// </summary>
    public async Task StartWorkflowAsync(string task, string agentTeam, string connectionId)
    {
        try
        {
            logger.LogInformation("Starting workflow for connection: {ConnectionId} with task: {Task}", connectionId, task);

            // Create new workflow state
            var workflowState = new WorkflowState
            {
                CurrentTask = task,
                AgentTeam = agentTeam,
                CurrentStage = WorkflowStage.UserRequest,
                ActiveRole = ParticipantRole.EndUser
            };

            // Store the workflow
            activeWorkflows[connectionId] = workflowState;

            // Send initial workflow state
            await hubContext.Clients.Client(connectionId)
                .SendAsync("WorkflowStateUpdate", workflowState);

            // Send confirmation message
            var startMessage = OrchestrationMessage.CreateSystemMessage(
                workflowState,
                $"🚀 Starting HITL workflow with {GetTeamDisplayName(agentTeam)} team. Task: {task}",
                "start"
            );

            await hubContext.Clients.Client(connectionId)
                .SendAsync("ReceiveOrchestrationMessage", startMessage);

            // Try SK GroupChat first, fall back to manual round-robin if it doesn't work
            var skWorked = await TrySkGroupChatAsync(task, agentTeam, connectionId);
            if (!skWorked)
            {
                logger.LogInformation("Using manual round-robin orchestration for connection {ConnectionId}", connectionId);
                
                // Use manual round-robin orchestration instead of simulation
                await StartRoundRobinOrchestration(task, agentTeam, connectionId);
            }

            logger.LogInformation("Successfully started workflow for connection: {ConnectionId}", connectionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting workflow for connection: {ConnectionId}", connectionId);
            throw;
        }
    }

    /// <summary>
    /// Submit a human decision and continue the workflow
    /// </summary>
    public async Task SubmitHumanDecisionAsync(string requestId, HumanReviewDecision decision, string connectionId)
    {
        try
        {
            logger.LogInformation("Processing human decision for request: {RequestId} from connection: {ConnectionId}",
                requestId, connectionId);

            // Resolve any pending human input
            if (pendingHumanInputs.TryRemove(requestId, out var tcs))
            {
                tcs.SetResult(decision);
                logger.LogInformation("Resolved pending human input for request: {RequestId}", requestId);
            }

            // Update workflow state if we have an active workflow
            if (activeWorkflows.TryGetValue(connectionId, out var workflowState))
            {
                workflowState.AwaitingHumanInput = false;
                workflowState.UpdateActivity();

                // Process the decision
                await ProcessHumanDecision(decision, workflowState, connectionId);
            }
            else
            {
                logger.LogWarning("No active workflow found for connection: {ConnectionId}", connectionId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing human decision for request: {RequestId}", requestId);
            throw;
        }
    }

    /// <summary>
    /// Get the current workflow state for a connection
    /// </summary>
    public async Task<WorkflowState?> GetWorkflowStateAsync(string connectionId)
    {
        activeWorkflows.TryGetValue(connectionId, out var workflowState);
        return await Task.FromResult(workflowState);
    }

    /// <summary>
    /// Reset the workflow for a connection
    /// </summary>
    public async Task ResetWorkflowAsync(string connectionId)
    {
        try
        {
            activeWorkflows.TryRemove(connectionId, out _);

            // Clean up any pending human inputs for this connection
            var keysToRemove = pendingHumanInputs.Keys
                .Where(key => key.StartsWith(connectionId))
                .ToList();

            foreach (var key in keysToRemove)
            {
                if (pendingHumanInputs.TryRemove(key, out var tcs))
                {
                    tcs.SetCanceled();
                }
            }

            logger.LogInformation("Reset workflow for connection: {ConnectionId}", connectionId);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resetting workflow for connection: {ConnectionId}", connectionId);
            throw;
        }
    }

    /// <summary>
    /// Clean up resources for a disconnected connection
    /// </summary>
    public async Task CleanupConnectionAsync(string connectionId)
    {
        await ResetWorkflowAsync(connectionId);
    }

    /// <summary>
    /// Get available agent teams
    /// </summary>
    public Dictionary<string, string> GetAvailableTeams()
    {
        return new Dictionary<string, string>
        {
            { "copywriter", "CopyWriter + Reviewer" },
            { "research", "Researcher + Fact Checker" },
            { "debate", "Advocate + Devil's Advocate" },
            { "technical", "Architect + Code Reviewer" },
            { "test", "Creative + Analytical" }
        };
    }

    /// <summary>
    /// Request human input during the workflow (used by SK orchestration)
    /// </summary>
    public async Task<HumanReviewDecision> RequestHumanInputAsync(string connectionId, string prompt)
    {
        try
        {
            var requestId = $"{connectionId}_{Guid.NewGuid()}";
            var tcs = new TaskCompletionSource<HumanReviewDecision>();

            pendingHumanInputs[requestId] = tcs;

            if (activeWorkflows.TryGetValue(connectionId, out var workflowState))
            {
                workflowState.AwaitingHumanInput = true;
                workflowState.TransitionTo(WorkflowStage.HumanReview, ParticipantRole.HumanReviewer);

                var inputRequest = OrchestrationMessage.CreateHumanInputRequest(
                    workflowState, requestId, prompt);

                await hubContext.Clients.Client(connectionId)
                    .SendAsync("ReceiveOrchestrationMessage", inputRequest);

                await hubContext.Clients.Client(connectionId)
                    .SendAsync("WorkflowStateUpdate", workflowState);
            }

            // Wait for human response (with timeout)
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
            return await tcs.Task.WaitAsync(cts.Token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error requesting human input for connection: {ConnectionId}", connectionId);
            throw;
        }
    }

    /// <summary>
    /// Temporary simulation of workflow progression for Step 3 testing
    /// This will be replaced with SK GroupChatOrchestration in later steps
    /// </summary>
    private async Task SimulateWorkflowProgression(WorkflowState workflowState, string connectionId)
    {
        try
        {
            // Simulate agent collaboration stage
            workflowState.TransitionTo(WorkflowStage.AgentCollaboration, ParticipantRole.Agent1);

            var collaborationMessage = OrchestrationMessage.CreateSystemMessage(
                workflowState,
                "🤖 Agents are collaborating on your task...",
                "progress"
            );

            await hubContext.Clients.Client(connectionId)
                .SendAsync("ReceiveOrchestrationMessage", collaborationMessage);

            await hubContext.Clients.Client(connectionId)
                .SendAsync("WorkflowStateUpdate", workflowState);

            // Simulate some agent work
            await Task.Delay(2000);

            // Request human review
            var reviewPrompt = $"The agents have completed their work on: '{workflowState.CurrentTask}'. Please review their output and decide whether to approve, request revisions, or continue collaboration.";

            var decision = await RequestHumanInputAsync(connectionId, reviewPrompt);

            // Process the decision
            await ProcessHumanDecision(decision, workflowState, connectionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in workflow simulation for connection: {ConnectionId}", connectionId);
        }
    }

    /// <summary>
    /// Process a human decision and determine next workflow steps
    /// </summary>
    private async Task ProcessHumanDecision(HumanReviewDecision decision, WorkflowState workflowState, string connectionId)
    {
        try
        {
            switch (decision.Decision)
            {
                case ReviewDecision.Approve:
                    workflowState.TransitionTo(WorkflowStage.Completed);
                    var approvalMessage = OrchestrationMessage.CreateSystemMessage(
                        workflowState,
                        "✅ Workflow completed successfully! The output has been approved.",
                        "completion"
                    );
                    await hubContext.Clients.Client(connectionId)
                        .SendAsync("ReceiveOrchestrationMessage", approvalMessage);
                    break;

                case ReviewDecision.Revise:
                    workflowState.Iteration++;
                    workflowState.TransitionTo(WorkflowStage.AgentCollaboration, ParticipantRole.Agent1);
                    var revisionMessage = OrchestrationMessage.CreateSystemMessage(
                        workflowState,
                        $"🔄 Requesting revisions (Iteration {workflowState.Iteration}): {decision.Feedback}",
                        "revision"
                    );
                    await hubContext.Clients.Client(connectionId)
                        .SendAsync("ReceiveOrchestrationMessage", revisionMessage);

                    // Continue simulation for revision
                    await SimulateWorkflowProgression(workflowState, connectionId);
                    break;

                case ReviewDecision.Continue:
                    workflowState.TransitionTo(WorkflowStage.AgentCollaboration, ParticipantRole.Agent1);
                    var continueMessage = OrchestrationMessage.CreateSystemMessage(
                        workflowState,
                        "➡️ Continuing agent collaboration...",
                        "continue"
                    );
                    await hubContext.Clients.Client(connectionId)
                        .SendAsync("ReceiveOrchestrationMessage", continueMessage);
                    break;

                case ReviewDecision.Cancel:
                    workflowState.TransitionTo(WorkflowStage.Completed);
                    var cancelMessage = OrchestrationMessage.CreateSystemMessage(
                        workflowState,
                        "❌ Workflow cancelled by user.",
                        "cancellation"
                    );
                    await hubContext.Clients.Client(connectionId)
                        .SendAsync("ReceiveOrchestrationMessage", cancelMessage);
                    break;
            }

            await hubContext.Clients.Client(connectionId)
                .SendAsync("WorkflowStateUpdate", workflowState);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing human decision for connection: {ConnectionId}", connectionId);
        }
    }

    /// <summary>
    /// Get display name for agent team
    /// </summary>
    private static string GetTeamDisplayName(string teamKey)
    {
        return teamKey.ToLowerInvariant() switch
        {
            "copywriter" => "CopyWriter + Reviewer",
            "research" => "Researcher + Fact Checker",
            "debate" => "Advocate + Devil's Advocate",
            "technical" => "Architect + Code Reviewer",
            "test" => "Creative + Analytical",
            _ => teamKey
        };
    }

    /// <summary>
    /// Experimental: Try SK GroupChat orchestration (fallback to simulation if fails)
    /// </summary>
    private async Task<bool> TrySkGroupChatAsync(string task, string agentTeam, string connectionId)
    {
        // SK GroupChat abstraction marked as too problematic - always fall back to manual orchestration
        logger.LogInformation("Skipping SK GroupChat (marked as problematic), using manual round-robin orchestration");
        await Task.Delay(1); // Avoid async warning
        return false;
    }

    /// <summary>
    /// Manual Round-Robin orchestration using proven MultiAgent pattern
    /// User → Agent A → Agent B → Agent A → Agent B → HUMAN STOP → Approve/Cancel
    /// </summary>
    private async Task StartRoundRobinOrchestration(string task, string agentTeam, string connectionId)
    {
        try
        {
            logger.LogInformation("Starting round-robin orchestration for task: {Task}, team: {Team}", task, agentTeam);

            // Get the workflow state
            if (!activeWorkflows.TryGetValue(connectionId, out var workflowState))
            {
                logger.LogError("No workflow state found for connection {ConnectionId}", connectionId);
                return;
            }

            // Get agent configurations based on team
            var agentConfigs = GetTeamAgentConfigurations(agentTeam);
            if (agentConfigs.Count < 2)
            {
                logger.LogError("Need at least 2 agents for round-robin, got {Count}", agentConfigs.Count);
                await SendOrchestrationError(connectionId, "Team needs at least 2 agents for collaboration");
                return;
            }

            // Send agent configurations to UI for styling
            await hubContext.Clients.Client(connectionId).SendAsync("AgentConfigurationsUpdate", agentConfigs);

            // Start the round-robin discussion
            await ExecuteRoundRobinDiscussion(workflowState, agentConfigs, task, connectionId);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in round-robin orchestration for connection {ConnectionId}", connectionId);
            await SendOrchestrationError(connectionId, "Failed to start agent collaboration");
        }
    }

    /// <summary>
    /// Execute the round-robin discussion pattern: Agent A → Agent B → Agent A → Agent B → HUMAN STOP
    /// </summary>
    private async Task ExecuteRoundRobinDiscussion(WorkflowState workflowState, List<AgentConfiguration> agents, string task, string connectionId)
    {
        const int maxTurns = 4; // A→B→A→B then stop for human review
        var conversationHistory = new List<string>();
        
        // Add initial task
        conversationHistory.Add($"User: {task}");

        // Update workflow state
        workflowState.TransitionTo(WorkflowStage.AgentCollaboration, ParticipantRole.Agent1);
        await hubContext.Clients.Client(connectionId).SendAsync("WorkflowStateUpdate", workflowState);

        // Send start message
        var startMsg = OrchestrationMessage.CreateSystemMessage(
            workflowState,
            $"🤝 Starting {agents[0].Name} and {agents[1].Name} collaboration",
            "round_robin_start"
        );
        await hubContext.Clients.Client(connectionId).SendAsync("ReceiveOrchestrationMessage", startMsg);

        // Execute turns: Agent A → Agent B → Agent A → Agent B
        for (int turn = 0; turn < maxTurns; turn++)
        {
            var currentAgent = agents[turn % 2]; // Alternate between first two agents
            
            logger.LogInformation("Round-robin turn {Turn}: {AgentName} responding", turn + 1, currentAgent.Name);

            // Update workflow status to show current agent
            var currentRole = turn % 2 == 0 ? ParticipantRole.Agent1 : ParticipantRole.Agent2;
            workflowState.TransitionTo(WorkflowStage.AgentCollaboration, currentRole);
            await hubContext.Clients.Client(connectionId).SendAsync("WorkflowStateUpdate", workflowState);

            // Get agent context
            var context = BuildAgentContext(conversationHistory, currentAgent, task);
            
            // Get agent response (with streaming - no manual thinking indicator needed)
            var agentResponse = await GetAgentResponseWithStreaming(currentAgent, context, connectionId, currentRole);
            
            // Add to conversation history
            conversationHistory.Add($"{currentAgent.Name}: {agentResponse}");

            // Delay between agents for better UX
            await Task.Delay(1500);
        }

        // Transition to human review
        await RequestHumanReview(workflowState, conversationHistory, connectionId);
    }

    /// <summary>
    /// Get agent configurations for the specified team
    /// </summary>
    private List<AgentConfiguration> GetTeamAgentConfigurations(string agentTeam)
    {
        return agentTeam.ToLowerInvariant() switch
        {
            "copywriter" => new List<AgentConfiguration>
            {
                new() { 
                    Name = "CopyWriter", 
                    Description = "Creative content creator", 
                    AvatarEmoji = "✍️", 
                    ColorScheme = "agent-copywriter",
                    IsActive = true 
                },
                new() { 
                    Name = "Reviewer", 
                    Description = "Content evaluator", 
                    AvatarEmoji = "📝", 
                    ColorScheme = "agent-reviewer",
                    IsActive = true 
                }
            },
            "research" => new List<AgentConfiguration>
            {
                new() { 
                    Name = "Researcher", 
                    Description = "Information gatherer", 
                    AvatarEmoji = "🔍", 
                    ColorScheme = "agent-researcher",
                    IsActive = true 
                },
                new() { 
                    Name = "FactChecker", 
                    Description = "Accuracy verifier", 
                    AvatarEmoji = "✅", 
                    ColorScheme = "agent-factchecker",
                    IsActive = true 
                }
            },
            "technical" => new List<AgentConfiguration>
            {
                new() { 
                    Name = "Architect", 
                    Description = "System designer", 
                    AvatarEmoji = "🏗️", 
                    ColorScheme = "agent-architect",
                    IsActive = true 
                },
                new() { 
                    Name = "CodeReviewer", 
                    Description = "Code quality checker", 
                    AvatarEmoji = "🔍", 
                    ColorScheme = "agent-code-reviewer",
                    IsActive = true 
                }
            },
            _ => new List<AgentConfiguration>
            {
                new() { 
                    Name = "Creative", 
                    Description = "Creative thinking", 
                    AvatarEmoji = "🎨", 
                    ColorScheme = "agent-creative",
                    IsActive = true 
                },
                new() { 
                    Name = "Analytical", 
                    Description = "Logical analysis", 
                    AvatarEmoji = "📊", 
                    ColorScheme = "agent-analytical",
                    IsActive = true 
                }
            }
        };
    }

    /// <summary>
    /// Build context for agent response
    /// </summary>
    private string BuildAgentContext(List<string> conversationHistory, AgentConfiguration agent, string originalTask)
    {
        var context = $"Original task: {originalTask}\n\n";
        context += "Conversation so far:\n";
        context += string.Join("\n", conversationHistory.TakeLast(6)); // Last 6 messages for context
        context += $"\n\nYou are {agent.Name} ({agent.Description}). Provide a helpful response to move the task forward.";
        return context;
    }

    /// <summary>
    /// Get response from an agent using real AI with streaming (copying MultiAgent pattern)
    /// </summary>
    private async Task<string> GetAgentResponseWithStreaming(AgentConfiguration agent, string context, string connectionId, ParticipantRole agentRole)
    {
        try
        {
            // Create agent message placeholder for streaming (like MultiAgent)
            var agentMessageId = Guid.NewGuid().ToString();
            var agentMessage = new AgentMessage
            {
                Id = agentMessageId,
                Content = "",
                AgentName = agent.Name,
                MessageType = "agent",
                IsStreaming = true,
                IsComplete = false
            };

            // Convert to orchestration message for streaming
            var streamingMsg = OrchestrationMessage.FromAgentMessage(
                agentMessage,
                activeWorkflows[connectionId],
                agentRole
            );

            // Send initial streaming message placeholder
            await hubContext.Clients.Client(connectionId).SendAsync("ReceiveOrchestrationMessage", streamingMsg);

            // Create a chat completion service
            var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
            
            // Build agent-specific prompt
            var agentPrompt = BuildAgentPrompt(agent, context);
            
            // Create chat history with the agent prompt
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(agentPrompt);
            chatHistory.AddUserMessage(context);

            // Stream the agent response (like MultiAgent)
            var fullContent = "";
            await foreach (var chunk in chatCompletion.GetStreamingChatMessageContentsAsync(chatHistory, kernel: kernel))
            {
                var content = chunk.Content ?? "";
                if (!string.IsNullOrEmpty(content))
                {
                    fullContent += content;

                    // Send streaming chunk (using AgentStreamingMessage like MultiAgent)
                    var streamingUpdate = new AgentStreamingMessage
                    {
                        MessageId = agentMessageId,
                        Content = content,
                        IsComplete = false
                    };

                    await hubContext.Clients.Client(connectionId).SendAsync("ReceiveAgentStreamingChunk", streamingUpdate);
                }
            }

            // Send completion signal (like MultiAgent)
            var completionUpdate = new AgentStreamingMessage
            {
                MessageId = agentMessageId,
                Content = "",
                IsComplete = true
            };

            await hubContext.Clients.Client(connectionId).SendAsync("ReceiveAgentStreamingChunk", completionUpdate);

            logger.LogInformation("Agent {AgentName} completed streaming response", agent.Name);
            
            return fullContent;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting streaming agent response for {AgentName}", agent.Name);
            
            // Fallback to simple response on error
            var fallbackResponse = $"I'm analyzing this from the {agent.Description.ToLower()} perspective...";
            
            var errorMessage = new AgentMessage
            {
                Content = fallbackResponse,
                AgentName = agent.Name,
                MessageType = "agent",
                IsComplete = true
            };
            
            var errorMsg = OrchestrationMessage.FromAgentMessage(
                errorMessage,
                activeWorkflows[connectionId],
                agentRole
            );
            
            await hubContext.Clients.Client(connectionId).SendAsync("ReceiveOrchestrationMessage", errorMsg);
            
            return fallbackResponse;
        }
    }

    /// <summary>
    /// Send typing indicator for agent
    /// </summary>
    private async Task SendTypingIndicator(AgentConfiguration agent, string connectionId, bool isTyping)
    {
        if (!activeWorkflows.TryGetValue(connectionId, out var workflowState))
            return;

        if (isTyping)
        {
            // Send "thinking" message
            var thinkingMsg = OrchestrationMessage.CreateSystemMessage(
                workflowState,
                $"💭 {agent.Name} is thinking...",
                "agent_thinking"
            );
            await hubContext.Clients.Client(connectionId).SendAsync("ReceiveOrchestrationMessage", thinkingMsg);
        }
        else
        {
            // Could send a completion indicator if needed
            // For now, the actual response will replace the thinking indicator
        }
    }

    /// <summary>
    /// Build agent-specific prompt based on role and team
    /// </summary>
    private string BuildAgentPrompt(AgentConfiguration agent, string context)
    {
        var basePrompt = $"You are {agent.Name}, a {agent.Description}.";
        
        return agent.Name.ToLowerInvariant() switch
        {
            "copywriter" => $"{basePrompt} You excel at creating compelling, creative content that resonates with audiences. Focus on emotional appeal, memorable phrases, and persuasive language.",
            "reviewer" => $"{basePrompt} You excel at evaluating content for clarity, impact, and effectiveness. Provide constructive feedback and suggest improvements.",
            "researcher" => $"{basePrompt} You excel at gathering accurate information and providing well-researched insights. Focus on facts, data, and reliable sources.",
            "factchecker" => $"{basePrompt} You excel at verifying information accuracy and identifying potential issues. Focus on truth, credibility, and fact validation.",
            "architect" => $"{basePrompt} You excel at designing robust, scalable systems and solutions. Focus on structure, best practices, and technical excellence.",
            "codereviewer" => $"{basePrompt} You excel at evaluating code quality, security, and maintainability. Focus on best practices, potential issues, and improvements.",
            "creative" => $"{basePrompt} You excel at innovative, out-of-the-box thinking. Focus on creativity, originality, and unique perspectives.",
            "analytical" => $"{basePrompt} You excel at logical analysis and structured problem-solving. Focus on data, logic, and systematic approaches.",
            _ => $"{basePrompt} Provide helpful insights based on your expertise."
        };
    }

    /// <summary>
    /// Request human review after agent discussion
    /// </summary>
    private async Task RequestHumanReview(WorkflowState workflowState, List<string> conversationHistory, string connectionId)
    {
        // Transition to human review stage
        workflowState.TransitionTo(WorkflowStage.HumanReview, ParticipantRole.HumanReviewer);
        await hubContext.Clients.Client(connectionId).SendAsync("WorkflowStateUpdate", workflowState);

        // Generate a unique request ID
        var requestId = Guid.NewGuid().ToString("N")[..8];

        // Create human input request message using the proper method
        var conversationSummary = string.Join("\n", conversationHistory.TakeLast(4));
        var promptMessage = $"🛑 Agent discussion complete!\n\nConversation Summary:\n{conversationSummary}\n\nPlease review and decide next steps:";

        var humanInputMsg = OrchestrationMessage.CreateHumanInputRequest(
            workflowState,
            requestId,
            promptMessage
        );

        await hubContext.Clients.Client(connectionId).SendAsync("ReceiveOrchestrationMessage", humanInputMsg);

        // Store the pending human input for later processing
        pendingHumanInputs[requestId] = new TaskCompletionSource<HumanReviewDecision>();
    }

    /// <summary>
    /// Send orchestration error message
    /// </summary>
    private async Task SendOrchestrationError(string connectionId, string error)
    {
        if (activeWorkflows.TryGetValue(connectionId, out var workflowState))
        {
            var errorMsg = OrchestrationMessage.CreateSystemMessage(
                workflowState,
                $"❌ {error}",
                "error"
            );
            await hubContext.Clients.Client(connectionId).SendAsync("ReceiveOrchestrationMessage", errorMsg);
        }
    }
}
