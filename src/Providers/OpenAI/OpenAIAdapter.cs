using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace OpenInference.LLM.Telemetry.Providers.OpenAI
{
    /// <summary>
    /// Adapter for OpenAI SDK
    /// </summary>
    public static class OpenAIAdapter
    {
        /// <summary>
        /// Tracks a chat completion operation using the OpenAI client
        /// </summary>
        /// <param name="messages">List of messages in the conversation</param>
        /// <param name="responseContent">The response content from the model</param>
        /// <param name="modelName">The name of the model being used</param>
        /// <param name="latencyMs">The latency in milliseconds</param>
        /// <param name="promptTokens">The number of tokens in the prompt, if available</param>
        /// <param name="completionTokens">The number of tokens in the completion, if available</param>
        /// <param name="totalTokens">The total number of tokens, if available</param>
        public static void TrackChatCompletion(
            object[] messages,
            string responseContent,
            string modelName,
            long latencyMs,
            int? promptTokens = null,
            int? completionTokens = null,
            int? totalTokens = null)
        {
            // Build a consolidated prompt string from messages
            var promptBuilder = new StringBuilder();
            foreach (dynamic message in messages)
            {
                promptBuilder.AppendLine($"[{message.Role}]: {message.Content}");
            }
            
            using var activity = LLMTelemetry.StartLLMActivity(
                modelName: modelName,
                prompt: promptBuilder.ToString(),
                taskType: "chat",
                provider: "openai");
                
            LLMTelemetry.EndLLMActivity(
                activity: activity,
                response: responseContent,
                isSuccess: true,
                latencyMs: latencyMs);
                
            // Add token information if available
            if (activity != null)
            {
                if (promptTokens.HasValue)
                    activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_PROMPT, promptTokens.Value);
                
                if (completionTokens.HasValue)
                    activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_COMPLETION, completionTokens.Value);
                
                if (totalTokens.HasValue)
                    activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_TOTAL, totalTokens.Value);
            }
        }
        
        /// <summary>
        /// Tracks a completion operation using the OpenAI client
        /// </summary>
        /// <param name="prompt">The prompt text</param>
        /// <param name="responseContent">The response content from the model</param>
        /// <param name="modelName">The name of the model being used</param>
        /// <param name="latencyMs">The latency in milliseconds</param>
        /// <param name="promptTokens">The number of tokens in the prompt, if available</param>
        /// <param name="completionTokens">The number of tokens in the completion, if available</param>
        /// <param name="totalTokens">The total number of tokens, if available</param>
        public static void TrackCompletion(
            string prompt,
            string responseContent,
            string modelName,
            long latencyMs,
            int? promptTokens = null,
            int? completionTokens = null,
            int? totalTokens = null)
        {
            using var activity = LLMTelemetry.StartLLMActivity(
                modelName: modelName,
                prompt: prompt,
                taskType: "completion",
                provider: "openai");
                
            LLMTelemetry.EndLLMActivity(
                activity: activity,
                response: responseContent,
                isSuccess: true,
                latencyMs: latencyMs);
                
            // Add token information if available
            if (activity != null)
            {
                if (promptTokens.HasValue)
                    activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_PROMPT, promptTokens.Value);
                
                if (completionTokens.HasValue)
                    activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_COMPLETION, completionTokens.Value);
                
                if (totalTokens.HasValue)
                    activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_TOTAL, totalTokens.Value);
            }
        }
    }
}