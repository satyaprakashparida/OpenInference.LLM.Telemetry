using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenInference.LLM.Telemetry.Core.Models;

namespace OpenInference.LLM.Telemetry.Providers.Generic
{
    /// <summary>
    /// Generic adapter for any LLM orchestration framework to provide OpenInference telemetry conventions.
    /// This allows custom frameworks like LangChain, PromptFlow or custom .NET agents to emit standardized telemetry.
    /// </summary>
    public static class GenericLlmAdapter
    {
        private static readonly ILogger? _logger;
        
        static GenericLlmAdapter()
        {
            // Try to get a logger if ILoggerFactory is available
            try
            {
                var loggerFactory = LoggerFactory.Create(builder => 
                    builder.AddConsole().SetMinimumLevel(LogLevel.Information));
                _logger = loggerFactory.CreateLogger(typeof(GenericLlmAdapter));
            }
            catch
            {
                // Continue without logging if logger creation fails
            }
        }

        /// <summary>
        /// Tracks a general LLM operation using OpenInference conventions
        /// </summary>
        /// <param name="prompt">The input prompt or message</param>
        /// <param name="response">The response from the LLM</param>
        /// <param name="modelName">The name of the LLM model used</param>
        /// <param name="latencyMs">The latency of the operation in milliseconds</param>
        /// <param name="taskType">The type of task performed (e.g., "chat", "completion", "embedding")</param>
        /// <param name="provider">The provider of the LLM (e.g., "azure", "openai", "anthropic")</param>
        /// <param name="tokenUsage">Optional dictionary with token usage information</param>
        /// <param name="instrumentationOptions">Optional instrumentation options</param>
        /// <returns>The created activity or null if disabled</returns>
        public static Activity? TrackLlmOperation(
            string prompt,
            string response,
            string modelName,
            long latencyMs,
            string taskType = "completion",
            string provider = "unknown",
            Dictionary<string, int>? tokenUsage = null,
            LlmInstrumentationOptions? instrumentationOptions = null)
        {
            var operationData = CreateLlmOperationData(
                prompt: prompt,
                response: response,
                modelName: modelName,
                latencyMs: latencyMs,
                taskType: taskType,
                provider: provider,
                tokenUsage: tokenUsage);
                
            return LlmTelemetry.TrackLlmOperation(operationData, instrumentationOptions);
        }

        /// <summary>
        /// Tracks a chat completion operation using OpenInference conventions
        /// </summary>
        /// <param name="messages">The input chat messages</param>
        /// <param name="responseMessages">The response messages</param>
        /// <param name="modelName">The name of the LLM model used</param>
        /// <param name="latencyMs">The latency of the operation in milliseconds</param>
        /// <param name="provider">The provider of the LLM (e.g., "azure", "openai", "anthropic")</param>
        /// <param name="tokenUsage">Optional dictionary with token usage information</param>
        /// <param name="instrumentationOptions">Optional instrumentation options</param>
        /// <returns>The created activity or null if disabled</returns>
        public static Activity? TrackChatOperation(
            IEnumerable<LlmChatMessage> messages,
            IEnumerable<LlmChatMessage> responseMessages,
            string modelName,
            long latencyMs,
            string provider = "unknown",
            Dictionary<string, int>? tokenUsage = null,
            LlmInstrumentationOptions? instrumentationOptions = null)
        {
            var operationData = new LlmOperationData
            {
                ModelName = modelName,
                TaskType = "chat",
                Provider = provider,
                LatencyMs = latencyMs,
                IsSuccess = true,
                InputMessages = new List<LlmChatMessage>(messages),
                OutputMessages = new List<LlmChatMessage>(responseMessages),
                Timestamp = DateTimeOffset.UtcNow
            };
            
            // Extract the prompt and response for standard telemetry
            operationData.Prompt = string.Join("\n", operationData.InputMessages.ConvertAll(m => $"{m.Role}: {m.Content}"));
            if (operationData.OutputMessages.Count > 0)
                operationData.Response = operationData.OutputMessages[operationData.OutputMessages.Count - 1].Content;
            
            // Add token usage
            if (tokenUsage != null)
            {
                if (tokenUsage.TryGetValue("prompt_tokens", out var promptTokens))
                    operationData.PromptTokens = promptTokens;
                
                if (tokenUsage.TryGetValue("completion_tokens", out var completionTokens))
                    operationData.CompletionTokens = completionTokens;
                
                if (tokenUsage.TryGetValue("total_tokens", out var totalTokens))
                    operationData.TotalTokens = totalTokens;
            }
            
            return LlmTelemetry.TrackLlmOperation(operationData, instrumentationOptions);
        }
        
        /// <summary>
        /// Tracks an embedding operation using OpenInference conventions
        /// </summary>
        /// <param name="texts">The texts to embed</param>
        /// <param name="modelName">The name of the embedding model used</param>
        /// <param name="latencyMs">The latency of the operation in milliseconds</param>
        /// <param name="dimensions">The dimensions of the embedding vectors</param>
        /// <param name="provider">The provider of the embeddings (e.g., "azure", "openai")</param>
        /// <param name="tokenUsage">Optional dictionary with token usage information</param>
        /// <param name="vectors">Optional embedding vectors</param>
        /// <param name="instrumentationOptions">Optional instrumentation options</param>
        /// <returns>The created activity or null if disabled</returns>
        public static Activity? TrackEmbeddingOperation(
            IEnumerable<string> texts,
            string modelName,
            long latencyMs,
            int dimensions,
            string provider = "unknown",
            Dictionary<string, int>? tokenUsage = null,
            IEnumerable<IEnumerable<float>>? vectors = null,
            LlmInstrumentationOptions? instrumentationOptions = null)
        {
            var operationData = new LlmOperationData
            {
                ModelName = modelName,
                TaskType = "embedding",
                Provider = provider,
                LatencyMs = latencyMs,
                IsSuccess = true,
                Timestamp = DateTimeOffset.UtcNow,
                Embedding = new LlmEmbeddingData
                {
                    ModelName = modelName,
                    Texts = new List<string>(texts),
                    Dimensions = dimensions
                }
            };
            
            // Join texts for standard telemetry field
            operationData.Prompt = string.Join("\n", operationData.Embedding.Texts);
            
            // Add vectors if provided
            if (vectors != null)
            {
                operationData.Embedding.Vectors = new List<List<float>>();
                foreach (var vector in vectors)
                {
                    operationData.Embedding.Vectors.Add(new List<float>(vector));
                }
            }
            
            // Add token usage
            if (tokenUsage != null)
            {
                if (tokenUsage.TryGetValue("prompt_tokens", out var promptTokens))
                    operationData.PromptTokens = promptTokens;
                
                if (tokenUsage.TryGetValue("completion_tokens", out var completionTokens))
                    operationData.CompletionTokens = completionTokens;
                
                if (tokenUsage.TryGetValue("total_tokens", out var totalTokens))
                    operationData.TotalTokens = totalTokens;
            }
            
            return LlmTelemetry.TrackLlmOperation(operationData, instrumentationOptions);
        }

        /// <summary>
        /// Tracks a retrieval operation using OpenInference conventions
        /// </summary>
        /// <param name="query">The query used for retrieval</param>
        /// <param name="documents">The documents retrieved</param>
        /// <param name="retrieverType">The type of retriever used</param>
        /// <param name="latencyMs">The latency of the operation in milliseconds</param>
        /// <param name="instrumentationOptions">Optional instrumentation options</param>
        /// <returns>The created activity or null if disabled</returns>
        public static Activity? TrackRetrievalOperation(
            string query,
            IEnumerable<LlmDocument> documents,
            string retrieverType,
            long latencyMs,
            LlmInstrumentationOptions? instrumentationOptions = null)
        {
            var operationData = new LlmOperationData
            {
                TaskType = "retrieval",
                Provider = "retrieval",
                LatencyMs = latencyMs,
                IsSuccess = true,
                Timestamp = DateTimeOffset.UtcNow,
                Prompt = query,
                Response = $"Retrieved {documents} documents",
                Retrieval = new LlmRetrievalData
                {
                    Query = query,
                    Documents = new List<LlmDocument>(documents),
                    RetrieverType = retrieverType
                }
            };
            
            return LlmTelemetry.TrackLlmOperation(operationData, instrumentationOptions);
        }
        
        /// <summary>
        /// Creates an LlmOperationData object from a custom LLM operation
        /// </summary>
        /// <param name="prompt">The input prompt or message</param>
        /// <param name="response">The response from the LLM</param>
        /// <param name="modelName">The name of the LLM model used</param>
        /// <param name="latencyMs">The latency of the operation in milliseconds</param>
        /// <param name="taskType">The type of task performed (e.g., "chat", "completion", "embedding")</param>
        /// <param name="provider">The provider of the LLM (e.g., "azure", "openai", "anthropic")</param>
        /// <param name="tokenUsage">Optional dictionary with token usage information</param>
        /// <param name="isSuccess">Whether the operation was successful</param>
        /// <param name="errorMessage">Optional error message if the operation failed</param>
        /// <returns>An LlmOperationData object containing operation details</returns>
        public static LlmOperationData CreateLlmOperationData(
            string prompt,
            string response,
            string modelName,
            long latencyMs,
            string taskType = "completion",
            string provider = "unknown",
            Dictionary<string, int>? tokenUsage = null,
            bool isSuccess = true,
            string? errorMessage = null)
        {
            int? promptTokens = null;
            int? completionTokens = null;
            int? totalTokens = null;
            
            if (tokenUsage != null)
            {
                tokenUsage.TryGetValue("prompt_tokens", out var pTokens);
                promptTokens = pTokens;
                
                tokenUsage.TryGetValue("completion_tokens", out var cTokens);
                completionTokens = cTokens;
                
                tokenUsage.TryGetValue("total_tokens", out var tTokens);
                totalTokens = tTokens;
            }

            return new LlmOperationData
            {
                ModelName = modelName,
                Prompt = prompt,
                Response = response,
                TaskType = taskType,
                Provider = provider,
                IsSuccess = isSuccess,
                LatencyMs = latencyMs,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                TotalTokens = totalTokens,
                ErrorMessage = errorMessage,
                Timestamp = DateTimeOffset.UtcNow
            };
        }

        /// <summary>
        /// Tracks a chain of LLM operations as a single traced operation with multiple steps
        /// </summary>
        /// <param name="operationName">The name of the overall operation</param>
        /// <param name="chainSteps">Collection of operation data for each step in the chain</param>
        /// <param name="totalLatencyMs">The total latency of the entire chain</param>
        /// <param name="additionalAttributes">Optional additional attributes to record</param>
        /// <returns>The created parent activity or null if disabled</returns>
        public static Activity? TrackLlmChain(
            string operationName,
            IEnumerable<LlmOperationData> chainSteps,
            long totalLatencyMs,
            Dictionary<string, object>? additionalAttributes = null)
        {
            // Create a parent activity for the entire chain
            var parentActivity = LlmTelemetry.ActivitySource.StartActivity(
                operationName,
                ActivityKind.Internal);

            if (parentActivity != null)
            {
                parentActivity.SetTag(Core.SemanticConventions.OPENINFERENCE_SPAN_KIND, Core.SemanticConventions.SpanKind.CHAIN);
                parentActivity.SetTag(Core.SemanticConventions.CHAIN_NAME, operationName);
                parentActivity.SetTag(Core.SemanticConventions.CHAIN_TYPE, "sequential");
                parentActivity.SetTag(Core.SemanticConventions.LLM_LATENCY_MS, totalLatencyMs);
                
                // Add any additional attributes
                if (additionalAttributes != null)
                {
                    foreach (var attribute in additionalAttributes)
                    {
                        parentActivity.SetTag(attribute.Key, attribute.Value);
                    }
                }

                // Process each step
                int stepIndex = 0;
                foreach (var step in chainSteps)
                {
                    try
                    {
                        // Create child activity for this step
                        using var childActivity = LlmTelemetry.ActivitySource.StartActivity(
                            $"{operationName}.step{stepIndex}", 
                            ActivityKind.Internal,
                            parentActivity.Context);

                        if (childActivity != null)
                        {
                            childActivity.SetTag(Core.SemanticConventions.OPENINFERENCE_SPAN_KIND, Core.SemanticConventions.SpanKind.LLM);
                            childActivity.SetTag(Core.SemanticConventions.CHAIN_STEP_INDEX, stepIndex);
                            childActivity.SetTag(Core.SemanticConventions.CHAIN_STEP_NAME, $"Step {stepIndex}");
                            
                            // Add standard LLM attributes
                            childActivity.SetTag(Core.SemanticConventions.LLM_MODEL, step.ModelName);
                            childActivity.SetTag(Core.SemanticConventions.LLM_MODEL_PROVIDER, step.Provider);
                            childActivity.SetTag(Core.SemanticConventions.LLM_REQUEST_TYPE, step.TaskType);
                            childActivity.SetTag(Core.SemanticConventions.LLM_REQUEST, step.Prompt);
                            childActivity.SetTag(Core.SemanticConventions.LLM_RESPONSE, step.Response);
                            childActivity.SetTag(Core.SemanticConventions.LLM_SUCCESS, step.IsSuccess);
                            childActivity.SetTag(Core.SemanticConventions.LLM_LATENCY_MS, step.LatencyMs);
                            
                            // Add token usage information
                            if (step.PromptTokens.HasValue)
                                childActivity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_PROMPT, step.PromptTokens.Value);
                            
                            if (step.CompletionTokens.HasValue)
                                childActivity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_COMPLETION, step.CompletionTokens.Value);
                            
                            if (step.TotalTokens.HasValue)
                                childActivity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_TOTAL, step.TotalTokens.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error tracking chain step {StepIndex}", stepIndex);
                    }

                    stepIndex++;
                }
                
                // Stop the parent activity
                parentActivity.Stop();
            }
            
            return parentActivity;
        }
        
        /// <summary>
        /// Tracks an LLM operation asynchronously with CancellationToken support
        /// </summary>
        /// <typeparam name="T">The return type of the operation</typeparam>
        /// <param name="operation">The async operation to execute and track</param>
        /// <param name="operationData">The LLM operation data for telemetry</param>
        /// <param name="instrumentationOptions">Optional instrumentation options</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>The result of the operation</returns>
        public static async Task<T> TrackOperationAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            LlmOperationData operationData,
            LlmInstrumentationOptions? instrumentationOptions = null,
            CancellationToken cancellationToken = default)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));
                
            if (operationData == null)
                throw new ArgumentNullException(nameof(operationData));
            
            var stopwatch = Stopwatch.StartNew();
            var activity = LlmTelemetry.StartLlmActivity(
                modelName: operationData.ModelName ?? "unknown",
                prompt: operationData.Prompt ?? string.Empty,
                taskType: operationData.TaskType ?? "completion",
                provider: operationData.Provider ?? "unknown",
                options: instrumentationOptions);
                
            try
            {
                // Execute the operation
                var result = await operation(cancellationToken).ConfigureAwait(false);
                stopwatch.Stop();
                
                // Update operation data with actual latency
                operationData.LatencyMs = stopwatch.ElapsedMilliseconds;
                
                // Handle response automatically if it's a string and we haven't set one
                if (result is string stringResult && string.IsNullOrEmpty(operationData.Response))
                    operationData.Response = stringResult;
                
                // End the activity with success
                if (activity != null)
                {
                    LlmTelemetry.EndLlmActivity(
                        activity: activity,
                        response: operationData.Response ?? string.Empty,
                        isSuccess: true,
                        latencyMs: operationData.LatencyMs,
                        options: instrumentationOptions);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Update operation data with error details
                operationData.IsSuccess = false;
                operationData.ErrorMessage = ex.Message;
                operationData.LatencyMs = stopwatch.ElapsedMilliseconds;
                
                // Record exception in activity
                if (activity != null)
                {
                    LlmTelemetry.RecordException(activity, ex);
                    activity.Stop();
                }
                
                throw;
            }
        }
    }
}
