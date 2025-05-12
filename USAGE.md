# Advanced Usage Guide for OpenInference.LLM.Telemetry

This document provides detailed examples and guidance for more advanced usage scenarios with the OpenInference.LLM.Telemetry library.

## Table of Contents

1. [ASP.NET Core Integration](#aspnet-core-integration)
2. [Azure Functions Integration](#azure-functions-integration)
3. [Custom Telemetry Exporters](#custom-telemetry-exporters)
4. [Common Integration Patterns](#common-integration-patterns)
5. [Handling Token Usage](#handling-token-usage)
6. [Troubleshooting](#troubleshooting)
7. [Semantic Kernel Integration](#semantic-kernel-integration)
8. [Updated Advanced Usage](#updated-advanced-usage)

## ASP.NET Core Integration

Here's a complete example of integrating OpenInference.LLM.Telemetry with an ASP.NET Core application:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add OpenInference LLM Telemetry
builder.Services.AddOpenInferenceLlmTelemetry(options => {
    options.EmitTextContent = true;
    options.MaxTextLength = 2000;
    options.RecordTokenUsage = true;
    options.SanitizeSensitiveInfo = true;
});

// Configure OpenTelemetry with LLM instrumentation
builder.Services.AddOpenTelemetryWithLlmTracing(
    serviceName: "MyLlmWebService",
    configurator => configurator
        .AddConsoleExporter()
        .AddOtlpExporter(opts => {
            opts.Endpoint = new Uri("http://localhost:4317");
        })
        .AddLlmEnrichment() // Add additional contextual data to LLM spans
);

// Add HTTP client with LLM telemetry
builder.Services.AddHttpClient("AzureOpenAI", client => {
    client.BaseAddress = new Uri(builder.Configuration["AzureOpenAI:Endpoint"]);
    client.DefaultRequestHeaders.Add("api-key", builder.Configuration["AzureOpenAI:ApiKey"]);
})
.AddLLMTelemetryWithDI("gpt-4", "azure");

var app = builder.Build();

// Configure middleware
app.UseOpenTelemetry();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

## Azure Functions Integration

Here's how to integrate the OpenInference.LLM.Telemetry library with Azure Functions:

```csharp
// Program.cs
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // Add OpenInference LLM Telemetry
        services.AddOpenInferenceLlmTelemetry();

        // Configure OpenTelemetry with LLM instrumentation
        services.AddOpenTelemetryWithLlmTracing(
            serviceName: "MyLlmFunctionApp",
            configurator => configurator
                .AddAzureMonitorTraceExporter(opts => {
                    opts.ConnectionString = context.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
                })
        );

        // Add HTTP client with LLM telemetry
        services.AddAzureOpenAIClientWithTelemetry(client => {
            client.BaseAddress = new Uri(context.Configuration["AzureOpenAI:Endpoint"]);
            client.DefaultRequestHeaders.Add("api-key", context.Configuration["AzureOpenAI:ApiKey"]);
        }, "gpt-4");
    })
    .Build();

host.Run();
```

## Custom Telemetry Exporters

You can configure the OpenTelemetry pipeline to export your LLM telemetry to various destinations:

### Azure Application Insights

```csharp
builder.Services.AddOpenTelemetryWithLlmTracing(
    serviceName: "MyService",
    configurator => configurator
        .AddAzureMonitorTraceExporter(options => {
            options.ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
        })
);
```

### OTLP (OpenTelemetry Protocol)

```csharp
builder.Services.AddOpenTelemetryWithLlmTracing(
    serviceName: "MyService",
    configurator => configurator
        .AddOtlpExporter(options => {
            options.Endpoint = new Uri("http://localhost:4317");
            options.Protocol = OtlpExportProtocol.Grpc;
        })
);
```

### Console (for debugging)

```csharp
builder.Services.AddOpenTelemetryWithLlmTracing(
    serviceName: "MyService",
    configurator => configurator
        .AddConsoleExporter()
);
```

## Common Integration Patterns

### Using LlmInstrumentation Service

```csharp
/// <summary>
/// Service for handling LLM operations with proper telemetry integration
/// </summary>
public class MyLlmService
{
    private readonly OpenAIClient _client;
    private readonly LlmInstrumentation _instrumentation;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MyLlmService"/> class
    /// </summary>
    /// <param name="client">The OpenAI client for API communication</param>
    /// <param name="instrumentation">The LLM instrumentation service for telemetry</param>
    public MyLlmService(OpenAIClient client, LlmInstrumentation instrumentation)
    {
        _client = client;
        _instrumentation = instrumentation;
    }
    
    /// <summary>
    /// Generates text from the given prompt with telemetry tracking
    /// </summary>
    /// <param name="prompt">The input prompt to send to the LLM</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the generated text response.</returns>
    public async Task<string> GenerateTextAsync(string prompt)
    {
        var activity = _instrumentation.StartOperation(
            modelName: "gpt-4",
            prompt: prompt,
            taskType: "chat",
            provider: "openai");
            
        try
        {
            var stopwatch = Stopwatch.StartNew();
            var options = new ChatCompletionsOptions { /* configure options */ };
            
            // Use async/await for the API call
            var response = await _client.GetChatCompletionsAsync(options);
            stopwatch.Stop();
            
            var responseText = response.Value.Choices[0].Message.Content;
            
            _instrumentation.EndOperation(
                activity: activity,
                response: responseText,
                isSuccess: true,
                latencyMs: stopwatch.ElapsedMilliseconds);
                
            return responseText;
        }
        catch (Exception ex)
        {
            LLMTelemetry.RecordException(activity, ex);
            throw;
        }
    }
}
```

### Creating Rich Telemetry Data

```csharp
public async Task<string> GenerateTextWithDetailedTelemetry(string prompt)
{
    var stopwatch = Stopwatch.StartNew();
    var options = new ChatCompletionsOptions { ... };
    
    try
    {
        var response = await _client.GetChatCompletionsAsync(options);
        stopwatch.Stop();
        
        var operationData = AzureOpenAIAdapter.CreateChatCompletionData(
            options: options,
            result: response.Value,
            modelName: "gpt-4",
            latencyMs: stopwatch.ElapsedMilliseconds,
            isSuccess: true);
            
        _instrumentation.TrackOperation(operationData);
        
        return response.Value.Choices[0].Message.Content;
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        
        var operationData = new LlmOperationData
        {
            ModelName = "gpt-4",
            Prompt = prompt,
            TaskType = "chat",
            Provider = "azure",
            IsSuccess = false,
            LatencyMs = stopwatch.ElapsedMilliseconds,
            ErrorMessage = ex.Message
        };
        
        _instrumentation.TrackOperation(operationData);
        throw;
    }
}
```

## Handling Token Usage

The library makes it easy to track token usage in your LLM calls:

```csharp
using var activity = LLMTelemetry.StartLLMActivity(
    modelName: "gpt-4",
    prompt: prompt,
    taskType: "chat",
    provider: "openai");

var response = await client.GetChatCompletionsAsync(options);

// Manually add token usage if available
if (activity != null && response.Value.Usage != null)
{
    activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_PROMPT, response.Value.Usage.PromptTokens);
    activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_COMPLETION, response.Value.Usage.CompletionTokens);
    activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_TOTAL, response.Value.Usage.TotalTokens);
}

LLMTelemetry.EndLLMActivity(
    activity: activity,
    response: response.Value.Choices[0].Message.Content,
    isSuccess: true,
    latencyMs: stopwatch.ElapsedMilliseconds);
```

## Troubleshooting

### No Telemetry Data Appearing

1. Verify that you've added the correct OpenTelemetry exporters
2. Confirm that you've called `AddOpenInferenceLlmTelemetry` in your service configuration
3. Make sure `LLMTelemetry.ActivitySource` is registered with your TracerProvider

### Activity Data Missing Expected Fields

1. Check that `EmitTextContent` is set to `true` if you're expecting to see prompt/response text
2. Verify that `RecordTokenUsage` is enabled if you're looking for token counts
3. Ensure you're properly ending activities with `EndLLMActivity`

### General Troubleshooting Steps

Enable debug logging for OpenTelemetry to see more details:

```csharp
services.AddLogging(logging => {
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
});

services.Configure<OpenTelemetryLoggerOptions>(options => {
    options.IncludeScopes = true;
    options.ParseStateValues = true;
    options.IncludeFormattedMessage = true;
});
```

## Semantic Kernel Integration

### Basic Integration

Here's how to integrate OpenInference.LLM.Telemetry with Microsoft Semantic Kernel:

```csharp
// Set up telemetry options
LLMTelemetry.Configure(options => {
    options.EmitTextContent = true;
    options.MaxTextLength = 1000;
    options.EmitLatencyMetrics = true;
    options.RecordModelName = true;
});

// Using the SemanticKernelAdapter for tracking chat completions
var stopwatch = Stopwatch.StartNew();
var chatHistory = new ChatHistory();
chatHistory.AddSystemMessage("You are a helpful AI assistant.");
chatHistory.AddUserMessage("What are the benefits of telemetry in AI systems?");

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
var chatResult = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
stopwatch.Stop();

// Track the chat completion with telemetry
SemanticKernelAdapter.TrackChatCompletion(
    chatHistory: chatHistory,
    result: chatResult,
    modelName: "gpt-4", // Or retrieve from service if available
    latencyMs: stopwatch.ElapsedMilliseconds);
```

### Advanced Semantic Kernel Integration

For more advanced scenarios, you can extract additional metadata from Semantic Kernel responses:

```csharp
// Extract token usage from chat completion result if available
var tokenUsage = new Dictionary<string, int>();
if (chatResult.Metadata != null && 
    chatResult.Metadata.TryGetValue("Usage", out var usageObj) && 
    usageObj is OpenAIUsage usage)
{
    tokenUsage["prompt_tokens"] = usage.PromptTokens;
    tokenUsage["completion_tokens"] = usage.CompletionTokens;
    tokenUsage["total_tokens"] = usage.TotalTokens;
}

// Create detailed operation data
var operationData = SemanticKernelAdapter.CreateChatCompletionData(
    chatHistory: chatHistory,
    result: chatResult,
    modelName: "gpt-4",
    latencyMs: stopwatch.ElapsedMilliseconds,
    isSuccess: true,
    tokenUsage: tokenUsage);

// Track the operation with instrumentation service if needed
llmInstrumentation.TrackOperation(operationData);
```

### Adding Semantic Kernel Operations to a Workflow Chain

You can integrate Semantic Kernel operations into multi-step workflows:

```csharp
public async Task<string> RunAIWorkflowAsync(string userQuery)
{
    var chainSteps = new List<LlmOperationData>();
    var stopwatch = Stopwatch.StartNew();
    
    // Step 1: Use Semantic Kernel for initial processing
    var chatHistory = new ChatHistory();
    chatHistory.AddSystemMessage("You are a helpful AI assistant.");
    chatHistory.AddUserMessage(userQuery);
    
    var chatResult = await _semanticKernel.GetRequiredService<IChatCompletionService>
        .GetChatMessageContentAsync(chatHistory);
    
    // Create operation data for this step
    var step1Data = SemanticKernelAdapter.CreateChatCompletionData(
        chatHistory: chatHistory,
        result: chatResult,
        modelName: "gpt-4",
        latencyMs: stopwatch.ElapsedMilliseconds);
    
    chainSteps.Add(step1Data);
    
    // Additional workflow steps can be added here...
    
    // Track the entire workflow
    GenericLlmAdapter.TrackLlmChain(
        operationName: "AI_Workflow",
        chainSteps: chainSteps,
        totalLatencyMs: stopwatch.ElapsedMilliseconds);
    
    return chatResult.Content;
}
```

## Updated Advanced Usage

### Structured Logging with ILogger
```csharp
private static readonly ILogger? _logger;

static GenericLlmAdapter()
{
    var loggerFactory = LoggerFactory.Create(builder =>
        builder.AddConsole().SetMinimumLevel(LogLevel.Information));
    _logger = loggerFactory.CreateLogger(typeof(GenericLlmAdapter));
}

_logger?.LogInformation("Tracking LLM operation for model {ModelName}", modelName);
```

### Cost Tracking Example
```csharp
if (operationData.Cost.HasValue)
{
    activity.SetTag(SemanticConventions.LLM_USAGE_COST, operationData.Cost.Value);
    if (!string.IsNullOrEmpty(operationData.Currency))
    {
        activity.SetTag(SemanticConventions.LLM_USAGE_CURRENCY, operationData.Currency);
    }
}
```

### PII Sanitization Rules
```csharp
options.SanitizeSensitiveInfo = true; // Redacts sensitive information like credit card numbers and SSNs.
```

### Observability Configurations
#### Grafana Dashboard
1. Use Prometheus as the data source.
2. Create panels for:
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