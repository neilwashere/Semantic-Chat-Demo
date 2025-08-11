using semantic_chat_demo.Components;
using semantic_chat_demo.Models;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add SignalR
builder.Services.AddSignalR();

// Configure OpenAI settings
builder.Services.Configure<OpenAIConfig>(
    builder.Configuration.GetSection("OpenAI"));

// Add Semantic Kernel with OpenAI
builder.Services.AddKernel()
    .AddOpenAIChatCompletion(
        modelId: builder.Configuration["OpenAI:ModelId"] ?? "gpt-4o-mini",
        apiKey: builder.Configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key is required"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map SignalR hubs (will be added when we create ChatHub)
// app.MapHub<ChatHub>("/chathub");

app.Run();
