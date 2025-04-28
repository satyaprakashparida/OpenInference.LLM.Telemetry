using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenInference.LLM.Telemetry.Core.Models;

namespace OpenInference.LLM.Telemetry.Extensions.HttpClient
{
    /// <summary>
    /// Delegating handler that adds LLM telemetry to HTTP requests
    /// </summary>
    public class LlmTelemetryDelegatingHandler : DelegatingHandler
    {
        private readonly string _defaultModelName;
        private readonly string _provider;
        private readonly LlmInstrumentationOptions? _options;
        
        /// <summary>
        /// Creates a new LlmTelemetryDelegatingHandler with default options
        /// </summary>
        /// <param name="defaultModelName">Default model name to use if not detected in request</param>
        /// <param name="provider">LLM provider name (e.g., "azure", "openai")</param>
        public LlmTelemetryDelegatingHandler(string defaultModelName, string provider)
            : this(defaultModelName, provider, null)
        {
        }
        
        /// <summary>
        /// Creates a new LlmTelemetryDelegatingHandler with custom options
        /// </summary>
        /// <param name="defaultModelName">Default model name to use if not detected in request</param>
        /// <param name="provider">LLM provider name (e.g., "azure", "openai")</param>
        /// <param name="options">Custom instrumentation options</param>
        public LlmTelemetryDelegatingHandler(string defaultModelName, string provider, LlmInstrumentationOptions? options)
        {
            _defaultModelName = defaultModelName ?? throw new ArgumentNullException(nameof(defaultModelName));
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _options = options;
        }
        
        /// <summary>
        /// Sends an HTTP request with LLM telemetry
        /// </summary>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
                
            if (!IsLlmApiCall(request))
            {
                return await base.SendAsync(request, cancellationToken);
            }
            
            string? requestBody = null;
            if (request.Content != null && (_options?.EmitTextContent ?? true))
            {
                try
                {
                    requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
                    
                    // Create a clone of the content since ReadAsStringAsync can consume it
                    request.Content = new StringContent(
                        requestBody,
                        Encoding.UTF8,
                        request.Content.Headers.ContentType?.MediaType ?? "application/json");
                }
                catch (Exception ex)
                {
                    // Log error but continue with the request
                    System.Diagnostics.Debug.WriteLine($"Error reading request body: {ex.Message}");
                }
            }
            
            var modelName = ExtractModelName(request) ?? _defaultModelName;
            var taskType = DetermineTaskType(request);
            
            using var activity = LLMTelemetry.StartLLMActivity(
                modelName: modelName,
                prompt: requestBody ?? string.Empty,
                taskType: taskType,
                provider: _provider,
                options: _options);
                
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var response = await base.SendAsync(request, cancellationToken);
                stopwatch.Stop();
                
                string? responseBody = null;
                if (response.Content != null && (_options?.EmitTextContent ?? true))
                {
                    try
                    {
                        responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                        
                        // Create a clone of the content since ReadAsStringAsync can consume it
                        response.Content = new StringContent(
                            responseBody,
                            Encoding.UTF8,
                            response.Content.Headers.ContentType?.MediaType ?? "application/json");
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue with the response
                        System.Diagnostics.Debug.WriteLine($"Error reading response body: {ex.Message}");
                    }
                }
                
                LLMTelemetry.EndLLMActivity(
                    activity: activity,
                    response: responseBody ?? string.Empty,
                    isSuccess: response.IsSuccessStatusCode,
                    latencyMs: stopwatch.ElapsedMilliseconds,
                    options: _options);
                
                return response;
            }
            catch (Exception ex)
            {
                LLMTelemetry.RecordException(activity, ex);
                throw;
            }
        }
        
        private bool IsLlmApiCall(HttpRequestMessage request)
        {
            if (request?.RequestUri == null) return false;
            
            // Logic to determine if this is an LLM API call
            var path = request.RequestUri.PathAndQuery?.ToLowerInvariant();
            return path?.Contains("completions") == true || 
                   path?.Contains("chat") == true ||
                   path?.Contains("generate") == true;
        }
        
        private string DetermineTaskType(HttpRequestMessage request)
        {
            if (request?.RequestUri == null) return "unknown";
            
            var path = request.RequestUri.PathAndQuery?.ToLowerInvariant();
            
            if (path?.Contains("chat") == true) return "chat";
            if (path?.Contains("completions") == true) return "completion";
            if (path?.Contains("embeddings") == true) return "embedding";
            if (path?.Contains("generate") == true) return "generation";
            
            return "unknown";
        }
        
        private string? ExtractModelName(HttpRequestMessage request)
        {
            if (request?.RequestUri == null) return null;
            
            try
            {
                // Try to extract model name from URL path segments
                var segments = request.RequestUri.Segments;
                
                // For Azure OpenAI the pattern is often /deployments/{model}/...
                for (int i = 0; i < segments.Length - 1; i++)
                {
                    if (segments[i].Trim('/').Equals("deployments", StringComparison.OrdinalIgnoreCase))
                    {
                        return segments[i + 1].TrimEnd('/');
                    }
                }
            }
            catch
            {
                // Ignore extraction errors and fall back to default model name
            }
            
            return null;
        }
    }
}