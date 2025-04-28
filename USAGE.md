# Advanced Usage Guide for OpenInference.LLM.Telemetry

This document provides detailed examples and guidance for more advanced usage scenarios with the OpenInference.LLM.Telemetry library.

## Table of Contents

1. [ASP.NET Core Integration](#aspnet-core-integration)
2. [Azure Functions Integration](#azure-functions-integration)
3. [Custom Telemetry Exporters](#custom-telemetry-exporters)
4. [Common Integration Patterns](#common-integration-patterns)
5. [Handling Token Usage](#handling-token-usage)
6. [Troubleshooting](#troubleshooting)

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
public class MyLlmService
{
    private readonly OpenAIClient _client;
    private readonly LlmInstrumentation _instrumentation;
    
    public MyLlmService(OpenAIClient client, LlmInstrumentation instrumentation)
    {
        _client = client;
        _instrumentation = instrumentation;
    }
    
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
            var options = new ChatCompletionsOptions { ... };
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