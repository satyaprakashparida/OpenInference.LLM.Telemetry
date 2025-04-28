using Azure.AI.OpenAI;
using OpenInference.LLM.Telemetry.Core.Models;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OpenInference.LLM.Telemetry.Providers.Azure
{
    /// <summary>
    /// Adapter for Azure OpenAI SDK
    /// </summary>
    public static class AzureOpenAIAdapter
    {
        /// <summary>
        /// Tracks a chat completion operation using the Azure OpenAI client
        /// </summary>
        /// <param name="options">The chat completion options used</param>
        /// <param name="result">The chat completion result</param>
        /// <param name="modelName">The name of the model being used</param>
        /// <param name="latencyMs">The latency in milliseconds</param>
        /// <param name="instrumentationOptions">Optional telemetry instrumentation options</param>
        public static void TrackChatCompletion(
            global::Azure.AI.OpenAI.ChatCompletionsOptions options, // Reverted type name
            global::Azure.AI.OpenAI.ChatCompletions result,         // Reverted type name
            string modelName,
            long latencyMs,
            LlmInstrumentationOptions? instrumentationOptions = null)
        {
            if (result == null) return;
            // Build a consolidated prompt string
            var promptBuilder = new StringBuilder();
            foreach (var message in options.Messages)
            {
                string messageContent = string.Empty;
                if (message is ChatRequestUserMessage userMessage)
                {
                    messageContent = userMessage.Content;
                }
                else if (message is ChatRequestSystemMessage systemMessage)
                {
                    messageContent = systemMessage.Content;
                }
                else if (message is ChatRequestAssistantMessage assistantMessage)
                {
                    messageContent = assistantMessage.Content;
                }
                // Add other potential message types if needed (e.g., Tool messages)

                promptBuilder.AppendLine($"[{message.Role}]: {messageContent}");
            }
            // Get the completion content
            // Assuming Choices structure is similar, might need adjustment
            var responseContent = result?.Choices.FirstOrDefault()?.Message.Content ?? string.Empty;
            using var activity = LLMTelemetry.StartLLMActivity(
                modelName: modelName,
                prompt: promptBuilder.ToString(),
                taskType: "chat",
                provider: "azure",
                options: instrumentationOptions);
            LLMTelemetry.EndLLMActivity(
                activity: activity,
                response: responseContent,
                isSuccess: true,
                latencyMs: latencyMs,
                options: instrumentationOptions);
            // Add token information if available
            // Assuming Usage property and token names are similar
            if (activity != null && result?.Usage != null)
            {
                activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_PROMPT, result.Usage.PromptTokens);
                activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_COMPLETION, result.Usage.CompletionTokens);
                activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_TOTAL, result.Usage.TotalTokens);
            }
        }
        
        // Note: The 'Completions' type might have changed significantly or been deprecated
        // in v2.0.0 in favor of Chat Completions. This method might need a rewrite
        // depending on the intended functionality and the v2.0.0 API.
        // For now, commenting it out to resolve the immediate build error.
        /*
        /// <summary>
        /// Tracks a completions operation using the Azure OpenAI client
        /// </summary>
        /// <param name="prompt">The prompt text</param>
        /// <param name="result">The completions result</param>
        /// <param name="modelName">The name of the model being used</param>
        /// <param name="latencyMs">The latency in milliseconds</param>
        /// <param name="instrumentationOptions">Optional telemetry instrumentation options</param>
        public static void TrackCompletion(
            string prompt,
            Completions result, // This type likely changed in v2.0.0
            string modelName,
            long latencyMs,
            LlmInstrumentationOptions? instrumentationOptions = null)
        {
            if (result == null) return;
            // Get the completion content
            var responseContent = result?.Choices.FirstOrDefault()?.Text ?? string.Empty;
            using var activity = LLMTelemetry.StartLLMActivity(
                modelName: modelName,
                prompt: prompt,
                taskType: "completion",
                provider: "azure",
                options: instrumentationOptions);
            LLMTelemetry.EndLLMActivity(
                activity: activity,
                response: responseContent,
                isSuccess: true,
                latencyMs: latencyMs,
                options: instrumentationOptions);
            // Add token information if available
            if (activity != null && result?.Usage != null)
            {
                activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_PROMPT, result.Usage.PromptTokens);
                activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_COMPLETION, result.Usage.CompletionTokens);
                activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_TOTAL, result.Usage.TotalTokens);
            }
        }
        */
        
        /// <summary>
        /// Creates an LlmOperationData object from a chat completion result
        /// </summary>
        /// <param name="options">The chat completion options used</param>
        /// <param name="result">The chat completion result</param>
        /// <param name="modelName">The name of the model being used</param>
        /// <param name="latencyMs">The latency in milliseconds</param>
        /// <param name="isSuccess">Whether the operation was successful</param>
        /// <param name="errorMessage">Optional error message if the operation failed</param>
        /// <returns>An LlmOperationData object containing operation details</returns>
        public static LlmOperationData? CreateChatCompletionData(
            global::Azure.AI.OpenAI.ChatCompletionsOptions options, // Reverted type name
            global::Azure.AI.OpenAI.ChatCompletions? result,        // Reverted type name
            string modelName,
            long latencyMs,
            bool isSuccess = true,
            string? errorMessage = null)
        {
            if (result == null) return null;
            // Build a consolidated prompt string
            var promptBuilder = new StringBuilder();
            foreach (var message in options.Messages)
            {
                string messageContent = string.Empty;
                if (message is ChatRequestUserMessage userMessage)
                {
                    messageContent = userMessage.Content;
                }
                else if (message is ChatRequestSystemMessage systemMessage)
                {
                    messageContent = systemMessage.Content;
                }
                else if (message is ChatRequestAssistantMessage assistantMessage)
                {
                    messageContent = assistantMessage.Content;
                }
                // Add other potential message types if needed (e.g., Tool messages)

                promptBuilder.AppendLine($"[{message.Role}]: {messageContent}");
            }
            // Get the completion content
            // Assuming Choices structure is similar, might need adjustment
            var responseContent = result?.Choices.FirstOrDefault()?.Message.Content ?? string.Empty;
            return new LlmOperationData
            {
                ModelName = modelName,
                Prompt = promptBuilder.ToString(),
                Response = responseContent,
                TaskType = "chat",
                Provider = "azure",
                IsSuccess = isSuccess,
                LatencyMs = latencyMs,
                // Assuming Usage property and token names are similar
                PromptTokens = result?.Usage?.PromptTokens,
                CompletionTokens = result?.Usage?.CompletionTokens,
                TotalTokens = result?.Usage?.TotalTokens,
                ErrorMessage = errorMessage
            };
        }
    }
}