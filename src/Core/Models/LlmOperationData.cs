using System;
using System.Text.Json.Serialization;

namespace OpenInference.LLM.Telemetry.Core.Models
{
    /// <summary>
    /// Data model for LLM operation telemetry
    /// </summary>
    public class LlmOperationData
    {
        /// <summary>
        /// Gets or sets the name of the LLM model used
        /// </summary>
        [JsonPropertyName("model_name")]
        public string? ModelName { get; set; }
        
        /// <summary>
        /// Gets or sets the prompt text sent to the model
        /// </summary>
        [JsonPropertyName("prompt")]
        public string? Prompt { get; set; }
        
        /// <summary>
        /// Gets or sets the response text from the model
        /// </summary>
        [JsonPropertyName("response")]
        public string? Response { get; set; }
        
        /// <summary>
        /// Gets or sets the task type (e.g. "chat", "completion")
        /// </summary>
        [JsonPropertyName("task_type")]
        public string? TaskType { get; set; }
        
        /// <summary>
        /// Gets or sets the LLM provider name (e.g. "azure", "openai")
        /// </summary>
        [JsonPropertyName("provider")]
        public string? Provider { get; set; }
        
        /// <summary>
        /// Gets or sets whether the operation was successful
        /// </summary>
        [JsonPropertyName("is_success")]
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// Gets or sets the operation latency in milliseconds
        /// </summary>
        [JsonPropertyName("latency_ms")]
        public long LatencyMs { get; set; }
        
        /// <summary>
        /// Gets or sets the number of tokens in the prompt
        /// </summary>
        [JsonPropertyName("prompt_tokens")]
        public int? PromptTokens { get; set; }
        
        /// <summary>
        /// Gets or sets the number of tokens in the completion/response
        /// </summary>
        [JsonPropertyName("completion_tokens")]
        public int? CompletionTokens { get; set; }
        
        /// <summary>
        /// Gets or sets the total number of tokens
        /// </summary>
        [JsonPropertyName("total_tokens")]
        public int? TotalTokens { get; set; }
        
        /// <summary>
        /// Gets or sets the timestamp when the operation started
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        
        /// <summary>
        /// Gets or sets any error message if the operation failed
        /// </summary>
        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; set; }
    }
}