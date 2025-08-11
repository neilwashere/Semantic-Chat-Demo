# Semantic Chat Demo

This project is a learning demonstration for the **Semantic Kernel SDK**. It's a simple Blazor Server application that implements a real-time chat interface using SignalR, allowing you to interact with an AI model powered by Semantic Kernel.

The primary goal of this project is to provide a hands-on, step-by-step guide to understanding and implementing various Semantic Kernel concepts, from basic setup to advanced AI orchestration.

## Project Purpose

- **Learn Semantic Kernel**: Progressively learn SK concepts by building a functional application.
- **Demonstrate Core Features**: Showcase features like streaming, function calling, and conversation management.
- **Provide a Reference**: Serve as a practical reference for building AI-powered applications with .NET.

## How to Configure and Run

### 1. Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- An OpenAI API key

### 2. Configuration

The application uses .NET's user secrets to manage the OpenAI API key.

1.  **Initialize User Secrets**:
    Open a terminal in the project root and run:
    ```bash
    dotnet user-secrets init
    ```

2.  **Set the API Key**:
    Run the following command, replacing `"your-api-key-here"` with your actual OpenAI API key:
    ```bash
    dotnet user-secrets set "OpenAI:ApiKey" "your-api-key-here"
    ```

3.  **(Optional) Set the Model ID**:
    The default model is `gpt-4o-mini`. If you want to use a different model, you can set it with:
    ```bash
    dotnet user-secrets set "OpenAI:ModelId" "your-model-id"
    ```

### 3. Running the Application

1.  **Build the Project**:
    ```bash
    dotnet build SemanticChatDemo.csproj
    ```

2.  **Run the Application**:
    ```bash
    dotnet run --project SemanticChatDemo.csproj
    ```

The application will be available at `http://localhost:5184` (or another port specified in the terminal output).

## Learning Roadmap

This project follows a phased learning roadmap. You can track the progress and see what's next by reviewing the [ROADMAP.md](./docs/ROADMAP.md) file.
