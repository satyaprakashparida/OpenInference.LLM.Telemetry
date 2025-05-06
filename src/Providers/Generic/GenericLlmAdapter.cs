using System.Diagnostics;
using OpenInference.LLM.Telemetry.Core.Models;

namespace OpenInference.LLM.Telemetry.Providers.Generic
{
    /// <summary>
    /// Generic adapter for any LLM orchestration framework to provide OpenInference telemetry conventions.
    /// This allows custom frameworks like LangChain, PromptFlow or custom .NET agents to emit standardized telemetry.
    /// </summary>
    public static class GenericLlmAdapter
    {
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
        public static void TrackLlmOperation(
            string prompt,
            string response,
            string modelName,
            long latencyMs,
            string taskType = "completion",
            string provider = "unknown",
            Dictionary<string, int>? tokenUsage = null,
            LlmInstrumentationOptions? instrumentationOptions = null)
        {
            using var activity = LLMTelemetry.StartLLMActivity(
                modelName: modelName,
                prompt: prompt,
                taskType: taskType,
                provider: provider,
                options: instrumentationOptions);

            LLMTelemetry.EndLLMActivity(
                activity: activity,
                response: response,
                isSuccess: true,
                latencyMs: latencyMs,
                options: instrumentationOptions);

            if (activity != null && tokenUsage != null)
            {
                // Add standard OpenTelemetry GenAI token usage
                if (tokenUsage.TryGetValue("prompt_tokens", out var promptTokens))
                    activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_PROMPT, promptTokens);
                
                if (tokenUsage.TryGetValue("completion_tokens", out var completionTokens))
                    activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_COMPLETION, completionTokens);
                
                if (tokenUsage.TryGetValue("total_tokens", out var totalTokens))
                    activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_TOTAL, totalTokens);

                // Add OpenInference specific telemetry attributes
                activity.SetTag("openinference.span.kind", "llm");
                activity.SetTag("openinference.llm.input_tokens", tokenUsage.GetValueOrDefault("prompt_tokens", 0));
                activity.SetTag("openinference.llm.output_tokens", tokenUsage.GetValueOrDefault("completion_tokens", 0));
                activity.SetTag("openinference.llm.total_tokens", tokenUsage.GetValueOrDefault("total_tokens", 0));

                // Add additional contextual information that OpenInference expects
                if (tokenUsage.TryGetValue("context_tokens", out var contextTokens))
                    activity.SetTag("openinference.llm.context_tokens", contextTokens);
            }
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
            int? promptTokens = tokenUsage?.GetValueOrDefault("prompt_tokens");
            int? completionTokens = tokenUsage?.GetValueOrDefault("completion_tokens");
            int? totalTokens = tokenUsage?.GetValueOrDefault("total_tokens");

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
        public static void TrackLlmChain(
            string operationName,
            IEnumerable<LlmOperationData> chainSteps,
            long totalLatencyMs,
            Dictionary<string, object>? additionalAttributes = null)
        {
            // Create a parent activity for the entire chain
            using var parentActivity = LLMTelemetry.ActivitySource.StartActivity(
                operationName,
                ActivityKind.Internal);

            if (parentActivity != null)
            {
                parentActivity.SetTag("openinference.span.kind", "chain");
                parentActivity.SetTag("openinference.chain.total_latency_ms", totalLatencyMs);
                
                // Add any additional attributes
                if (additionalAttributes != null)
                {
                    foreach (var attribute in additionalAttributes)
                    {
                        parentActivity.SetTag(attribute.Key, attribute.Value);
                    }
                }

                // Process each step
                int stepCount = 0;
                foreach (var step in chainSteps)
                {
                    // Create child activity for this step
                    using var childActivity = LLMTelemetry.ActivitySource.StartActivity(
                        $"{operationName}.step{stepCount}", 
                        ActivityKind.Internal,
                        parentActivity.Context);

                    if (childActivity != null)
                    {
                        childActivity.SetTag("openinference.span.kind", "llm");
                        childActivity.SetTag("openinference.chain.step", stepCount);
                        childActivity.SetTag("openinference.llm.model", step.ModelName);
                        childActivity.SetTag("openinference.llm.provider", step.Provider);
                        
                        if (step.PromptTokens.HasValue)
                            childActivity.SetTag("openinference.llm.input_tokens", step.PromptTokens.Value);
                        
                        if (step.CompletionTokens.HasValue)
                            childActivity.SetTag("openinference.llm.output_tokens", step.CompletionTokens.Value);
                        
                        if (step.TotalTokens.HasValue)
                            childActivity.SetTag("openinference.llm.total_tokens", step.TotalTokens.Value);
                        
                        childActivity.SetTag("openinference.llm.latency_ms", step.LatencyMs);
                        
                        // Add regular OTel GenAI attributes as well
                        childActivity.SetTag(Core.SemanticConventions.LLM_MODEL, step.ModelName);
                        childActivity.SetTag(Core.SemanticConventions.LLM_REQUEST_TYPE, step.TaskType);
                        
                        if (step.PromptTokens.HasValue)
                            childActivity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_PROMPT, step.PromptTokens.Value);
                        
                        if (step.CompletionTokens.HasValue)
                            childActivity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_COMPLETION, step.CompletionTokens.Value);
                        
                        if (step.TotalTokens.HasValue)
                            childActivity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_TOTAL, step.TotalTokens.Value);
                    }

                    stepCount++;
                }
            }
        }
    }
}