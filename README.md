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

## FAQ: Why Use OpenInference.LLM.Telemetry?

This section addresses common questions about the value and purpose of this SDK.

### Q1: Why does this SDK exist alongside OpenTelemetry GenAI conventions?
**A:** This SDK complements OpenTelemetry (OTel) GenAI conventions by providing a more specialized and richer set of telemetry attributes tailored for deep LLM observability. While OTel GenAI offers a foundational standard, OpenInference.LLM.Telemetry focuses on LLM-specific details (e.g., finer-grained token counts, cost attributes (planned), PII redaction controls), enabling more granular insights and control. It offers an incremental adoption path for teams evolving their observability maturity and requiring .NET-idiomatic solutions.

### Q2: What makes this SDK particularly valuable for .NET developers?
**A:** It provides a truly native .NET experience with:
*   **Strong Typing:** Leveraging .NET's type system for configuration and data models.
*   **Dependency Injection:** Seamless integration with .NET's dependency injection (DI) patterns.
*   **Idiomatic Patterns:** Intuitive abstractions like `LlmInstrumentationOptions` and `Activity`-based tracing that align with modern .NET development practices.
This makes implementation straightforward and enriches the .NET ecosystem for AI development.

### Q3: How does this SDK enhance semantic conventions beyond OTel GenAI?
**A:** The SDK implements OpenInference conventions, which often include richer and more fine-grained attributes. Examples include:
*   **Detailed Token Counts:** Explicit attributes for `llm.token_count.prompt`, `llm.token_count.completion`, and `llm.token_count.total`.
*   **Fine-grained Text Control:** Options like `EmitTextContent` and `MaxTextLength` for precise control over telemetry payload.
*   **Privacy Features:** Built-in `SanitizeSensitiveInfo` for PII redaction.
*   **Cost Tracking:** Planned attributes like `llm.usage.cost`.
*   Potentially more detailed attributes for retries, streaming events, and model-specific parameters as the OpenInference standard evolves.

### Q4: Is this SDK compatible with orchestration frameworks like LangChain or Semantic Kernel?
**A:** Yes. The SDK is designed to be orchestration framework agnostic.
*   **Semantic Kernel:** It includes dedicated adapters like `SemanticKernelAdapter` for seamless integration.
*   **LangChain.NET & Custom Frameworks:** It provides standardized telemetry for teams using LangChain.NET, custom-built agentic systems, or direct SDK interactions with LLM providers. The `GenericLlmAdapter` and direct `LLMTelemetry` class usage support these scenarios.
This ensures consistent observability regardless of your chosen orchestration tools. The `samples` directory includes examples for Semantic Kernel and custom framework usage.

### Q5: Can this SDK be used in non-Azure or multi-cloud scenarios?
**A:** Absolutely. While it offers first-class support and specific adapters for Azure AI services (e.g., `AzureOpenAIAdapter`), its core design and the underlying adapter pattern allow for pluggable LLM backends. The telemetry generated is based on the OpenInference standard, promoting interoperability across different LLM providers and cloud environments.

### Q6: How does this SDK handle privacy and sensitive data in telemetry?
**A:** The SDK includes a built-in option for PII (Personally Identifiable Information) redaction through the `SanitizeSensitiveInfo` property in `LlmInstrumentationOptions`. When enabled, it attempts to detect and redact common PII patterns (like emails, credit card numbers, SSNs) from telemetry data, helping to protect sensitive information before it's exported.

### Q7: Does this SDK support Azure-specific LLM services better than generic telemetry?
**A:** Yes, for Azure OpenAI, it provides specialized integration through the `AzureOpenAIAdapter`. This adapter is designed to understand the specific nuances of Azure OpenAI responses and can extract richer, more accurate telemetry (e.g., specific model deployment IDs, detailed usage information from Azure's response structure) compared to a generic approach that might not be tailored to Azure's specific metadata.

### Q8: What are the benefits for teams migrating from custom telemetry implementations?
**A:** Migrating to this SDK offers several advantages:
*   **Standardization:** Adopts the OpenInference standard, aligning with a broader community and enabling the use of standard observability tools.
*   **Reduced Maintenance:** Leverages a maintained library, reducing the burden of developing and managing custom telemetry code.
*   **Richer Features:** Gains access to features like PII redaction, fine-grained telemetry controls, and framework adapters out-of-the-box.
*   **Interoperability:** Improves interoperability with other systems and tools that support OpenTelemetry and OpenInference.
*   **Future-Proofing:** Stays current with evolving LLM observability best practices through library updates.

### Q9: How does this SDK help with LLM usage cost tracking and optimization?
**A:** The SDK captures detailed token usage (prompt, completion, and total tokens) for each LLM operation when `RecordTokenUsage` is enabled. This granular data is crucial for:
*   **Accurate Cost Tracking:** Correlating token consumption with LLM API costs.
*   **Optimization:** Identifying high-cost operations, optimizing prompts, and selecting appropriate models to manage and reduce LLM expenses.
Future enhancements aim to include direct cost attributes (e.g., `llm.usage.cost`) based on token counts and model pricing.

### Q10: Is this SDK suitable for production-level AI observability needs?
**A:** Yes. The SDK is designed with production use cases in mind, offering:
*   **PII Redaction:** To protect sensitive data.
*   **Configurable Verbosity:** To manage telemetry volume and cost.
*   **Robust Error Handling:** To ensure telemetry capture doesn't disrupt application flow.
*   **OpenTelemetry Alignment:** Ensures that the collected data can be exported to production-grade monitoring and observability platforms like Azure Application Insights, Grafana (via Prometheus/Loki), Datadog, New Relic, etc., for comprehensive AI observability.

### Q11: What specific telemetry features does OpenInference.LLM.Telemetry provide? (Summary)
**A:** Key features include:
*   **Rich Content Analysis:** Full prompt and response tracking with configurable limits and PII redaction.
*   **Performance Metrics:** Detailed latency tracking for optimizing LLM interactions.
*   **Cost Management Data:** Granular token usage tracking (prompt, completion, total).
*   **Error Handling:** Structured error capturing specific to LLM interactions.
*   **Multi-provider Support:** Adapters for consistent telemetry across different LLM providers (initially Azure OpenAI, OpenAI, Semantic Kernel, with an extensible design).
*   **Workflow Tracking:** Support for tracking complex multi-step LLM chains and agent workflows via `GenericLlmAdapter.TrackLlmChain`.

### Q12: How does this SDK benefit the broader .NET ecosystem?
**A:** It strengthens the .NET AI/ML landscape by:
*   **Promoting Standardization:** Encouraging the adoption of OpenInference and OpenTelemetry for LLM observability.
*   **Enhancing Interoperability:** Facilitating consistent telemetry practices across different .NET projects, frameworks, and even with Python-based systems using OpenInference.
*   **Lowering Adoption Barriers:** Providing a ready-to-use, .NET-idiomatic library for robust LLM telemetry.

### Q13: What future development is planned for OpenInference.LLM.Telemetry?
**A:** We're actively working on enhancing the library, with plans for:
*   **Broader Provider Support:** Adapters for additional LLM providers (e.g., Anthropic, Cohere, local models via Ollama).
*   **Advanced Cost Tracking:** Direct financial cost attributes based on token usage and model pricing.
*   **Expanded Parameter Tracking:** More comprehensive capture of model configuration parameters (e.g., temperature, top_p, stop sequences).
*   **Enhanced Visualization Examples:** Sample dashboards or configurations for popular monitoring tools.
*   **Community-Driven Extensions:** Supporting contributions for specialized LLM use cases and emerging OpenInference attributes.

## Attribute Comparison: OpenInference vs. OTel GenAI (Illustrative)

This table provides an illustrative comparison of attribute areas. It will be updated as both standards evolve and as this SDK implements more specific OpenInference attributes.

| Feature/Attribute Area        | OpenInference.LLM.Telemetry (Specific Examples/Focus) | OTel GenAI (General Coverage) | Notes                                                                 |
| ----------------------------- | ----------------------------------------------------- | ----------------------------- | --------------------------------------------------------------------- |
| **Request/Response Text**     | `llm.request`, `llm.response`; explicit control via `EmitTextContent`, `MaxTextLength`, `SanitizeSensitiveInfo` | Basic request/response attributes | OpenInference (via this SDK) offers more explicit control over verbosity, length, and PII. |
| **Token Counts**              | `llm.token_count.prompt`, `llm.token_count.completion`, `llm.token_count.total` (when `RecordTokenUsage` is true) | May include token counts        | OpenInference standardizes these crucial cost/usage metrics; this SDK makes them easily configurable. |
| **Model Identification**      | `llm.model`, `llm.model_provider`                     | Similar attributes            | Consistent naming.                                                    |
| **Operation Details**         | `llm.request_type`, `llm.latency_ms`, `llm.success`, `llm.error.message` | Similar attributes            | Core operational metrics are covered by both.                         |
| **Model Parameters**          | (Planned: `llm.temperature`, `llm.top_p`, etc.)       | May include some parameters   | OpenInference aims for more comprehensive model parameter tracking.   |
| **Cost Tracking**             | (Planned: `llm.usage.cost`)                           | Generally not specified       | A key planned differentiator for OpenInference.                       |
| **PII Handling**              | `SanitizeSensitiveInfo` option for redaction          | Not explicitly defined        | Built-in, configurable feature in this SDK.                           |
| **Azure Specifics**           | `AzureOpenAIAdapter` for detailed Azure metadata      | Generic provider attributes   | Deeper integration with Azure services via dedicated adapters.        |
| **Streaming/Retries**         | (Planned: attributes for streaming events, retries)   | Basic operation status        | Future enhancements for more detailed operational insights.           |
| **Workflow/Chain Tracking**   | `GenericLlmAdapter.TrackLlmChain` for multi-step ops  | Less explicit focus           | This SDK provides helpers for tracking complex LLM workflows.         |

*Disclaimer: This table is for illustrative purposes. Refer to the official OpenInference and OpenTelemetry GenAI specifications for the most current and complete attribute lists.*

## License

MIT