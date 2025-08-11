using SemanticChatDemo.Features.Chat.Services;
using SemanticChatDemo.Features.MultiAgent.Services;
using SemanticChatDemo.Features.Orchestration.Services;

namespace SemanticChatDemo.Features;

/// <summary>
/// Extension methods for registering feature-specific services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all Chat feature services
    /// </summary>
    public static IServiceCollection AddChatFeature(this IServiceCollection services)
    {
        services.AddScoped<ChatService>();
        services.AddScoped<ConversationPersistenceService>();
        return services;
    }

    /// <summary>
    /// Register all MultiAgent feature services
    /// </summary>
    public static IServiceCollection AddMultiAgentFeature(this IServiceCollection services)
    {
        services.AddScoped<AgentService>();
        return services;
    }

    /// <summary>
    /// Register all Orchestration feature services
    /// </summary>
    public static IServiceCollection AddOrchestrationFeature(this IServiceCollection services)
    {
        services.AddScoped<OrchestrationService>();
        return services;
    }
}
