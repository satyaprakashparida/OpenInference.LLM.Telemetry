using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using OpenInference.LLM.Telemetry.Core.Models;

namespace OpenInference.LLM.Telemetry.Providers.SemanticKernel
{
    /// <summary>
    /// Adapter for Microsoft Semantic Kernel to provide OpenInference telemetry conventions.
    /// This adapter brings OpenInference semantic conventions to Semantic Kernel operations.
    /// </summary>
    public static class SemanticKernelAdapter
    {
        /// <summary>
        /// Tracks a chat completion operation using Semantic Kernel with OpenInference conventions
        /// </summary>
        /// <param name="chatHistory">The chat history with prompt messages</param>
        /// <param name="result">The chat completion result</param>
        /// <param name="modelName">The name of the model being used</param>
        /// <param name="latencyMs">The latency in milliseconds</param>
        /// <param name="instrumentationOptions">Optional telemetry instrumentation options</param>
        public static void TrackChatCompletion(
            ChatHistory chatHistory,
            ChatMessageContent result,
            string modelName,
            long latencyMs,
            LlmInstrumentationOptions? instrumentationOptions = null)
        {
            if (result == null) return;

            // Build a consolidated prompt string
            var promptBuilder = new StringBuilder();
            foreach (var message in chatHistory)
            {
                promptBuilder.AppendLine($"[{message.Role}]: {message.Content}");
            }

            using var activity = LlmTelemetry.StartLlmActivity(
                modelName: modelName,
                prompt: promptBuilder.ToString(),
                taskType: "chat",
                provider: "semantic_kernel",
                options: instrumentationOptions);

            LlmTelemetry.EndLlmActivity(
                activity: activity,
                response: result.Content ?? string.Empty,
                isSuccess: true,
                latencyMs: latencyMs,
                options: instrumentationOptions);

            // Add token information if available
            if (activity != null && result.Metadata != null)
            {
                var tokenInfo = GetTokenInformation(result);
                if (tokenInfo != null)
                {
                    var (promptTokens, completionTokens, totalTokens) = tokenInfo.Value;

                    if (promptTokens.HasValue)
                        activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_PROMPT, promptTokens.Value);
                    
                    if (completionTokens.HasValue)
                        activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_COMPLETION, completionTokens.Value);
                    
                    if (totalTokens.HasValue)
                        activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_TOTAL, totalTokens.Value);

                    // Add OpenInference specific telemetry attributes
                    activity.SetTag("openinference.span.kind", "llm");
                    activity.SetTag("openinference.llm.input_tokens", promptTokens ?? 0);
                    activity.SetTag("openinference.llm.output_tokens", completionTokens ?? 0);
                    activity.SetTag("openinference.llm.total_tokens", totalTokens ?? 0);
                }
            }
        }

        /// <summary>
        /// Tracks a text completion operation using Semantic Kernel with OpenInference conventions
        /// </summary>
        /// <param name="prompt">The prompt text sent to the model</param>
        /// <param name="result">The text completion result</param>
        /// <param name="modelName">The name of the model being used</param>
        /// <param name="latencyMs">The latency in milliseconds</param>
        /// <param name="instrumentationOptions">Optional telemetry instrumentation options</param>
        public static void TrackTextCompletion(
            string prompt,
            TextContent result,
            string modelName,
            long latencyMs,
            LlmInstrumentationOptions? instrumentationOptions = null)
        {
            if (result == null) return;

            using var activity = LlmTelemetry.StartLlmActivity(
                modelName: modelName,
                prompt: prompt,
                taskType: "completion",
                provider: "semantic_kernel", 
                options: instrumentationOptions);

            LlmTelemetry.EndLlmActivity(
                activity: activity,
                response: result.Text ?? string.Empty,
                isSuccess: true,
                latencyMs: latencyMs,
                options: instrumentationOptions);

            // Add token information if available
            if (activity != null && result.Metadata != null)
            {
                var tokenInfo = GetTokenInformation(result);
                if (tokenInfo != null)
                {
                    var (promptTokens, completionTokens, totalTokens) = tokenInfo.Value;

                    if (promptTokens.HasValue)
                        activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_PROMPT, promptTokens.Value);
                    
                    if (completionTokens.HasValue)
                        activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_COMPLETION, completionTokens.Value);
                    
                    if (totalTokens.HasValue)
                        activity.SetTag(Core.SemanticConventions.LLM_TOKEN_COUNT_TOTAL, totalTokens.Value);

                    // Add OpenInference specific telemetry attributes
                    activity.SetTag("openinference.span.kind", "llm");
                    activity.SetTag("openinference.llm.input_tokens", promptTokens ?? 0);
                    activity.SetTag("openinference.llm.output_tokens", completionTokens ?? 0);
                    activity.SetTag("openinference.llm.total_tokens", totalTokens ?? 0);
                }
            }
        }

        /// <summary>
        /// Creates an LlmOperationData object from a chat completion result
        /// </summary>
        /// <param name="chatHistory">The chat history with prompt messages</param>
        /// <param name="result">The chat completion result</param>
        /// <param name="modelName">The name of the model being used</param>
        /// <param name="latencyMs">The latency in milliseconds</param>
        /// <param name="isSuccess">Whether the operation was successful</param>
        /// <param name="errorMessage">Optional error message if the operation failed</param>
        /// <returns>An LlmOperationData object containing operation details</returns>
        public static LlmOperationData? CreateChatCompletionData(
            ChatHistory chatHistory,
            ChatMessageContent? result,
            string modelName,
            long latencyMs,
            bool isSuccess = true,
            string? errorMessage = null)
        {
            if (result == null) return null;

            // Build a consolidated prompt string
            var promptBuilder = new StringBuilder();
            foreach (var message in chatHistory)
            {
                promptBuilder.AppendLine($"[{message.Role}]: {message.Content}");
            }

            var tokenInfo = GetTokenInformation(result);
            
            int? promptTokens = null;
            int? completionTokens = null;
            int? totalTokens = null;
            
            if (tokenInfo.HasValue)
            {
                promptTokens = tokenInfo.Value.PromptTokens;
                completionTokens = tokenInfo.Value.CompletionTokens;
                totalTokens = tokenInfo.Value.TotalTokens;
            }

            return new LlmOperationData
            {
                ModelName = modelName,
                Prompt = promptBuilder.ToString(),
                Response = result.Content,
                TaskType = "chat",
                Provider = "semantic_kernel",
                IsSuccess = isSuccess,
                LatencyMs = latencyMs,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                TotalTokens = totalTokens,
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Creates an LlmOperationData object from a text completion result
        /// </summary>
        /// <param name="prompt">The prompt text</param>
        /// <param name="result">The text completion result</param>
        /// <param name="modelName">The name of the model being used</param>
        /// <param name="latencyMs">The latency in milliseconds</param>
        /// <param name="isSuccess">Whether the operation was successful</param>
        /// <param name="errorMessage">Optional error message if the operation failed</param>
        /// <returns>An LlmOperationData object containing operation details</returns>
        public static LlmOperationData? CreateTextCompletionData(
            string prompt,
            TextContent? result,
            string modelName,
            long latencyMs,
            bool isSuccess = true,
            string? errorMessage = null)
        {
            if (result == null) return null;

            var tokenInfo = GetTokenInformation(result);
            
            int? promptTokens = null;
            int? completionTokens = null;
            int? totalTokens = null;
            
            if (tokenInfo.HasValue)
            {
                promptTokens = tokenInfo.Value.PromptTokens;
                completionTokens = tokenInfo.Value.CompletionTokens;
                totalTokens = tokenInfo.Value.TotalTokens;
            }

            return new LlmOperationData
            {
                ModelName = modelName,
                Prompt = prompt,
                Response = result.Text,
                TaskType = "completion",
                Provider = "semantic_kernel",
                IsSuccess = isSuccess,
                LatencyMs = latencyMs,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                TotalTokens = totalTokens,
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Represents usage information from OpenAI models
        /// </summary>
        private class OpenAIUsage
        {
            /// <summary>
            /// Gets or sets the number of prompt tokens
            /// </summary>
            public int PromptTokens { get; set; }
            
            /// <summary>
            /// Gets or sets the number of completion tokens
            /// </summary>
            public int CompletionTokens { get; set; }
            
            /// <summary>
            /// Gets or sets the total number of tokens
            /// </summary>
            public int TotalTokens { get; set; }
        }

        /// <summary>
        /// Extracts token count information from the result's metadata
        /// </summary>
        /// <param name="result">The chat message content result</param>
        /// <returns>Tuple containing prompt, completion, and total token counts</returns>
        private static (int? PromptTokens, int? CompletionTokens, int? TotalTokens)? GetTokenInformation(ChatMessageContent result)
        {
            if (result.Metadata == null) return null;

            // Try to extract token usage for OpenAI responses
            if (result.Metadata.TryGetValue("Usage", out var usageObj) && 
                usageObj is OpenAIUsage usage)
            {
                return (
                    usage.PromptTokens,
                    usage.CompletionTokens,
                    usage.TotalTokens
                );
            }

            // Try to extract token count through individual metadata items
            int? promptTokens = null;
            int? completionTokens = null;
            int? totalTokens = null;

            if (result.Metadata.TryGetValue("PromptTokenCount", out var promptTokenObj) && 
                promptTokenObj is int promptTokenCount)
            {
                promptTokens = promptTokenCount;
            }

            if (result.Metadata.TryGetValue("CompletionTokenCount", out var completionTokenObj) && 
                completionTokenObj is int completionTokenCount)
            {
                completionTokens = completionTokenCount;
            }

            if (result.Metadata.TryGetValue("TotalTokenCount", out var totalTokenObj) && 
                totalTokenObj is int totalTokenCount)
            {
                totalTokens = totalTokenCount;
            }

            if (promptTokens != null || completionTokens != null || totalTokens != null)
            {
                return (promptTokens, completionTokens, totalTokens);
            }

            return null;
        }

        /// <summary>
        /// Extracts token count information from the result's metadata
        /// </summary>
        /// <param name="result">The text content result</param>
        /// <returns>Tuple containing prompt, completion, and total token counts</returns>
        private static (int? PromptTokens, int? CompletionTokens, int? TotalTokens)? GetTokenInformation(TextContent result)
        {
            if (result.Metadata == null) return null;

            // Try to extract token usage for OpenAI responses
            if (result.Metadata.TryGetValue("Usage", out var usageObj) && 
                usageObj is OpenAIUsage usage)
            {
                return (
                    usage.PromptTokens,
                    usage.CompletionTokens,
                    usage.TotalTokens
                );
            }

            // Try to extract token count through individual metadata items
            int? promptTokens = null;
            int? completionTokens = null;
            int? totalTokens = null;

            if (result.Metadata.TryGetValue("PromptTokenCount", out var promptTokenObj) && 
                promptTokenObj is int promptTokenCount)
            {
                promptTokens = promptTokenCount;
            }

            if (result.Metadata.TryGetValue("CompletionTokenCount", out var completionTokenObj) && 
                completionTokenObj is int completionTokenCount)
            {
                completionTokens = completionTokenCount;
            }

            if (result.Metadata.TryGetValue("TotalTokenCount", out var totalTokenObj) && 
                totalTokenObj is int totalTokenCount)
            {
                totalTokens = totalTokenCount;
            }

            if (promptTokens != null || completionTokens != null || totalTokens != null)
            {
                return (promptTokens, completionTokens, totalTokens);
            }

            return null;
        }

        /// <summary>
        /// Extension method for Semantic Kernel's chat completion with telemetry tracking.
        /// </summary>
        /// <param name="chatCompletionService">The chat completion service.</param>
        /// <param name="chatHistory">The chat history containing the conversation.</param>
        /// <param name="settings">Optional chat completion request settings.</param>
        /// <param name="kernel">Optional kernel instance.</param>
        /// <param name="modelName">The model name used for the chat completion.</param>
        /// <param name="instrumentationOptions">Optional instrumentation options for telemetry.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The chat message response with telemetry tracked.</returns>
        public static async Task<ChatMessageContent> GetChatCompletionWithTelemetryAsync(
            this IChatCompletionService chatCompletionService,
            ChatHistory chatHistory,
            PromptExecutionSettings? settings = null,
            Kernel? kernel = null,
            string modelName = "unknown",
            LlmInstrumentationOptions? instrumentationOptions = null,
            CancellationToken cancellationToken = default)
        {
            if (chatCompletionService == null)
                throw new ArgumentNullException(nameof(chatCompletionService));
            
            if (chatHistory == null)
                throw new ArgumentNullException(nameof(chatHistory));

            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Fixed method call to match the correct Semantic Kernel API signature
                var result = await chatCompletionService.GetChatMessageContentAsync(
                    chatHistory: chatHistory, 
                    executionSettings: settings,
                    kernel: kernel,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                
                stopwatch.Stop();
                
                // Track the operation
                TrackChatCompletion(
                    chatHistory,
                    result,
                    modelName,
                    stopwatch.ElapsedMilliseconds,
                    instrumentationOptions);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Build a consolidated prompt string for error tracking
                var promptBuilder = new StringBuilder();
                foreach (var message in chatHistory)
                {
                    promptBuilder.AppendLine($"[{message.Role}]: {message.Content}");
                }
                
                // Track the failed operation
                var activity = LlmTelemetry.StartLlmActivity(
                    modelName: modelName,
                    prompt: promptBuilder.ToString(),
                    taskType: "chat",
                    provider: "semantic_kernel",
                    options: instrumentationOptions);
                
                if (activity != null)
                {
                    LlmTelemetry.RecordException(activity, ex);
                    
                    LlmTelemetry.EndLlmActivity(
                        activity: activity,
                        response: string.Empty,
                        isSuccess: false,
                        latencyMs: stopwatch.ElapsedMilliseconds,
                        options: instrumentationOptions);
                }
                
                throw;
            }
        }

        /// <summary>
        /// Extension method for Semantic Kernel's text completion with telemetry tracking.
        /// </summary>
        /// <param name="textGenerationService">The text generation service.</param>
        /// <param name="prompt">The text prompt for completion.</param>
        /// <param name="settings">Optional text generation request settings.</param>
        /// <param name="kernel">Optional kernel instance.</param>
        /// <param name="modelName">The model name used for the text completion.</param>
        /// <param name="instrumentationOptions">Optional instrumentation options for telemetry.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The text generation response with telemetry tracked.</returns>
        public static async Task<TextContent> GetTextContentWithTelemetryAsync(
            this ITextGenerationService textGenerationService,
            string prompt,
            PromptExecutionSettings? settings = null,
            Kernel? kernel = null,
            string modelName = "unknown",
            LlmInstrumentationOptions? instrumentationOptions = null,
            CancellationToken cancellationToken = default)
        {
            if (textGenerationService == null)
                throw new ArgumentNullException(nameof(textGenerationService));
            
            if (string.IsNullOrEmpty(prompt))
                throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));

            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Fixed method call to match the correct Semantic Kernel API signature
                var result = await textGenerationService.GetTextContentAsync(
                    prompt: prompt,
                    executionSettings: settings, 
                    kernel: kernel,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                
                stopwatch.Stop();
                
                // Track the operation
                TrackTextCompletion(
                    prompt,
                    result,
                    modelName,
                    stopwatch.ElapsedMilliseconds,
                    instrumentationOptions);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Track the failed operation
                var activity = LlmTelemetry.StartLlmActivity(
                    modelName: modelName,
                    prompt: prompt,
                    taskType: "completion",
                    provider: "semantic_kernel",
                    options: instrumentationOptions);
                
                if (activity != null)
                {
                    LlmTelemetry.RecordException(activity, ex);
                    
                    LlmTelemetry.EndLlmActivity(
                        activity: activity,
                        response: string.Empty,
                        isSuccess: false,
                        latencyMs: stopwatch.ElapsedMilliseconds,
                        options: instrumentationOptions);
                }
                
                throw;
            }
        }

        /// <summary>
        /// Extension method for Semantic Kernel's stream chat completion with telemetry tracking.
        /// </summary>
        /// <param name="chatCompletionService">The chat completion service.</param>
        /// <param name="chatHistory">The chat history containing the conversation.</param>
        /// <param name="modelName">The model name used for the chat completion.</param>
        /// <param name="settings">Optional chat completion request settings.</param>
        /// <param name="instrumentationOptions">Optional instrumentation options for telemetry.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The streaming chat message content with telemetry tracked on completion.</returns>
        public static IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatCompletionsWithTelemetryAsync(
            this IChatCompletionService chatCompletionService,
            ChatHistory chatHistory,
            string modelName,
            PromptExecutionSettings? settings = null,
            LlmInstrumentationOptions? instrumentationOptions = null,
            CancellationToken cancellationToken = default)
        {
            if (chatCompletionService == null)
                throw new ArgumentNullException(nameof(chatCompletionService));
            
            if (chatHistory == null)
                throw new ArgumentNullException(nameof(chatHistory));

            // Create a wrapper class to capture all streamed content and track telemetry at the end
            return new StreamingChatCompletionTelemetryWrapper(
                chatCompletionService,
                chatHistory,
                modelName,
                settings,
                instrumentationOptions,
                cancellationToken).GetStreamingContentAsync();
        }

        /// <summary>
        /// A wrapper class to handle streaming chat completions with telemetry
        /// </summary>
        private class StreamingChatCompletionTelemetryWrapper
        {
            private readonly IChatCompletionService _chatCompletionService;
            private readonly ChatHistory _chatHistory;
            private readonly string _modelName;
            private readonly PromptExecutionSettings? _settings;
            private readonly LlmInstrumentationOptions? _instrumentationOptions;
            private readonly CancellationToken _cancellationToken;
            private readonly StringBuilder _resultBuilder = new();
            private readonly Stopwatch _stopwatch = new();
            private ChatMessageContent? _finalContent;

            public StreamingChatCompletionTelemetryWrapper(
                IChatCompletionService chatCompletionService,
                ChatHistory chatHistory,
                string modelName,
                PromptExecutionSettings? settings,
                LlmInstrumentationOptions? instrumentationOptions,
                CancellationToken cancellationToken)
            {
                _chatCompletionService = chatCompletionService;
                _chatHistory = chatHistory;
                _modelName = modelName;
                _settings = settings;
                _instrumentationOptions = instrumentationOptions;
                _cancellationToken = cancellationToken;
            }

            public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingContentAsync(
                [EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                _stopwatch.Start();
                IAsyncEnumerator<StreamingChatMessageContent>? enumerator = null;
                
                try
                {
                    // Use the passed cancellation token (which is decorated with EnumeratorCancellation)
                    // Combined with _cancellationToken from constructor using a linked token source
                    using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken, cancellationToken);
                    var effectiveToken = linkedTokenSource.Token;
                    
                    // Get the streaming content
                    var streamingContent = _chatCompletionService.GetStreamingChatMessageContentsAsync(
                        chatHistory: _chatHistory,
                        executionSettings: _settings,
                        kernel: null,
                        cancellationToken: effectiveToken);
                        
                    enumerator = streamingContent.GetAsyncEnumerator(effectiveToken);
                }
                catch (Exception ex)
                {
                    _stopwatch.Stop();
                    HandleStreamingException(ex);
                    throw;
                }
                
                // Process the streaming content
                if (enumerator != null)
                {
                    try
                    {
                        while (true)
                        {
                            bool hasNext;
                            StreamingChatMessageContent content;
                            
                            try
                            {
                                hasNext = await enumerator.MoveNextAsync();
                                if (!hasNext) break;
                                
                                content = enumerator.Current;
                            }
                            catch (Exception ex)
                            {
                                _stopwatch.Stop();
                                HandleStreamingException(ex);
                                throw;
                            }
                            
                            // This yield is not within a try-catch block
                            _resultBuilder.Append(content.Content);
                            _finalContent = new ChatMessageContent(
                                role: content.Role ?? AuthorRole.Assistant,
                                content: _resultBuilder.ToString());
                                
                            yield return content;
                        }
                        
                        _stopwatch.Stop();
                        
                        // Record telemetry after all content is streamed
                        if (_finalContent != null)
                        {
                            TrackChatCompletion(
                                _chatHistory,
                                _finalContent,
                                _modelName,
                                _stopwatch.ElapsedMilliseconds,
                                _instrumentationOptions);
                        }
                    }
                    finally
                    {
                        // Properly dispose the enumerator
                        await enumerator.DisposeAsync();
                    }
                }
            }
            
            // Helper method to handle exceptions from streaming
            private void HandleStreamingException(Exception ex)
            {
                // Build a consolidated prompt string for error tracking
                var promptBuilder = new StringBuilder();
                foreach (var message in _chatHistory)
                {
                    promptBuilder.AppendLine($"[{message.Role}]: {message.Content}");
                }
                
                // Track the failed operation
                var activity = LlmTelemetry.StartLlmActivity(
                    modelName: _modelName,
                    prompt: promptBuilder.ToString(),
                    taskType: "chat",
                    provider: "semantic_kernel",
                    options: _instrumentationOptions);
                
                if (activity != null)
                {
                    LlmTelemetry.RecordException(activity, ex);
                    
                    LlmTelemetry.EndLlmActivity(
                        activity: activity,
                        response: _resultBuilder.ToString(),
                        isSuccess: false,
                        latencyMs: _stopwatch.ElapsedMilliseconds,
                        options: _instrumentationOptions);
                }
            }
        }
    }
}
