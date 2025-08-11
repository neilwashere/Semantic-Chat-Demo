using Microsoft.SemanticKernel;
using SemanticChatDemo.Features.Shared.Models;
using SemanticChatDemo.Features.Shared.Services;

namespace SemanticChatDemo.Features.MultiAgent.Services;

/// <summary>
/// Service for managing multiple AI agents and their conversations
/// Inherits from BaseAgentService for shared functionality
/// </summary>
public class AgentService(Kernel kernel, ILogger<AgentService> logger) : BaseAgentService(kernel, logger)
{
    /// <summary>
    /// Enhance agent instructions with personality reinforcement using AgentTemplates
    /// </summary>
    protected override string EnhanceAgentInstructions(AgentConfiguration config)
    {
        return AgentTemplates.EnhanceAgentInstructions(config);
    }
}
