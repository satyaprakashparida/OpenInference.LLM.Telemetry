using OpenInference.LLM.Telemetry.Core.Models;
using System.Diagnostics;

namespace OpenInference.LLM.Telemetry.Extensions.OpenTelemetry.Instrumentation
{
    /// <summary>
    /// Instrumentation class for LLM operations
    /// </summary>
    public class LlmInstrumentation
    {
        private readonly LlmInstrumentationOptions _options;

        /// <summary>
        /// Creates a new LlmInstrumentation
        /// </summary>
        public LlmInstrumentation(LlmInstrumentationOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        
        /// <summary>
        /// Gets the current instrumentation options
        /// </summary>
        public LlmInstrumentationOptions Options => _options;

        /// <summary>
        /// Tracks an LLM operation and returns the created activity
        /// </summary>
        /// <param name="operationData">The operation data to track</param>
        /// <returns>The created Activity</returns>
        public Activity? TrackOperation(LlmOperationData operationData)
        {
            if (operationData == null)
                throw new ArgumentNullException(nameof(operationData));

            var activity = LLMTelemetry.StartLLMActivity(
                modelName: operationData.ModelName ?? "unknown",
                prompt: operationData.Prompt ?? string.Empty,
                taskType: operationData.TaskType ?? "unknown",
                provider: operationData.Provider ?? "unknown",
                options: _options);

            if (activity != null)
            {
                LLMTelemetry.EndLLMActivity(
                    activity: activity,
                    response: operationData.Response ?? string.Empty,
                    isSuccess: operationData.IsSuccess,
                    latencyMs: operationData.LatencyMs,
                    options: _options);

                // Add token information if available
                if (_options.RecordTokenUsage)
                {
                    if (operationData.PromptTokens.HasValue)
                        activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_PROMPT, operationData.PromptTokens.Value);
                    
                    if (operationData.CompletionTokens.HasValue)
                        activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_COMPLETION, operationData.CompletionTokens.Value);
                    
                    if (operationData.TotalTokens.HasValue)
                        activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_TOTAL, operationData.TotalTokens.Value);
                }

                // Add error message if present
                if (!string.IsNullOrEmpty(operationData.ErrorMessage))
                    activity.SetTag(Core.SemanticConventions.LLM_ERROR_MESSAGE, operationData.ErrorMessage);
            }

            return activity;
        }

        /// <summary>
        /// Creates a new Activity for an LLM operation but doesn't end it
        /// </summary>
        /// <param name="modelName">The model name</param>
        /// <param name="prompt">The prompt text</param>
        /// <param name="taskType">The task type</param>
        /// <param name="provider">The provider name</param>
        /// <returns>The created Activity</returns>
        public Activity? StartOperation(string modelName, string prompt, string taskType, string provider)
        {
            return LLMTelemetry.StartLLMActivity(
                modelName: modelName,
                prompt: prompt,
                taskType: taskType,
                provider: provider,
                options: _options);
        }

        /// <summary>
        /// Ends an in-progress LLM operation
        /// </summary>
        /// <param name="activity">The Activity to end</param>
        /// <param name="response">The model response</param>
        /// <param name="isSuccess">Whether the operation was successful</param>
        /// <param name="latencyMs">The operation latency in milliseconds</param>
        public void EndOperation(Activity? activity, string response, bool isSuccess, long latencyMs)
        {
            if (activity == null) return;
            
            LLMTelemetry.EndLLMActivity(
                activity: activity,
                response: response,
                isSuccess: isSuccess,
                latencyMs: latencyMs,
                options: _options);
        }
    }
}