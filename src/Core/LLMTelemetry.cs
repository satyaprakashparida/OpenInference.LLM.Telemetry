using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using OpenInference.LLM.Telemetry.Core.Models;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace OpenInference.LLM.Telemetry
{
    /// <summary>
    /// Core telemetry class for tracking LLM operations according to OpenInference semantic conventions
    /// </summary>
    public static class LLMTelemetry 
    {
        /// <summary>
        /// The ActivitySource used for creating LLM telemetry spans
        /// </summary>
        public static readonly ActivitySource ActivitySource = new("OpenInference.LLM.Telemetry");
        
        private static LlmInstrumentationOptions _globalOptions = new LlmInstrumentationOptions();
        
        /// <summary>
        /// Configure global options for LLM telemetry
        /// </summary>
        /// <param name="configure">Action to configure options</param>
        public static void Configure(Action<LlmInstrumentationOptions> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
                
            var options = new LlmInstrumentationOptions();
            configure(options);
            _globalOptions = options.Validate();
        }
        
        /// <summary>
        /// Starts an Activity for tracking an LLM operation with OpenInference semantic conventions
        /// </summary>
        /// <param name="modelName">The LLM model name</param>
        /// <param name="prompt">The prompt text</param>
        /// <param name="taskType">The task type (e.g. "chat", "completion")</param>
        /// <param name="provider">The LLM provider (e.g. "openai", "azure")</param>
        /// <param name="options">Optional instrumentation options specific to this activity</param>
        /// <returns>The created Activity or null if disabled</returns>
        public static Activity? StartLLMActivity(
            string modelName, 
            string prompt, 
            string taskType, 
            string provider,
            LlmInstrumentationOptions? options = null)
        {
            var opts = options ?? _globalOptions;
            
            var activity = ActivitySource.StartActivity(
                name: "llm.completion",
                kind: ActivityKind.Client);

            if (activity != null)
            {
                activity.SetTag(Core.SemanticConventions.LLM_REQUEST_TYPE, taskType);
                
                if (opts.RecordModelName)
                    activity.SetTag(Core.SemanticConventions.LLM_MODEL, modelName);
                
                activity.SetTag(Core.SemanticConventions.LLM_MODEL_PROVIDER, provider);
                
                if (opts.EmitTextContent && !string.IsNullOrEmpty(prompt))
                {
                    var sanitizedPrompt = opts.SanitizeSensitiveInfo ? SanitizeText(prompt) : prompt;
                    var truncatedPrompt = TruncateText(sanitizedPrompt, opts.MaxTextLength);
                    activity.SetTag(Core.SemanticConventions.LLM_REQUEST, truncatedPrompt);
                }
            }

            return activity;
        }
        
        /// <summary>
        /// Completes an LLM Activity with response data
        /// </summary>
        /// <param name="activity">The Activity to end</param>
        /// <param name="response">The model response</param>
        /// <param name="isSuccess">Whether the operation was successful</param>
        /// <param name="latencyMs">The operation latency in milliseconds</param>
        /// <param name="options">Optional instrumentation options specific to this activity</param>
        public static void EndLLMActivity(
            Activity? activity, 
            string response, 
            bool isSuccess, 
            long latencyMs,
            LlmInstrumentationOptions? options = null)
        {
            if (activity == null) return;
            
            var opts = options ?? _globalOptions;

            if (opts.EmitTextContent && !string.IsNullOrEmpty(response))
            {
                var sanitizedResponse = opts.SanitizeSensitiveInfo ? SanitizeText(response) : response;
                var truncatedResponse = TruncateText(sanitizedResponse, opts.MaxTextLength);
                activity.SetTag(Core.SemanticConventions.LLM_RESPONSE, truncatedResponse);
            }
            
            activity.SetTag(Core.SemanticConventions.LLM_SUCCESS, isSuccess);
            
            if (opts.EmitLatencyMetrics)
                activity.SetTag(Core.SemanticConventions.LLM_LATENCY_MS, latencyMs);
            
            activity.Stop();
        }
        
        /// <summary>
        /// Adds exception information to an LLM Activity
        /// </summary>
        /// <param name="activity">The Activity to add the exception to</param>
        /// <param name="exception">The exception that occurred</param>
        public static void RecordException(Activity? activity, Exception exception)
        {
            if (activity == null || exception == null) return;

            activity.SetTag(Core.SemanticConventions.LLM_SUCCESS, false);
            activity.SetTag(Core.SemanticConventions.LLM_ERROR_MESSAGE, exception.Message);
            activity.RecordException(exception);
        }
        
        /// <summary>
        /// Truncates text to the specified maximum length
        /// </summary>
        private static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;
                
            return text.Substring(0, maxLength) + "...";
        }
        
        // Regular expressions for PII detection
        private static readonly Regex EmailRegex = new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b");
        private static readonly Regex CreditCardRegex = new Regex(@"\b(?:\d[ -]*?){13,16}\b");
        private static readonly Regex SsnRegex = new Regex(@"\b\d{3}-\d{2}-\d{4}\b");
        
        /// <summary>
        /// Basic sanitization of potentially sensitive information
        /// </summary>
        private static string SanitizeText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
                
            // Very basic PII redaction - in production, use a more sophisticated approach
            string redacted = EmailRegex.Replace(text, "[EMAIL]");
            redacted = CreditCardRegex.Replace(redacted, "[CREDIT_CARD]");
            redacted = SsnRegex.Replace(redacted, "[SSN]");
                
            return redacted;
        }
    }
}