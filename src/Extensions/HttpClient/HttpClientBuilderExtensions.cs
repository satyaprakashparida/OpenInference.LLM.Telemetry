using Microsoft.Extensions.DependencyInjection;
using OpenInference.LLM.Telemetry.Core.Models;

namespace OpenInference.LLM.Telemetry.Extensions.HttpClient
{
    /// <summary>
    /// Extension methods for IHttpClientBuilder
    /// </summary>
    public static class HttpClientBuilderExtensions
    {
        /// <summary>
        /// Adds LLM telemetry to an HTTP client
        /// </summary>
        /// <param name="builder">The IHttpClientBuilder instance</param>
        /// <param name="defaultModelName">Default model name to use if not detected in request</param>
        /// <param name="provider">LLM provider name (e.g., "azure", "openai")</param>
        /// <returns>The modified IHttpClientBuilder</returns>
        public static IHttpClientBuilder AddLlmTelemetry(
            this IHttpClientBuilder builder,
            string defaultModelName,
            string provider)
        {
            return builder.AddLlmTelemetry(defaultModelName, provider, null);
        }
        
        /// <summary>
        /// Adds LLM telemetry to an HTTP client with custom options
        /// </summary>
        /// <param name="builder">The IHttpClientBuilder instance</param>
        /// <param name="defaultModelName">Default model name to use if not detected in request</param>
        /// <param name="provider">LLM provider name (e.g., "azure", "openai")</param>
        /// <param name="options">Custom instrumentation options</param>
        /// <returns>The modified IHttpClientBuilder</returns>
        public static IHttpClientBuilder AddLlmTelemetry(
            this IHttpClientBuilder builder,
            string defaultModelName,
            string provider,
            LlmInstrumentationOptions? options)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (string.IsNullOrEmpty(defaultModelName)) throw new ArgumentException("Model name cannot be null or empty", nameof(defaultModelName));
            if (string.IsNullOrEmpty(provider)) throw new ArgumentException("Provider name cannot be null or empty", nameof(provider));
            
            return builder.AddHttpMessageHandler(sp => 
                new LlmTelemetryDelegatingHandler(defaultModelName, provider, options));
        }
        
        /// <summary>
        /// Adds LLM telemetry to an HTTP client using options from dependency injection
        /// </summary>
        /// <param name="builder">The IHttpClientBuilder instance</param>
        /// <param name="defaultModelName">Default model name to use if not detected in request</param>
        /// <param name="provider">LLM provider name (e.g., "azure", "openai")</param>
        /// <returns>The modified IHttpClientBuilder</returns>
        public static IHttpClientBuilder AddLlmTelemetryWithDI(
            this IHttpClientBuilder builder,
            string defaultModelName,
            string provider)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (string.IsNullOrEmpty(defaultModelName)) throw new ArgumentException("Model name cannot be null or empty", nameof(defaultModelName));
            if (string.IsNullOrEmpty(provider)) throw new ArgumentException("Provider name cannot be null or empty", nameof(provider));
            
            return builder.AddHttpMessageHandler(sp => 
            {
                var options = sp.GetService<LlmInstrumentationOptions>();
                return new LlmTelemetryDelegatingHandler(defaultModelName, provider, options);
            });
        }
    }
}
