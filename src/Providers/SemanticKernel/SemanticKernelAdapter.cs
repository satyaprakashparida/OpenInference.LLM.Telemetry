using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenInference.LLM.Telemetry.Core.Models;

namespace OpenInference.LLM.Telemetry.Providers.SemanticKernel
{
    /// <summary>
    /// Adapter for Microsoft Semantic Kernel to provide OpenInference telemetry conventions.
    /// This adapter brings OpenInference semantic conventions to Semantic Kernel operations.
    /// </summary>
    public static class SemanticKernelAdapter
    {
        /// <summary>
        /// Tracks a chat completion operation using Semantic Kernel with OpenInference conventions
        /// </summary>
        /// <param name="chatHistory">The chat history with prompt messages</param>
        /// <param name="result">The chat completion result</param>
        /// <param name="modelName">The name of the model being used</param>
        /// <param name="latencyMs">The latency in milliseconds</param>
        /// <param name="instrumentationOptions">Optional telemetry instrumentation options</param>
        public static void TrackChatCompletion(
            ChatHistory chatHistory,
            ChatMessageContent result,
            string modelName,
            long latencyMs,
            LlmInstrumentationOptions? instrumentationOptions = null)
        {
            if (result == null) return;

            // Build a consolidated prompt string
            var promptBuilder = new StringBuilder();
            foreach (var message in chatHistory)
            {
                promptBuilder.AppendLine($"[{message.Role}]: {message.Content}");
            }

            using var activity = LLMTelemetry.StartLLMActivity(
                modelName: modelName,
                prompt: promptBuilder.ToString(),
                taskType: "chat",
                provider: "semantic_kernel",
                options: instrumentationOptions);

            LLMTelemetry.EndLLMActivity(
                activity: activity,
                response: result.Content ?? string.Empty,
                isSuccess: true,
                latencyMs: latencyMs,
                options: instrumentationOptions);

            // Add token information if available
            if (activity != null && result.Metadata != null)
            {
                var tokenInfo = GetTokenInformation(result);
                if (tokenInfo != null)
                {
                    var (promptTokens, completionTokens, totalTokens) = tokenInfo.Value;

                    if (promptTokens.HasValue)
                        activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_PROMPT, promptTokens.Value);
                    
                    if (completionTokens.HasValue)
                        activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_COMPLETION, completionTokens.Value);
                    
                    if (totalTokens.HasValue)
                        activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_TOTAL, totalTokens.Value);

                    // Add OpenInference specific telemetry attributes
                    activity.SetTag("openinference.span.kind", "llm");
                    activity.SetTag("openinference.llm.input_tokens", promptTokens ?? 0);
                    activity.SetTag("openinference.llm.output_tokens", completionTokens ?? 0);
                    activity.SetTag("openinference.llm.total_tokens", totalTokens ?? 0);
                }
            }
        }

        /// <summary>
        /// Tracks a text completion operation using Semantic Kernel with OpenInference conventions
        /// </summary>
        /// <param name="prompt">The prompt text sent to the model</param>
        /// <param name="result">The text completion result</param>
        /// <param name="modelName">The name of the model being used</param>
        /// <param name="latencyMs">The latency in milliseconds</param>
        /// <param name="instrumentationOptions">Optional telemetry instrumentation options</param>
        public static void TrackTextCompletion(
            string prompt,
            TextContent result,
            string modelName,
            long latencyMs,
            LlmInstrumentationOptions? instrumentationOptions = null)
        {
            if (result == null) return;

            using var activity = LLMTelemetry.StartLLMActivity(
                modelName: modelName,
                prompt: prompt,
                taskType: "completion",
                provider: "semantic_kernel", 
                options: instrumentationOptions);

            LLMTelemetry.EndLLMActivity(
                activity: activity,
                response: result.Text ?? string.Empty,
                isSuccess: true,
                latencyMs: latencyMs,
                options: instrumentationOptions);

            // Add token information if available
            if (activity != null && result.Metadata != null)
            {
                var tokenInfo = GetTokenInformation(result);
                if (tokenInfo != null)
                {
                    var (promptTokens, completionTokens, totalTokens) = tokenInfo.Value;

                    if (promptTokens.HasValue)
                        activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_PROMPT, promptTokens.Value);
                    
                    if (completionTokens.HasValue)
                        activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_COMPLETION, completionTokens.Value);
                    
                    if (totalTokens.HasValue)
                        activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_TOTAL, totalTokens.Value);

                    // Add OpenInference specific telemetry attributes
                    activity.SetTag("openinference.span.kind", "llm");
                    activity.SetTag("openinference.llm.input_tokens", promptTokens ?? 0);
                    activity.SetTag("openinference.llm.output_tokens", completionTokens ?? 0);
                    activity.SetTag("openinference.llm.total_tokens", totalTokens ?? 0);
                }
            }
        }

        /// <summary>
        /// Creates an LlmOperationData object from a chat completion result
        /// </summary>
        /// <param name="chatHistory">The chat history with prompt messages</param>
        /// <param name="result">The chat completion result</param>
        /// <param name="modelName">The name of the model being used</param>
        /// <param name="latencyMs">The latency in milliseconds</param>
        /// <param name="isSuccess">Whether the operation was successful</param>
        /// <param name="errorMessage">Optional error message if the operation failed</param>
        /// <returns>An LlmOperationData object containing operation details</returns>
        public static LlmOperationData? CreateChatCompletionData(
            ChatHistory chatHistory,
            ChatMessageContent? result,
            string modelName,
            long latencyMs,
            bool isSuccess = true,
            string? errorMessage = null)
        {
            if (result == null) return null;

            // Build a consolidated prompt string
            var promptBuilder = new StringBuilder();
            foreach (var message in chatHistory)
            {
                promptBuilder.AppendLine($"[{message.Role}]: {message.Content}");
            }

            var tokenInfo = GetTokenInformation(result);
            
            int? promptTokens = null;
            int? completionTokens = null;
            int? totalTokens = null;
            
            if (tokenInfo.HasValue)
            {
                promptTokens = tokenInfo.Value.PromptTokens;
                completionTokens = tokenInfo.Value.CompletionTokens;
                totalTokens = tokenInfo.Value.TotalTokens;
            }

            return new LlmOperationData
            {
                ModelName = modelName,
                Prompt = promptBuilder.ToString(),
                Response = result.Content,
                TaskType = "chat",
                Provider = "semantic_kernel",
                IsSuccess = isSuccess,
                LatencyMs = latencyMs,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                TotalTokens = totalTokens,
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Creates an LlmOperationData object from a text completion result
        /// </summary>
        /// <param name="prompt">The prompt text</param>
        /// <param name="result">The text completion result</param>
        /// <param name="modelName">The name of the model being used</param>
        /// <param name="latencyMs">The latency in milliseconds</param>
        /// <param name="isSuccess">Whether the operation was successful</param>
        /// <param name="errorMessage">Optional error message if the operation failed</param>
        /// <returns>An LlmOperationData object containing operation details</returns>
        public static LlmOperationData? CreateTextCompletionData(
            string prompt,
            TextContent? result,
            string modelName,
            long latencyMs,
            bool isSuccess = true,
            string? errorMessage = null)
        {
            if (result == null) return null;

            var tokenInfo = GetTokenInformation(result);
            
            int? promptTokens = null;
            int? completionTokens = null;
            int? totalTokens = null;
            
            if (tokenInfo.HasValue)
            {
                promptTokens = tokenInfo.Value.PromptTokens;
                completionTokens = tokenInfo.Value.CompletionTokens;
                totalTokens = tokenInfo.Value.TotalTokens;
            }

            return new LlmOperationData
            {
                ModelName = modelName,
                Prompt = prompt,
                Response = result.Text,
                TaskType = "completion",
                Provider = "semantic_kernel",
                IsSuccess = isSuccess,
                LatencyMs = latencyMs,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                TotalTokens = totalTokens,
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Represents usage information from OpenAI models
        /// </summary>
        private class OpenAIUsage
        {
            /// <summary>
            /// Gets or sets the number of prompt tokens
            /// </summary>
            public int PromptTokens { get; set; }
            
            /// <summary>
            /// Gets or sets the number of completion tokens
            /// </summary>
            public int CompletionTokens { get; set; }
            
            /// <summary>
            /// Gets or sets the total number of tokens
            /// </summary>
            public int TotalTokens { get; set; }
        }

        /// <summary>
        /// Extracts token count information from the result's metadata
        /// </summary>
        /// <param name="result">The chat message content result</param>
        /// <returns>Tuple containing prompt, completion, and total token counts</returns>
        private static (int? PromptTokens, int? CompletionTokens, int? TotalTokens)? GetTokenInformation(ChatMessageContent result)
        {
            if (result.Metadata == null) return null;

            // Try to extract token usage for OpenAI responses
            if (result.Metadata.TryGetValue("Usage", out var usageObj) && 
                usageObj is OpenAIUsage usage)
            {
                return (
                    usage.PromptTokens,
                    usage.CompletionTokens,
                    usage.TotalTokens
                );
            }

            // Try to extract token count through individual metadata items
            int? promptTokens = null;
            int? completionTokens = null;
            int? totalTokens = null;

            if (result.Metadata.TryGetValue("PromptTokenCount", out var promptTokenObj) && 
                promptTokenObj is int promptTokenCount)
            {
                promptTokens = promptTokenCount;
            }

            if (result.Metadata.TryGetValue("CompletionTokenCount", out var completionTokenObj) && 
                completionTokenObj is int completionTokenCount)
            {
                completionTokens = completionTokenCount;
            }

            if (result.Metadata.TryGetValue("TotalTokenCount", out var totalTokenObj) && 
                totalTokenObj is int totalTokenCount)
            {
                totalTokens = totalTokenCount;
            }

            if (promptTokens != null || completionTokens != null || totalTokens != null)
            {
                return (promptTokens, completionTokens, totalTokens);
            }

            return null;
        }

        /// <summary>
        /// Extracts token count information from the result's metadata
        /// </summary>
        /// <param name="result">The text content result</param>
        /// <returns>Tuple containing prompt, completion, and total token counts</returns>
        private static (int? PromptTokens, int? CompletionTokens, int? TotalTokens)? GetTokenInformation(TextContent result)
        {
            if (result.Metadata == null) return null;

            // Try to extract token usage for OpenAI responses
            if (result.Metadata.TryGetValue("Usage", out var usageObj) && 
                usageObj is OpenAIUsage usage)
            {
                return (
                    usage.PromptTokens,
                    usage.CompletionTokens,
                    usage.TotalTokens
                );
            }

            // Try to extract token count through individual metadata items
            int? promptTokens = null;
            int? completionTokens = null;
            int? totalTokens = null;

            if (result.Metadata.TryGetValue("PromptTokenCount", out var promptTokenObj) && 
                promptTokenObj is int promptTokenCount)
            {
                promptTokens = promptTokenCount;
            }

            if (result.Metadata.TryGetValue("CompletionTokenCount", out var completionTokenObj) && 
                completionTokenObj is int completionTokenCount)
            {
                completionTokens = completionTokenCount;
            }

            if (result.Metadata.TryGetValue("TotalTokenCount", out var totalTokenObj) && 
                totalTokenObj is int totalTokenCount)
            {
                totalTokens = totalTokenCount;
            }

            if (promptTokens != null || completionTokens != null || totalTokens != null)
            {
                return (promptTokens, completionTokens, totalTokens);
            }

            return null;
        }
    }
}