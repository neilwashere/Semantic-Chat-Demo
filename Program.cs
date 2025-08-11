using SemanticChatDemo.Components;
using SemanticChatDemo.Features.Shared.Models;
using SemanticChatDemo.Features.Chat.Hubs;
using SemanticChatDemo.Features.MultiAgent.Hubs;
using SemanticChatDemo.Features.Shared.Plugins;
using Microsoft.SemanticKernel;
using SemanticChatDemo.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add SignalR
builder.Services.AddSignalR();

// Configure OpenAI settings
builder.Services.Configure<OpenAIConfig>(
    builder.Configuration.GetSection("OpenAI"));

// Add Semantic Kernel with OpenAI and plugins
var kernelBuilder = builder.Services.AddKernel();

kernelBuilder.AddOpenAIChatCompletion(
    modelId: builder.Configuration["OpenAI:ModelId"] ?? "gpt-4o-mini",
    apiKey: builder.Configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key is required"));

// Add our weather facts plugin
kernelBuilder.Plugins.AddFromType<WeatherFactsPlugin>();

// Add Feature services
builder.Services.AddChatFeature();
builder.Services.AddMultiAgentFeature();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Only use HTTPS redirection in production
// In development, we'll allow both HTTP and HTTPS


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map SignalR hubs
app.MapHub<ChatHub>("/chathub");
app.MapHub<MultiAgentHub>("/multiagenthub");

app.Run();
