using OpenInference.LLM.Telemetry.Core.Models;
using OpenTelemetry.Trace;
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace OpenInference.LLM.Telemetry.Extensions.OpenTelemetry
{
    /// <summary>
    /// Extensions for OpenTelemetry TracerProviderBuilder
    /// </summary>
    public static class TracerProviderBuilderExtensions
    {
        /// <summary>
        /// Adds LLM instrumentation to the TracerProviderBuilder
        /// </summary>
        /// <param name="builder">The TracerProviderBuilder instance</param>
        /// <returns>The updated TracerProviderBuilder</returns>
        public static TracerProviderBuilder AddLlmInstrumentation(
            this TracerProviderBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            
            return builder.AddSource(LLMTelemetry.ActivitySource.Name);
        }
        
        /// <summary>
        /// Adds LLM instrumentation to the TracerProviderBuilder with custom options
        /// </summary>
        /// <param name="builder">The TracerProviderBuilder instance</param>
        /// <param name="configure">Action to configure instrumentation options</param>
        /// <returns>The updated TracerProviderBuilder</returns>
        public static TracerProviderBuilder AddLlmInstrumentation(
            this TracerProviderBuilder builder,
            Action<LlmInstrumentationOptions> configure)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            
            var options = new LlmInstrumentationOptions();
            configure(options);
            
            // Apply configuration to the global LLM telemetry
            LLMTelemetry.Configure(opts => 
            {
                opts.EmitTextContent = options.EmitTextContent;
                opts.MaxTextLength = options.MaxTextLength;
                opts.EmitLatencyMetrics = options.EmitLatencyMetrics;
                opts.RecordModelName = options.RecordModelName;
                opts.RecordTokenUsage = options.RecordTokenUsage;
                opts.SanitizeSensitiveInfo = options.SanitizeSensitiveInfo;
            });
            
            return builder.AddSource(LLMTelemetry.ActivitySource.Name);
        }
        
        /// <summary>
        /// Adds a processor that will enrich LLM spans with additional attributes
        /// </summary>
        /// <param name="builder">The TracerProviderBuilder instance</param>
        /// <returns>The updated TracerProviderBuilder</returns>
        public static TracerProviderBuilder AddLlmEnrichment(
            this TracerProviderBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            
            // Add a processor that adds some additional context to LLM spans
            builder.AddProcessor(new SimpleActivityProcessor(activity =>
            {
                // Only process LLM spans
                if (activity.Source.Name == LLMTelemetry.ActivitySource.Name)
                {
                    // Add process information for diagnostics
                    activity.SetTag("process.runtime", "dotnet");
                    activity.SetTag("process.runtime.version", Environment.Version.ToString());
                    
                    // Add OpenInference specific metadata
                    activity.SetTag("openinference.name", typeof(LLMTelemetry).Assembly.GetName().Name);
                    activity.SetTag("openinference.version", typeof(LLMTelemetry).Assembly.GetName().Version?.ToString());
                }
                
                return true; // Continue with other processors
            }));
            
            return builder;
        }
    }
    
    /// <summary>
    /// Simple activity processor for customizing activities
    /// </summary>
    internal class SimpleActivityProcessor : global::OpenTelemetry.BaseProcessor<Activity>
    {
        private readonly Func<Activity, bool> _processAction;
        
        public SimpleActivityProcessor(Func<Activity, bool> processAction)
        {
            _processAction = processAction ?? throw new ArgumentNullException(nameof(processAction));
        }
        
        public override void OnStart(Activity activity)
        {
            if (activity != null)
            {
                _processAction(activity);
            }
        }
    }
}