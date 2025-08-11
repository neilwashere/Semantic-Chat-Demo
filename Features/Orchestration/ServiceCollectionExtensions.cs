using SemanticChatDemo.Features.Orchestration.Hubs;
using SemanticChatDemo.Features.Orchestration.Services;

namespace SemanticChatDemo.Features.Orchestration;

/// <summary>
/// Extension methods for registering Orchestration feature services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Human-in-the-Loop Orchestration feature services
    /// </summary>
    public static IServiceCollection AddOrchestrationFeature(this IServiceCollection services)
    {
        // Register orchestration services
        services.AddScoped<OrchestrationService>();

        // SignalR hub will be registered automatically when mapped in Program.cs

        return services;
    }
}
