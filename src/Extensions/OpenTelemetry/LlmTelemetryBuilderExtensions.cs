using OpenInference.LLM.Telemetry.Core.Models;
using OpenInference.LLM.Telemetry.Extensions.OpenTelemetry.Instrumentation;
using OpenTelemetry.Trace;

namespace OpenInference.LLM.Telemetry.Extensions.OpenTelemetry
{
    /// <summary>
    /// Extension methods for <see cref="TracerProviderBuilder"/> to add OpenInference LLM telemetry.
    /// </summary>
    public static class LlmTelemetryBuilderExtensions
    {
        /// <summary>
        /// Adds LLM instrumentation to the OpenTelemetry TracerProvider.
        /// </summary>
        /// <param name="builder">The <see cref="TracerProviderBuilder"/> to add instrumentation to.</param>
        /// <returns>The <see cref="TracerProviderBuilder"/> for chaining.</returns>
        public static TracerProviderBuilder AddLlmTelemetry(this TracerProviderBuilder builder)
        {
            return AddLlmTelemetry(builder, new LlmInstrumentationOptions());
        }

        /// <summary>
        /// Adds LLM instrumentation to the OpenTelemetry TracerProvider with the specified options.
        /// </summary>
        /// <param name="builder">The <see cref="TracerProviderBuilder"/> to add instrumentation to.</param>
        /// <param name="options">The options for LLM telemetry instrumentation.</param>
        /// <returns>The <see cref="TracerProviderBuilder"/> for chaining.</returns>
        public static TracerProviderBuilder AddLlmTelemetry(this TracerProviderBuilder builder, LlmInstrumentationOptions options)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (options == null)
                throw new ArgumentNullException(nameof(options));
            
            // Register the Activity source with OpenTelemetry
            builder.AddSource(LLMTelemetry.ActivitySource.Name);
            
            // Configure the global LLMTelemetry options
            LLMTelemetry.Configure(opt => 
            {
                opt.EmitTextContent = options.EmitTextContent;
                opt.MaxTextLength = options.MaxTextLength;
                opt.EmitLatencyMetrics = options.EmitLatencyMetrics;
                opt.RecordModelName = options.RecordModelName;
                opt.RecordTokenUsage = options.RecordTokenUsage;
                opt.SanitizeSensitiveInfo = options.SanitizeSensitiveInfo;
                opt.RecordCostInformation = options.RecordCostInformation;
                opt.CaptureEmbeddingVectors = options.CaptureEmbeddingVectors;
                opt.IncludeDocumentContent = options.IncludeDocumentContent;
                opt.CaptureToolCallDetails = options.CaptureToolCallDetails;
                opt.EmitMetrics = options.EmitMetrics;
                opt.CaptureStructuredErrors = options.CaptureStructuredErrors;
                
                // Copy any default attributes
                foreach (var attr in options.DefaultAttributes)
                {
                    opt.DefaultAttributes[attr.Key] = attr.Value;
                }
                
                // Use the provided text sanitizer if available
                if (options.TextSanitizer != null)
                {
                    opt.TextSanitizer = options.TextSanitizer;
                }
            });
            
            // Add semantic conventions to the OpenTelemetry TracerProvider
            LlmTelemetryBuilderExtensions.AddLlmInstrumentation(builder);
            
            return builder;
        }

        /// <summary>
        /// Adds LLM instrumentation to the OpenTelemetry TracerProvider with the specified configuration.
        /// </summary>
        /// <param name="builder">The <see cref="TracerProviderBuilder"/> to add instrumentation to.</param>
        /// <param name="configure">A delegate to configure the <see cref="LlmInstrumentationOptions"/>.</param>
        /// <returns>The <see cref="TracerProviderBuilder"/> for chaining.</returns>
        public static TracerProviderBuilder AddLlmTelemetry(this TracerProviderBuilder builder, Action<LlmInstrumentationOptions> configure)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var options = new LlmInstrumentationOptions();
            configure(options);

            return AddLlmTelemetry(builder, options);
        }
        
        /// <summary>
        /// Adds LLM instrumentation to the OpenTelemetry TracerProvider.
        /// </summary>
        /// <param name="builder">The <see cref="TracerProviderBuilder"/> to add instrumentation to.</param>
        /// <returns>The <see cref="TracerProviderBuilder"/> for chaining.</returns>
        internal static TracerProviderBuilder AddLlmInstrumentation(this TracerProviderBuilder builder)
        {
            // Register the LLM Activity processor if needed
            builder.AddProcessor(new LlmTelemetryProcessor());
            
            return builder;
        }
    }
}