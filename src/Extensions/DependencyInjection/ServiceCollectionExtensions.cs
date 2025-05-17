using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenInference.LLM.Telemetry.Core;
using OpenInference.LLM.Telemetry.Core.Models;
using OpenInference.LLM.Telemetry.Core.Utilities;
using OpenInference.LLM.Telemetry.Extensions.OpenTelemetry.Instrumentation;

namespace OpenInference.LLM.Telemetry.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for adding OpenInference LLM telemetry services to the DI container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds OpenInference LLM telemetry services to the DI container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
        public static IServiceCollection AddOpenInferenceTelemetry(this IServiceCollection services)
        {
            return AddOpenInferenceTelemetry(services, new LlmInstrumentationOptions());
        }
          /// <summary>
        /// Adds OpenInference LLM telemetry services to the DI container with the specified options.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="options">The options for LLM telemetry instrumentation.</param>
        /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
        public static IServiceCollection AddOpenInferenceTelemetry(this IServiceCollection services, LlmInstrumentationOptions options)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
                
            if (options == null)
                throw new ArgumentNullException(nameof(options));
                
            // Validate options
            OptionsValidator.ValidateOptionsAndThrow(options);

            // Configure global options
            LlmTelemetry.Configure(opt => 
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
                opt.HttpOperationTimeoutMs = options.HttpOperationTimeoutMs;
                opt.CaptureStructuredErrors = options.CaptureStructuredErrors;
                
                // Copy any default attributes
                foreach (var attr in options.DefaultAttributes)
                {
                    opt.DefaultAttributes[attr.Key] = attr.Value;
                }
            });
            
            // Register the text sanitizer as a singleton
            services.TryAddSingleton<ITextSanitizer>(options.TextSanitizer ?? new DefaultTextSanitizer());

            // Register the instrumentation options
            services.TryAddSingleton(options);
            
            // Register the LLM instrumentation
            services.TryAddSingleton<LlmInstrumentation>();
            
            return services;
        }
        
        /// <summary>
        /// Adds OpenInference LLM telemetry services to the DI container with the specified configuration.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configure">A delegate to configure the <see cref="LlmInstrumentationOptions"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
        public static IServiceCollection AddOpenInferenceTelemetry(this IServiceCollection services, Action<LlmInstrumentationOptions> configure)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
                
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
                
            var options = new LlmInstrumentationOptions();
            configure(options);
            
            return AddOpenInferenceTelemetry(services, options);
        }
    }
}
