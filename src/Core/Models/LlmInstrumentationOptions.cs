using OpenInference.LLM.Telemetry.Core.Utilities;

namespace OpenInference.LLM.Telemetry.Core.Models
{
    /// <summary>
    /// Options for configuring LLM telemetry instrumentation.
    /// </summary>
    public class LlmInstrumentationOptions
    {
        /// <summary>
        /// Gets or sets whether to emit OpenTelemetry metrics.
        /// </summary>
        public bool EmitMetrics { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to emit latency metrics.
        /// </summary>
        public bool EmitLatencyMetrics { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to capture structured error details.
        /// </summary>
        public bool CaptureStructuredErrors { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to include text content (prompts and responses) in telemetry.
        /// </summary>
        public bool EmitTextContent { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to record model name and provider information.
        /// </summary>
        public bool RecordModelName { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to record token usage information.
        /// </summary>
        public bool RecordTokenUsage { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to record cost information.
        /// </summary>
        public bool RecordCostInformation { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to sanitize sensitive information like credit card numbers and SSNs.
        /// </summary>
        public bool SanitizeSensitiveInfo { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum length of text content to include in telemetry.
        /// </summary>
        public int MaxTextLength { get; set; } = 10000;

        /// <summary>
        /// Gets or sets whether to capture embedding vectors in telemetry.
        /// </summary>
        public bool CaptureEmbeddingVectors { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to include full document content in retrieval telemetry.
        /// </summary>
        public bool IncludeDocumentContent { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to capture tool call details.
        /// </summary>
        public bool CaptureToolCallDetails { get; set; } = true;

        /// <summary>
        /// Gets or sets default attributes to include in all telemetry.
        /// </summary>
        public Dictionary<string, object> DefaultAttributes { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the text sanitizer used to remove sensitive information.
        /// </summary>
        public ITextSanitizer TextSanitizer { get; set; } = new DefaultTextSanitizer();

        /// <summary>
        /// Adds a default attribute to be included in all telemetry.
        /// </summary>
        /// <param name="key">The attribute key.</param>
        /// <param name="value">The attribute value.</param>
        /// <returns>This options instance for chaining.</returns>
        public LlmInstrumentationOptions AddDefaultAttribute(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            DefaultAttributes[key] = value;
            return this;
        }

        /// <summary>
        /// Sets a custom text sanitizer.
        /// </summary>
        /// <param name="sanitizer">The sanitizer to use.</param>
        /// <returns>This options instance for chaining.</returns>
        public LlmInstrumentationOptions WithSanitizer(ITextSanitizer sanitizer)
        {
            TextSanitizer = sanitizer ?? throw new ArgumentNullException(nameof(sanitizer));
            SanitizeSensitiveInfo = true;
            return this;
        }

        /// <summary>
        /// Sets the maximum length of text content to include in telemetry.
        /// </summary>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns>This options instance for chaining.</returns>
        public LlmInstrumentationOptions WithMaxTextLength(int maxLength)
        {
            if (maxLength <= 0)
                throw new ArgumentException("Maximum length must be greater than zero", nameof(maxLength));

            MaxTextLength = maxLength;
            return this;
        }
    }
}