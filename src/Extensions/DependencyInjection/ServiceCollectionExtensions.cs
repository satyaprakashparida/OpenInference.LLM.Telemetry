using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenInference.LLM.Telemetry.Core.Models;
using OpenInference.LLM.Telemetry.Extensions.HttpClient;
using OpenInference.LLM.Telemetry.Extensions.OpenTelemetry.Instrumentation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OpenInference.LLM.Telemetry.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for IServiceCollection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds OpenInference LLM telemetry services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configure">Optional action to configure instrumentation options</param>
        /// <returns>The updated service collection</returns>
        public static IServiceCollection AddOpenInferenceLlmTelemetry(
            this IServiceCollection services,
            Action<LlmInstrumentationOptions>? configure = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            
            var options = new LlmInstrumentationOptions();
            configure?.Invoke(options);
            
            // Apply global configuration
            LLMTelemetry.Configure(opts => {
                opts.EmitTextContent = options.EmitTextContent;
                opts.MaxTextLength = options.MaxTextLength;
                opts.EmitLatencyMetrics = options.EmitLatencyMetrics;
                opts.RecordModelName = options.RecordModelName;
                opts.RecordTokenUsage = options.RecordTokenUsage;
                opts.SanitizeSensitiveInfo = options.SanitizeSensitiveInfo;
            });
            
            services.TryAddSingleton(options);
            services.TryAddSingleton<LlmInstrumentation>();
            
            return services;
        }
        
        /// <summary>
        /// Adds OpenTelemetry with LLM tracing to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="serviceName">The name of the service for OpenTelemetry Resource</param>
        /// <param name="configureTracerProvider">Optional action to further configure the tracer provider</param>
        /// <returns>The updated service collection</returns>
        public static IServiceCollection AddOpenTelemetryWithLlmTracing(
            this IServiceCollection services,
            string serviceName,
            Action<TracerProviderBuilder>? configureTracerProvider = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (string.IsNullOrEmpty(serviceName)) throw new ArgumentException("Service name cannot be null or empty", nameof(serviceName));
            
            services.AddOpenTelemetry()
                .WithTracing(builder =>
                {
                    builder
                        .AddSource(LLMTelemetry.ActivitySource.Name)
                        .SetResourceBuilder(
                            ResourceBuilder.CreateDefault()
                                .AddService(serviceName)
                                .AddAttributes(new[] {
                                    new KeyValuePair<string, object>("service.instance.id", Environment.MachineName),
                                    new KeyValuePair<string, object>("service.namespace", "OpenInference.LLM.Telemetry")
                                }));
                    
                    configureTracerProvider?.Invoke(builder);
                });
            
            return services;
        }
        
        /// <summary>
        /// Adds Azure OpenAI client with LLM telemetry
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configureClient">Action to configure the HTTP client</param>
        /// <param name="modelName">The default model name to use</param>
        /// <returns>The HTTP client builder for further configuration</returns>
        public static IHttpClientBuilder AddAzureOpenAIClientWithTelemetry(
            this IServiceCollection services,
            Action<System.Net.Http.HttpClient> configureClient,
            string modelName = "unknown")
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configureClient == null) throw new ArgumentNullException(nameof(configureClient));
            
            return services.AddHttpClient("AzureOpenAI", configureClient)
                .AddLLMTelemetryWithDI(modelName, "azure");
        }
        
        /// <summary>
        /// Adds Semantic Kernel specific telemetry handlers to capture OpenInference telemetry from Semantic Kernel operations
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddSemanticKernelTelemetry(this IServiceCollection services)
        {
            // Register the base OpenInference telemetry services
            services.AddOpenInferenceLlmTelemetry();
            
            // Register Semantic Kernel specific services if needed
            // (Currently, the adapter is static so no registration is required)
            
            return services;
        }

        /// <summary>
        /// Configures OpenTelemetry tracing to include LLM telemetry
        /// </summary>
        /// <param name="builder">The TracerProviderBuilder</param>
        /// <returns>The TracerProviderBuilder for chaining</returns>
        public static TracerProviderBuilder AddLlmTelemetry(this TracerProviderBuilder builder)
        {
            return builder.AddSource(LLMTelemetry.ActivitySource.Name);
        }
    }
}