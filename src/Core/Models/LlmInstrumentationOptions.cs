using System;

namespace OpenInference.LLM.Telemetry.Core.Models
{
    /// <summary>
    /// Configuration options for LLM telemetry instrumentation
    /// </summary>
    public class LlmInstrumentationOptions
    {
        private int _maxTextLength = 1000;
        
        /// <summary>
        /// Gets or sets whether to include prompt and response text content in telemetry.
        /// When true, the actual text of prompts and responses are included in spans.
        /// Default: true
        /// </summary>
        public bool EmitTextContent { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the maximum length of text content to include in telemetry.
        /// Text longer than this will be truncated to avoid excessive storage costs.
        /// Default: 1000 characters
        /// </summary>
        public int MaxTextLength 
        { 
            get => _maxTextLength; 
            set => _maxTextLength = value < 0 ? 0 : value; 
        }
        
        /// <summary>
        /// Gets or sets whether to emit latency metrics for LLM operations.
        /// Default: true
        /// </summary>
        public bool EmitLatencyMetrics { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to record the model name in telemetry.
        /// Default: true
        /// </summary>
        public bool RecordModelName { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to record token usage information.
        /// Default: true
        /// </summary>
        public bool RecordTokenUsage { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to sanitize sensitive information in prompts.
        /// When true, attempts to redact potential PII from telemetry.
        /// Default: false
        /// </summary>
        public bool SanitizeSensitiveInfo { get; set; } = false;
        
        /// <summary>
        /// Creates a new instance of LlmInstrumentationOptions with default settings
        /// </summary>
        public LlmInstrumentationOptions() { }

        /// <summary>
        /// Creates a validated copy of the options
        /// </summary>
        internal LlmInstrumentationOptions Validate()
        {
            if (MaxTextLength < 0)
                throw new ArgumentOutOfRangeException(nameof(MaxTextLength), "MaxTextLength cannot be negative");
                
            return this;
        }
    }
}