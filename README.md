# OpenInference.LLM.Telemetry

[![NuGet](https://img.shields.io/nuget/v/OpenInference.LLM.Telemetry.svg)](https://www.nuget.org/packages/OpenInference.LLM.Telemetry/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A comprehensive OpenTelemetry-based library for tracking and monitoring Large Language Model (LLM) operations in .NET applications according to the [OpenInference](https://github.com/Arize-ai/openinference) semantic convention standard.

## Features

- ✅ OpenInference compliant LLM telemetry instrumentation
- ✅ Seamless integration with OpenTelemetry
- ✅ Support for Azure OpenAI and OpenAI APIs
- ✅ Multiple integration patterns:
  - HTTP client handlers
  - Direct instrumentation
  - Dependency Injection
  - Convenience methods for popular LLM operations
- ✅ Compatible with:
  - Azure Functions
  - ASP.NET Core
  - Any .NET application using OpenTelemetry

## Architecture Overview

The library consists of several components that work together to provide comprehensive LLM telemetry:

```
┌─────────────────────────────────────────────────────────────────┐
│                    Your .NET Application                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌───────────────────┐       ┌───────────────────────────────┐  │
│  │                   │       │                               │  │
│  │  LLM API Client   │◀─────▶│  LLMTelemetryHandler          │  │
│  │                   │       │  (Automatic HTTP Telemetry)   │  │
│  └───────────────────┘       └───────────────────────────────┘  │
│            │                               │                    │
│            ▼                               │                    │
│  ┌───────────────────┐                     │                    │
│  │                   │                     │                    │
│  │  Your LLM Logic   │                     ▼                    │
│  │                   │        ┌───────────────────────────────┐ │
│  └───────────────────┘        │                               │ │
│            │                  │  LLMTelemetry                 │ │
│            │                  │  (Core Telemetry Methods)     │ │
│            ▼                  │                               │ │
│  ┌───────────────────┐        └───────────────────────────────┘ │
│  │  Manual           │                     │                    │
│  │  Instrumentation  │─────────────────────┘                    │
│  │  (Optional)       │                                          │
│  └───────────────────┘                                          │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│                      OpenTelemetry                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌───────────────────┐        ┌───────────────────────────────┐ │
│  │  OpenTelemetry    │        │                               │ │
│  │  TracerProvider   │◀───────│  LlmInstrumentation           │ │
│  │                   │        │                               │ │
│  └───────────────────┘        └───────────────────────────────┘ │
│            │                                                    │
│            ▼                                                    │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                                                           │  │
│  │  OpenTelemetry Exporters (Console, OTLP, Azure Monitor)   │  │
│  │                                                           │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Components:

1. **LLMTelemetryHandler**: Intercepts HTTP requests to LLM APIs and automatically adds telemetry.
2. **LLMTelemetry**: Core class with methods for creating and managing telemetry activities.
3. **LlmInstrumentation**: Integration with OpenTelemetry for standardized tracing.
4. **Manual Instrumentation**: Optional direct instrumentation for custom scenarios.

This architecture allows for flexible integration options while ensuring consistent telemetry data.

## Installation

```bash
dotnet add package OpenInference.LLM.Telemetry
```

## Quick Start

### Basic Setup

Add OpenInference LLM telemetry to your application:

```csharp
// Program.cs or Startup.cs

// Register with dependency injection
builder.Services.AddOpenInferenceLlmTelemetry(options => {
    options.EmitTextContent = true;       // Include prompt & response text
    options.MaxTextLength = 1000;         // Truncate long text to limit storage
    options.SanitizeSensitiveInfo = true; // Attempt to redact sensitive info
});

// Configure OpenTelemetry
builder.Services.AddOpenTelemetryWithLlmTracing(
    serviceName: "MyService",
    builder => builder
        .AddConsoleExporter()
        .AddOtlpExporter());
```

### Integration Methods

#### HttpClient Integration

```csharp
// Add LLM telemetry to your HttpClient
services.AddHttpClient("AzureOpenAI", client => {
    client.BaseAddress = new Uri("https://your-resource.openai.azure.com/");
    client.DefaultRequestHeaders.Add("api-key", "your-api-key");
})
.AddLLMTelemetry(
    defaultModelName: "gpt-4", 
    provider: "azure");
```

#### Direct Instrumentation

```csharp
// Use the core LLMTelemetry class directly
using var activity = LLMTelemetry.StartLLMActivity(
    modelName: "gpt-4",
    prompt: "Explain quantum computing",
    taskType: "chat",
    provider: "openai");

try
{
    // Perform your LLM operation
    var response = await client.GetChatCompletionsAsync(options);
    
    LLMTelemetry.EndLLMActivity(
        activity: activity,
        response: response.Value.Choices[0].Message.Content,
        isSuccess: true,
        latencyMs: stopwatch.ElapsedMilliseconds);
}
catch (Exception ex)
{
    LLMTelemetry.RecordException(activity, ex);
    throw;
}
```

#### Azure OpenAI Integration

```csharp
// Track chat completions from Azure OpenAI
var stopwatch = Stopwatch.StartNew();
var result = await client.GetChatCompletionsAsync(options);
stopwatch.Stop();

AzureOpenAIAdapter.TrackChatCompletion(
    options: options,
    result: result.Value,
    modelName: "gpt-4",
    latencyMs: stopwatch.ElapsedMilliseconds);
```

### Configuration Options

```csharp
services.AddOpenInferenceLlmTelemetry(options => {
    // Whether to include prompt and response text in telemetry
    options.EmitTextContent = true;
    
    // Maximum length of text to include (truncated beyond this)
    options.MaxTextLength = 1000;
    
    // Whether to emit latency metrics 
    options.EmitLatencyMetrics = true;
    
    // Whether to include model name information
    options.RecordModelName = true;
    
    // Whether to record token usage information
    options.RecordTokenUsage = true;
    
    // Whether to attempt to sanitize sensitive information in prompts
    options.SanitizeSensitiveInfo = false;
});
```

## Advanced Usage

For more advanced usage scenarios and examples, see [USAGE.md](USAGE.md).

## OpenInference Semantic Conventions

This library follows the [OpenInference semantic conventions](https://openinference.io/) for LLM telemetry. Key attributes captured include:

- `llm.request`: The prompt text
- `llm.response`: The model's response
- `llm.request_type`: The type of request (chat, completion, etc)
- `llm.model`: The model name
- `llm.model_provider`: The provider name (azure, openai)
- `llm.success`: Whether the operation succeeded
- `llm.latency_ms`: Operation latency in milliseconds
- `llm.token_count.prompt`: Number of tokens in prompt
- `llm.token_count.completion`: Number of tokens in completion
- `llm.token_count.total`: Total token count

## Semantic Kernel Integration
InProgress


## License

MIT