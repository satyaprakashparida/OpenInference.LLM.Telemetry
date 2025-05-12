using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using OpenInference.LLM.Telemetry.Core.Models;
using OpenInference.LLM.Telemetry.Core.Utilities;

namespace OpenInference.LLM.Telemetry.Core
{
    /// <summary>
    /// Core class for LLM telemetry instrumentation.
    /// </summary>
    public static class LLMTelemetry
    {
        private static readonly ActivitySource _activitySource = new ActivitySource("OpenInference.LLM.Telemetry", "1.0.0");
        private static readonly Meter _meter = new Meter("OpenInference.LLM.Telemetry", "1.0.0");
        private static LlmInstrumentationOptions _options = new LlmInstrumentationOptions();
        
        // Define metrics
        private static readonly Counter<long> _llmRequestCounter;
        private static readonly Histogram<long> _llmLatencyHistogram;
        private static readonly Counter<long> _llmTokensCounter;
        private static readonly Counter<decimal> _llmCostCounter;
        private static readonly Counter<long> _llmErrorCounter;

        /// <summary>
        /// Gets the ActivitySource used for creating LLM telemetry activities.
        /// </summary>
        public static ActivitySource ActivitySource => _activitySource;

        static LLMTelemetry()
        {
            // Initialize metrics
            _llmRequestCounter = _meter.CreateCounter<long>("llm.requests.count", "requests", "Number of LLM requests");
            _llmLatencyHistogram = _meter.CreateHistogram<long>("llm.latency", "ms", "Latency of LLM requests in milliseconds");
            _llmTokensCounter = _meter.CreateCounter<long>("llm.tokens.count", "tokens", "Number of tokens used in LLM requests");
            _llmCostCounter = _meter.CreateCounter<decimal>("llm.cost", "currency", "Cost of LLM requests");
            _llmErrorCounter = _meter.CreateCounter<long>("llm.errors.count", "errors", "Number of LLM request errors");
        }

        /// <summary>
        /// Configures global options for LLM telemetry.
        /// </summary>
        /// <param name="configure">Action to configure options.</param>
        public static void Configure(Action<LlmInstrumentationOptions> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
                
            configure(_options);
        }

        /// <summary>
        /// Starts an activity for tracking an LLM operation.
        /// </summary>
        /// <param name="modelName">Name of the LLM model.</param>
        /// <param name="prompt">The prompt sent to the model.</param>
        /// <param name="taskType">The task type (e.g. "chat", "completion", "embedding").</param>
        /// <param name="provider">The model provider (e.g. "azure", "openai", "anthropic").</param>
        /// <param name="options">Optional instrumentation options that override global options.</param>
        /// <returns>The created activity.</returns>
        public static Activity? StartLLMActivity(
            string modelName,
            string prompt,
            string taskType,
            string provider,
            LlmInstrumentationOptions? options = null)
        {
            var opts = options ?? _options;

            // Create and start an activity
            var activity = _activitySource.StartActivity(
                name: $"llm.{taskType}",
                kind: ActivityKind.Client);

            if (activity != null)
            {
                // Set common attributes for all LLM operations
                activity.SetTag(SemanticConventions.OPENINFERENCE_SPAN_KIND, SemanticConventions.SpanKind.LLM);
                activity.SetTag(SemanticConventions.LLM_REQUEST_TYPE, taskType);
                
                if (opts.RecordModelName)
                {
                    activity.SetTag(SemanticConventions.LLM_MODEL, modelName);
                    activity.SetTag(SemanticConventions.LLM_MODEL_PROVIDER, provider);
                }

                // Set request content (prompt) if enabled and within length limits
                if (opts.EmitTextContent && !string.IsNullOrEmpty(prompt))
                {
                    var sanitizedPrompt = prompt;
                    
                    // Apply sanitization if enabled
                    if (opts.SanitizeSensitiveInfo)
                    {
                        sanitizedPrompt = opts.TextSanitizer.Sanitize(prompt);
                    }
                    
                    // Truncate if needed
                    if (sanitizedPrompt.Length > opts.MaxTextLength)
                    {
                        sanitizedPrompt = sanitizedPrompt.Substring(0, opts.MaxTextLength) + "... [truncated]";
                    }
                    
                    activity.SetTag(SemanticConventions.LLM_REQUEST, sanitizedPrompt);
                }
                
                // Add any default attributes
                foreach (var attr in opts.DefaultAttributes)
                {
                    activity.SetTag(attr.Key, attr.Value);
                }
            }

            return activity;
        }

        /// <summary>
        /// Ends an LLM activity and records metrics.
        /// </summary>
        /// <param name="activity">The activity to end.</param>
        /// <param name="response">The model response.</param>
        /// <param name="isSuccess">Whether the operation was successful.</param>
        /// <param name="latencyMs">The latency of the operation in milliseconds.</param>
        /// <param name="options">Optional instrumentation options that override global options.</param>
        public static void EndLLMActivity(
            Activity? activity,
            string response,
            bool isSuccess,
            long latencyMs,
            LlmInstrumentationOptions? options = null)
        {
            if (activity == null)
                return;
                
            var opts = options ?? _options;
            
            // Set response and success status
            activity.SetTag(SemanticConventions.LLM_SUCCESS, isSuccess);
            
            if (opts.EmitTextContent && !string.IsNullOrEmpty(response))
            {
                var sanitizedResponse = response;
                
                // Apply sanitization if enabled
                if (opts.SanitizeSensitiveInfo)
                {
                    sanitizedResponse = opts.TextSanitizer.Sanitize(response);
                }
                
                // Truncate if needed
                if (sanitizedResponse.Length > opts.MaxTextLength)
                {
                    sanitizedResponse = sanitizedResponse.Substring(0, opts.MaxTextLength) + "... [truncated]";
                }
                
                activity.SetTag(SemanticConventions.LLM_RESPONSE, sanitizedResponse);
            }
            
            // Set latency
            activity.SetTag(SemanticConventions.LLM_LATENCY_MS, latencyMs);
            
            // Record metrics
            if (opts.EmitMetrics)
            {
                var tags = new TagList
                {
                    { "model", activity.GetTagItem(SemanticConventions.LLM_MODEL)?.ToString() ?? "unknown" },
                    { "provider", activity.GetTagItem(SemanticConventions.LLM_MODEL_PROVIDER)?.ToString() ?? "unknown" },
                    { "request_type", activity.GetTagItem(SemanticConventions.LLM_REQUEST_TYPE)?.ToString() ?? "unknown" },
                    { "success", isSuccess }
                };
                
                _llmRequestCounter.Add(1, tags);
                _llmLatencyHistogram.Record(latencyMs, tags);
                
                // Record error if operation failed
                if (!isSuccess)
                {
                    _llmErrorCounter.Add(1, tags);
                }
            }
            
            // End the activity
            activity.Stop();
        }

        /// <summary>
        /// Tracks a complete LLM operation.
        /// </summary>
        /// <param name="operationData">The operation data to track.</param>
        /// <param name="options">Optional instrumentation options that override global options.</param>
        /// <returns>The activity created for the operation.</returns>
        public static Activity? TrackLlmOperation(
            LlmOperationData operationData,
            LlmInstrumentationOptions? options = null)
        {
            if (operationData == null)
                throw new ArgumentNullException(nameof(operationData));
                
            var opts = options ?? _options;
            
            var activity = StartLLMActivity(
                operationData.ModelName ?? "unknown",
                operationData.Prompt ?? string.Empty,
                operationData.TaskType ?? "unknown",
                operationData.Provider ?? "unknown",
                opts);
                
            if (activity != null)
            {
                // Add token information if available
                if (opts.RecordTokenUsage)
                {
                    if (operationData.PromptTokens.HasValue)
                        activity.SetTag(SemanticConventions.LLM_TOKEN_COUNT_PROMPT, operationData.PromptTokens.Value);
                        
                    if (operationData.CompletionTokens.HasValue)
                        activity.SetTag(SemanticConventions.LLM_TOKEN_COUNT_COMPLETION, operationData.CompletionTokens.Value);
                        
                    if (operationData.TotalTokens.HasValue)
                        activity.SetTag(SemanticConventions.LLM_TOKEN_COUNT_TOTAL, operationData.TotalTokens.Value);
                        
                    // Record token metrics
                    if (opts.EmitMetrics && operationData.TotalTokens.HasValue)
                    {
                        var tags = new TagList
                        {
                            { "model", operationData.ModelName ?? "unknown" },
                            { "provider", operationData.Provider ?? "unknown" },
                            { "request_type", operationData.TaskType ?? "unknown" }
                        };
                        
                        _llmTokensCounter.Add(operationData.TotalTokens.Value, tags);
                    }
                }
                
                // Add cost information if available
                if (opts.RecordCostInformation && operationData.Cost.HasValue)
                {
                    activity.SetTag(SemanticConventions.LLM_USAGE_COST, operationData.Cost.Value);
                    
                    if (!string.IsNullOrEmpty(operationData.Currency))
                        activity.SetTag(SemanticConventions.LLM_USAGE_CURRENCY, operationData.Currency);
                        
                    // Record cost metrics
                    if (opts.EmitMetrics)
                    {
                        var tags = new TagList
                        {
                            { "model", operationData.ModelName ?? "unknown" },
                            { "provider", operationData.Provider ?? "unknown" },
                            { "request_type", operationData.TaskType ?? "unknown" },
                            { "currency", operationData.Currency ?? "USD" }
                        };
                        
                        _llmCostCounter.Add(operationData.Cost.Value, tags);
                    }
                }
                
                // Add chat information if available
                if (operationData.InputMessages?.Count > 0 || operationData.OutputMessages?.Count > 0)
                {
                    if (operationData.InputMessages?.Count > 0 && opts.EmitTextContent)
                    {
                        var serializedMessages = JsonSerializer.Serialize(operationData.InputMessages);
                        activity.SetTag(SemanticConventions.LLM_INPUT_MESSAGES, serializedMessages);
                    }
                    
                    if (operationData.OutputMessages?.Count > 0 && opts.EmitTextContent)
                    {
                        var serializedMessages = JsonSerializer.Serialize(operationData.OutputMessages);
                        activity.SetTag(SemanticConventions.LLM_OUTPUT_MESSAGES, serializedMessages);
                    }
                }
                
                // Add embedding information if available
                if (operationData.TaskType == "embedding" && operationData.Embedding != null)
                {
                    activity.SetTag(SemanticConventions.EMBEDDING_MODEL_NAME, operationData.Embedding.ModelName);
                    
                    if (operationData.Embedding.Dimensions.HasValue)
                        activity.SetTag(SemanticConventions.EMBEDDING_DIMENSIONS, operationData.Embedding.Dimensions.Value);
                        
                    if (operationData.Embedding.Truncated.HasValue)
                        activity.SetTag(SemanticConventions.EMBEDDING_TRUNCATED, operationData.Embedding.Truncated.Value);
                        
                    if (opts.CaptureEmbeddingVectors && operationData.Embedding.Vectors?.Count > 0)
                    {
                        activity.SetTag(SemanticConventions.EMBEDDING_VECTOR, JsonSerializer.Serialize(operationData.Embedding.Vectors));
                    }
                }
                
                // Add retrieval information if available
                if (operationData.Retrieval != null)
                {
                    if (!string.IsNullOrEmpty(operationData.Retrieval.Query))
                        activity.SetTag(SemanticConventions.RETRIEVER_QUERY, operationData.Retrieval.Query);
                        
                    if (!string.IsNullOrEmpty(operationData.Retrieval.RetrieverType))
                        activity.SetTag(SemanticConventions.RETRIEVER_TYPE, operationData.Retrieval.RetrieverType);
                        
                    if (operationData.Retrieval.TopK.HasValue)
                        activity.SetTag(SemanticConventions.RETRIEVER_TOP_K, operationData.Retrieval.TopK.Value);
                        
                    if (operationData.Retrieval.Documents?.Count > 0)
                    {
                        if (opts.IncludeDocumentContent)
                        {
                            activity.SetTag(SemanticConventions.RETRIEVAL_DOCUMENTS, JsonSerializer.Serialize(operationData.Retrieval.Documents));
                        }
                        else
                        {
                            // Include only document IDs and scores
                            var documentIds = new List<object>();
                            foreach (var doc in operationData.Retrieval.Documents)
                            {
                                documentIds.Add(new { id = doc.Id, score = doc.Score });
                            }
                            activity.SetTag(SemanticConventions.RETRIEVAL_DOCUMENTS, JsonSerializer.Serialize(documentIds));
                        }
                    }
                }
                
                // Add tool information if available
                if (operationData.Tools?.Count > 0 && opts.CaptureToolCallDetails)
                {
                    activity.SetTag(SemanticConventions.LLM_TOOLS, JsonSerializer.Serialize(operationData.Tools));
                }
                
                // Add streaming information if available
                if (operationData.StreamingData != null)
                {
                    activity.SetTag(SemanticConventions.LLM_IS_STREAMING, operationData.StreamingData.IsStreaming);
                    
                    if (!string.IsNullOrEmpty(operationData.StreamingData.ChunkId))
                        activity.SetTag(SemanticConventions.LLM_STREAM_CHUNK_ID, operationData.StreamingData.ChunkId);
                        
                    if (operationData.StreamingData.TotalChunks.HasValue)
                        activity.SetTag(SemanticConventions.LLM_STREAM_TOTAL_CHUNKS, operationData.StreamingData.TotalChunks.Value);
                }
                
                // Add prompt template information if available
                if (operationData.PromptTemplate != null)
                {
                    if (!string.IsNullOrEmpty(operationData.PromptTemplate.Template))
                        activity.SetTag(SemanticConventions.LLM_PROMPT_TEMPLATE_TEMPLATE, operationData.PromptTemplate.Template);
                        
                    if (!string.IsNullOrEmpty(operationData.PromptTemplate.Version))
                        activity.SetTag(SemanticConventions.LLM_PROMPT_TEMPLATE_VERSION, operationData.PromptTemplate.Version);
                        
                    if (!string.IsNullOrEmpty(operationData.PromptTemplate.Name))
                        activity.SetTag(SemanticConventions.LLM_PROMPT_TEMPLATE_NAME, operationData.PromptTemplate.Name);
                        
                    if (!string.IsNullOrEmpty(operationData.PromptTemplate.Type))
                        activity.SetTag(SemanticConventions.LLM_PROMPT_TEMPLATE_TYPE, operationData.PromptTemplate.Type);
                        
                    if (operationData.PromptTemplate.Variables?.Count > 0)
                        activity.SetTag(SemanticConventions.LLM_PROMPT_TEMPLATE_VARIABLES, JsonSerializer.Serialize(operationData.PromptTemplate.Variables));
                }
                
                // Add custom attributes if available
                if (operationData.CustomAttributes?.Count > 0)
                {
                    foreach (var attr in operationData.CustomAttributes)
                    {
                        activity.SetTag(attr.Key, attr.Value);
                    }
                }
                
                // Add invocation parameters if available
                if (operationData.InvocationParameters?.Count > 0)
                {
                    activity.SetTag(SemanticConventions.LLM_INVOCATION_PARAMETERS, JsonSerializer.Serialize(operationData.InvocationParameters));
                }
                
                // Add error information if the operation failed
                if (!operationData.IsSuccess && !string.IsNullOrEmpty(operationData.ErrorMessage))
                {
                    activity.SetTag(SemanticConventions.LLM_ERROR_MESSAGE, operationData.ErrorMessage);
                }
                
                EndLLMActivity(
                    activity,
                    operationData.Response ?? string.Empty,
                    operationData.IsSuccess,
                    operationData.LatencyMs,
                    opts);
            }
            
            return activity;
        }

        /// <summary>
        /// Records an exception in an LLM activity.
        /// </summary>
        /// <param name="activity">The activity to record the exception in.</param>
        /// <param name="exception">The exception to record.</param>
        public static void RecordException(Activity? activity, Exception exception)
        {
            if (activity == null || exception == null)
                return;
                
            activity.SetTag(SemanticConventions.LLM_SUCCESS, false);
            activity.SetTag(SemanticConventions.LLM_ERROR_MESSAGE, exception.Message);
            
            // Record the exception
            activity.RecordException(exception);
        }

        /// <summary>
        /// Asynchronously tracks an LLM operation.
        /// </summary>
        /// <typeparam name="TResult">The type of the operation result.</typeparam>
        /// <param name="operation">The operation to track.</param>
        /// <param name="modelName">The model name.</param>
        /// <param name="prompt">The prompt text.</param>
        /// <param name="taskType">The task type.</param>
        /// <param name="provider">The model provider.</param>
        /// <param name="options">Optional instrumentation options.</param>
        /// <returns>The result of the operation.</returns>
        public static async Task<TResult> TrackLlmOperationAsync<TResult>(
            Func<Task<TResult>> operation,
            string modelName,
            string prompt, 
            string taskType,
            string provider,
            LlmInstrumentationOptions? options = null)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
                
            var stopwatch = Stopwatch.StartNew();
            var activity = StartLLMActivity(modelName, prompt, taskType, provider, options);
            
            try
            {
                var result = await operation().ConfigureAwait(false);
                stopwatch.Stop();
                
                EndLLMActivity(activity, result?.ToString() ?? string.Empty, true, stopwatch.ElapsedMilliseconds, options);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                RecordException(activity, ex);
                EndLLMActivity(activity, string.Empty, false, stopwatch.ElapsedMilliseconds, options);
                
                throw;
            }
        }
        
        /// <summary>
        /// Gets the current global instrumentation options.
        /// </summary>
        /// <returns>The current instrumentation options.</returns>
        public static LlmInstrumentationOptions GetOptions() => _options;
    }
}