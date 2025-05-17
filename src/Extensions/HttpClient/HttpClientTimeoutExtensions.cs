using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenInference.LLM.Telemetry.Core.Models;
using OpenInference.LLM.Telemetry.Core.Utilities;

namespace OpenInference.LLM.Telemetry.Extensions.HttpClient
{
    /// <summary>
    /// Extension methods for configuring HttpClient instances with improved LLM telemetry.
    /// </summary>
    public static class HttpClientTimeoutExtensions
    {
        /// <summary>
        /// Adds an HTTP client with timeout support based on LLM instrumentation options.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="name">The name of the client.</param>
        /// <param name="defaultModelName">The default model name to use for telemetry.</param>
        /// <param name="provider">The provider name (e.g., "azure", "openai").</param>
        /// <param name="options">The LLM instrumentation options.</param>
        /// <returns>An IHttpClientBuilder that can be used to configure the client.</returns>
        public static IHttpClientBuilder AddLlmHttpClientWithTimeout(
            this IServiceCollection services,
            string name,
            string defaultModelName,
            string provider,
            LlmInstrumentationOptions options)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
                
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
                
            if (string.IsNullOrEmpty(defaultModelName))
                throw new ArgumentNullException(nameof(defaultModelName));
                
            if (string.IsNullOrEmpty(provider))
                throw new ArgumentNullException(nameof(provider));
                
            if (options == null)
                throw new ArgumentNullException(nameof(options));            // Validate options
            OptionsValidator.ValidateOptionsAndThrow(options);
                
            return services.AddHttpClient(name)
                .AddHttpMessageHandler(() => new LlmTelemetryDelegatingHandler(defaultModelName, provider, options))
                .ConfigureHttpClient(client => ConfigureTimeoutFromOptions(client, options));
        }
        
        /// <summary>
        /// Configures an HttpClient with timeout settings from LLM instrumentation options.
        /// </summary>
        /// <param name="client">The HttpClient to configure.</param>
        /// <param name="options">The LLM instrumentation options.</param>
        /// <returns>The configured HttpClient for chaining.</returns>
        public static System.Net.Http.HttpClient ConfigureTimeoutFromOptions(
            this System.Net.Http.HttpClient client,
            LlmInstrumentationOptions options)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
                
            if (options == null)
                throw new ArgumentNullException(nameof(options));
                
            // Apply timeout from options if specified
            if (options.HttpOperationTimeoutMs > 0)
            {
                client.Timeout = TimeSpan.FromMilliseconds(options.HttpOperationTimeoutMs);
            }
            
            return client;
        }
    }
}
