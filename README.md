# OpenInference.LLM.Telemetry

[![NuGet](https://img.shields.io/nuget/v/OpenInference.LLM.Telemetry.svg)](https://www.nuget.org/packages/OpenInference.LLM.Telemetry/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A OpenTelemetry-based library for tracking and monitoring Large Language Model (LLM) operations in .NET applications according to the [OpenInference](https://github.com/Arize-ai/openinference) semantic convention standard.

## Introducing OpenInference.LLM.Telemetry SDK for .NET

### Bridging the Gap in LLM Telemetry Standardization

This library addresses critical gaps in the .NET ecosystem for standardized LLM telemetry, implementing the OpenInference semantic conventions to provide comprehensive observability for LLM operations.

### Problem Statement

- **Lack of .NET Support:** OpenInference SDKs exist for Python and TypeScript, but no official .NET support exists.
- **Limited Semantic Conventions:** OpenTelemetry's current AI semantic conventions (`gen_ai.*`) focus on general attributes (operation names, statuses) but lack detailed, domain-specific telemetry for LLMs.
- **Inconsistent Telemetry Across Platforms:** Diverse tools (Azure AI, AWS Bedrock, LlamaIndex, etc.) lack unified telemetry standards, especially in .NET.
- **PII and Data Privacy Concerns:** Sensitive data (emails, phone numbers, API keys) often appear in telemetry, requiring robust sanitization mechanisms.

### Why OpenInference?

#### Open Source Standard
OpenInference (by Arize AI) provides detailed, domain-specific semantic conventions for LLM telemetry.

#### Rich Telemetry Attributes
- Structured prompts and responses
- Token usage and cost tracking
- Embedding details
- Tool and function call metadata
- Retrieval and document metadata
- Prompt template tracking

#### Industry Adoption
Widely adopted in Python/TypeScript ecosystems (LangChain, LlamaIndex, AWS Bedrock, etc.)

### OpenInference.LLM.Telemetry SDK for .NET

- **Comprehensive Semantic Conventions:** Implements OpenInference semantic conventions in .NET, providing detailed telemetry attributes.
- **Easy Integration:** Simple, intuitive APIs for .NET developers to instrument LLM operations.
- **Flexible Instrumentation:** Supports Azure OpenAI, OpenAI direct, and generic LLM providers.
- **Built-in PII Handling:** Automatic sanitization of sensitive data (emails, phone numbers, API keys, etc.).

### Benefits to Industry or .NET Ecosystem

- **Standardized Telemetry:** Establish consistent, standardized telemetry across .NET applications, Python-based solutions, and ML.NET pipeline workflows, enabling unified observability and streamlined collaboration across all teams.
- **Enhanced Observability:** Detailed insights into LLM performance, usage, and costs.
- **Cross-Platform Compatibility:** Aligns with OpenInference standards used across industry-leading tools.
- **Improved Security:** Built-in PII sanitization ensures compliance and reduces risk.

## Features

- ✅ OpenInference compliant LLM telemetry instrumentation
- ✅ Seamless integration with OpenTelemetry
- ✅ Generic LLM adapter and Semantic Kernel integration
- ✅ Multiple integration patterns:
  - HTTP client handlers
  - Direct instrumentation
  - Dependency Injection
  - Convenience methods for popular LLM operations
- ✅ Compatible with:
  - Azure Functions
  - ASP.NET Core
  - Any .NET application using OpenTelemetry

### Updated Features
- **Cost Tracking**: Added attributes for `llm.usage.cost` and `llm.usage.currency` to track LLM operation costs.
- **PII Sanitization**: Extended sanitization rules to include credit card numbers and SSNs.
- **Structured Logging**: Integrated `ILogger` for consistent structured logging across all adapters.
- **Sample Observability Configurations**: Added examples for Grafana dashboards and Azure Monitor integration.

## Architecture Overview

The library consists of several components that work together to provide comprehensive LLM telemetry.

```
┌─────────────────────────────────────────────────────────────────┐
│                    Your .NET Application                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌───────────────────┐       ┌───────────────────────────────┐  │
│  │                   │       │                               │  │
│  │  LLM API Client   │◀─────▶│  LLMTelemetryHandler          │  │
│  │                   │       │  (Async HTTP Telemetry)       │  │
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

1. **LLMTelemetryHandler**: Intercepts HTTP requests to LLM APIs and automatically adds telemetry with proper async/await patterns for all network operations.
2. **LLMTelemetry**: Core static class with fully documented methods following C# standards, using underscore (_) prefixed private fields and proper error handling.
3. **LlmInstrumentation**: Integration with OpenTelemetry for standardized tracing with full XML documentation and async operation support.
4. **Manual Instrumentation**: Optional direct instrumentation for custom scenarios with examples following best C# coding practices.

This architecture allows for flexible integration options while ensuring consistent telemetry data and adherence to C# coding standards.

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
    options.SanitizeSensitiveInfo = true; // Attempt to redact sensitive info (PII)
    options.RecordTokenUsage = true;      // Capture token counts
});

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracingProviderBuilder =>
        tracingProviderBuilder
            .AddSource(LLMTelemetry.ActivitySourceName) // Source from this SDK
            // Add other sources from your application as needed
            // .AddSource("MyApplicationActivitySource") 
            .ConfigureResource(resource => resource.AddService("MyLlmService"))
            .AddConsoleExporter() // For local debugging
            .AddOtlpExporter(otlpOptions => // For systems like Prometheus/Grafana, Jaeger, etc.
            {
                otlpOptions.Endpoint = new Uri("http://localhost:4317");
            })
            .AddAzureMonitorTraceExporter(o => // For Azure Application Insights
            {
                o.ConnectionString = "YOUR_APPLICATION_INSIGHTS_CONNECTION_STRING";
            }));
```
Note: Replace `"YOUR_APPLICATION_INSIGHTS_CONNECTION_STRING"` with your actual Application Insights connection string. Telemetry exported via OTLP can be ingested by systems like Prometheus (then visualized in Grafana) or other OpenTelemetry-compatible backends.

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

#### ML.NET Pipelines Integration

```csharp
// Example integration with ML.NET pipeline
var pipeline = new LearningPipeline();
pipeline.Add(new TextLoader("data.csv"));
pipeline.Add(new LlmTelemetryTransform());
pipeline.Add(new StochasticDualCoordinateAscentClassifier());
pipeline.Add(new PredictedLabelColumnOriginalValueConverter());
var model = pipeline.Train<SentimentData, SentimentPrediction>();
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

This library follows the [OpenInference semantic conventions](https://github.com/Arize-ai/openinference/blob/main/spec/semantic_conventions.md) for LLM telemetry. Key attributes captured include:

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

### Additional OpenInference Semantic Conventions

OpenInference defines an extensive set of semantic conventions that this library supports:

#### Core LLM Attributes
- `llm.provider`: The hosting provider of the LLM (e.g., azure, openai)
- `llm.system`: The AI product as identified by instrumentation (e.g., anthropic, openai)
- `llm.model_name`: The name of the language model being utilized
- `llm.function_call`: Object recording details of a function call
- `llm.invocation_parameters`: Parameters used during LLM invocation
- `llm.input_messages`: List of messages sent to the LLM in a chat request
- `llm.output_messages`: List of messages received from the LLM

#### Messages and Content
- `message.role`: Role of the entity in a message (e.g., user, system)
- `message.content`: The content of a message in a chat
- `message.contents`: The message contents to the LLM as array of content objects
- `message.function_call_arguments_json`: The arguments to the function call in JSON
- `message.function_call_name`: Function call function name
- `message.tool_call_id`: Tool call result identifier
- `message.tool_calls`: List of tool calls generated by the LLM
- `messagecontent.type`: The type of content (e.g., text, image)
- `messagecontent.text`: The text content of the message
- `messagecontent.image`: The image content of the message

#### Token Usage Details
- `llm.token_count.completion_details.reasoning`: Token count for model reasoning
- `llm.token_count.completion_details.audio`: Audio tokens generated by the model
- `llm.token_count.prompt_details.cache_read`: Tokens read from previously cached prompts
- `llm.token_count.prompt_details.cache_write`: Tokens written to cache
- `llm.token_count.prompt_details.audio`: Audio input tokens in the prompt

#### Tools and Function Calling
- `llm.tools`: List of tools that are advertised to the LLM
- `tool.name`: The name of the tool being utilized
- `tool.description`: Description of the tool's purpose
- `tool.json_schema`: The JSON schema of a tool input
- `tool.parameters`: The parameters definition for invoking the tool
- `tool.id`: The identifier for the result of the tool call
- `tool_call.function.name`: The name of the function being invoked
- `tool_call.function.arguments`: The arguments for the function invocation
- `tool_call.id`: The ID of a tool call

#### Prompt Template Information
- `llm.prompt_template.template`: Template used to generate prompts
- `llm.prompt_template.variables`: Variables applied to the prompt template
- `llm.prompt_template.version`: The version of the prompt template

#### Retrieval and Documents
- `document.content`: The content of a retrieved document
- `document.id`: Unique identifier for a document
- `document.metadata`: Metadata associated with a document
- `document.score`: Score representing document relevance
- `retrieval.documents`: List of retrieved documents
- `reranker.input_documents`: List of documents input to the reranker
- `reranker.output_documents`: List of documents output by the reranker
- `reranker.model_name`: Model name of the reranker
- `reranker.query`: Query parameter of the reranker
- `reranker.top_k`: Top K parameter of the reranker

#### Embedding Information
- `embedding.embeddings`: List of embedding objects
- `embedding.model_name`: Name of the embedding model used
- `embedding.text`: The text represented in the embedding
- `embedding.vector`: The embedding vector of floats

#### Media and Content Types
- `input.mime_type`: MIME type of the input value
- `input.value`: The input value to an operation
- `output.mime_type`: MIME type of the output value
- `output.value`: The output value of an operation
- `image.url`: Link to the image or base64 encoding
- `audio.url`: URL to an audio file
- `audio.mime_type`: The MIME type of the audio file
- `audio.transcript`: The transcript of the audio file

#### Exception and Error Handling
- `exception.escaped`: Indicator if exception escaped the span's scope
- `exception.message`: Detailed message describing the exception
- `exception.stacktrace`: Stack trace of the exception
- `exception.type`: The type of exception thrown

#### Session and User Information
- `session.id`: Unique identifier for a session
- `user.id`: Unique identifier for a user
- `metadata`: Metadata associated with a span
- `tag.tags`: List of tags to categorize the span
- `openinference.span.kind`: The kind of span (e.g., CHAIN, LLM, RETRIEVER)

## Semantic Kernel Integration

The library supports native integration with Microsoft Semantic Kernel, providing automatic telemetry for AI operations:

```csharp
// Track a Semantic Kernel chat completion operation
var stopwatch = Stopwatch.StartNew();
var chatResult = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
stopwatch.Stop();

SemanticKernelAdapter.TrackChatCompletion(
    chatHistory: chatHistory,
    result: chatResult,
    modelName: "gpt-4",
    latencyMs: stopwatch.ElapsedMilliseconds);
```

Key features of the Semantic Kernel integration:
- Automatic capture of prompt and response content
- Chat history tracking
- Token usage metrics when available
- Easy integration with minimal code changes

For more detailed examples, see the `samples/SemanticKernelSample` project and `samples/CustomFrameworkSample` for non-Semantic Kernel usage.

## OpenTelemetry Compatibility

OpenInference.LLM.Telemetry is built upon OpenTelemetry and is designed to complement, not replace, the OpenTelemetry GenAI semantic conventions.

*   **Working Together:** You can use this SDK to capture rich, LLM-specific telemetry based on OpenInference conventions, while still leveraging the broader OpenTelemetry ecosystem for traces, metrics, and logs from other parts of your application. The activities generated by this SDK integrate seamlessly into your existing OpenTelemetry traces.
*   **Enhanced Data:** Think of OpenInference.LLM.Telemetry as providing a more specialized layer of detail for your LLM interactions. If OTel GenAI provides the general blueprint, OpenInference adds the detailed annotations specific to LLMs.
*   **Transition and Flexibility:**
    *   If you are starting with LLM observability, this SDK provides a comprehensive, .NET-native solution.
    *   If you are already using OTel GenAI, you can introduce this SDK to capture additional OpenInference attributes for deeper insights without disrupting your existing setup. The SDK's activities will simply carry more LLM-specific tags.
    *   As OTel GenAI conventions evolve, this SDK will aim to maintain compatibility and continue to provide value through its .NET-specific features and potentially faster adoption of emerging LLM observability needs.
*   **Exporting Data:** All telemetry captured by this SDK is standard OpenTelemetry data and can be exported using any OpenTelemetry-compatible exporter to backends like Azure Application Insights, Prometheus (for Grafana), Jaeger, Zipkin, OTLP collectors, etc., as shown in the Quick Start example.

## Sample Observability Configurations

#### Grafana Dashboard for LLM Telemetry

1. Use Prometheus as the data source.
2. Create a new dashboard with panels for:
   - LLM Request Count (`llm.requests.count`)
   - LLM Latency (`llm.latency`)
   - Token Usage (`llm.tokens.count`)
   - Cost Tracking (`llm.cost`)

#### Azure Monitor Integration

```csharp
builder.Services.AddOpenTelemetryWithLlmTracing(
    serviceName: "MyService",
    configurator => configurator
        .AddAzureMonitorTraceExporter(options => {
            options.ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
        })
);
```

## License

MIT