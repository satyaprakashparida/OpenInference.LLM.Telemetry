using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenInference.LLM.Telemetry;
using OpenInference.LLM.Telemetry.Providers.SemanticKernel;

namespace SemanticKernelSample
{
    /// <summary>
    /// Sample application demonstrating OpenInference.LLM.Telemetry integration with Semantic Kernel
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main entry point for the application
        /// </summary>
        static async Task Main(string[] args)
        {
            Console.WriteLine("OpenInference.LLM.Telemetry with Semantic Kernel Sample");
            Console.WriteLine("==================================================");
            Console.WriteLine();

            // Configure global telemetry options
            LLMTelemetry.Configure(options => {
                options.EmitTextContent = true;
                options.MaxTextLength = 1000;
                options.EmitLatencyMetrics = true;
                options.RecordModelName = true;
            });

            // Create a kernel with a mock chat completion service
            var builder = Kernel.CreateBuilder();
            builder.Services.AddKeyedSingleton<IChatCompletionService>("mock", new MockChatCompletionService());
            var kernel = builder.Build();

            await RunChatCompletionSampleAsync(kernel);
            
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Runs a sample chat completion with Semantic Kernel while using OpenInference telemetry
        /// </summary>
        /// <param name="kernel">The Semantic Kernel instance</param>
        private static async Task RunChatCompletionSampleAsync(Kernel kernel)
        {
            try
            {
                Console.WriteLine("Running chat completion with OpenInference telemetry...");
                
                // Create a chat history
                var chatHistory = new ChatHistory();
                chatHistory.AddSystemMessage("You are a helpful AI assistant.");
                chatHistory.AddUserMessage("What are the key benefits of using telemetry in AI applications?");

                // Get the chat completion service
                var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
                
                // Record timing for accurate latency measurement
                var stopwatch = Stopwatch.StartNew();
                
                // Execute the chat completion
                var chatResult = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
                stopwatch.Stop();
                
                // Output the result
                Console.WriteLine($"Assistant: {chatResult.Content}");
                Console.WriteLine();
                
                // Track the operation with OpenInference telemetry
                // This is where the OpenInference telemetry magic happens!
                string modelName = "gpt-4";  // Default model name
                
                // Try to get the actual model name if possible
                if (chatCompletionService is MockChatCompletionService)
                {
                    if (chatCompletionService.Attributes.TryGetValue("ModelId", out var modelIdObj) && 
                        modelIdObj is string modelId)
                    {
                        modelName = modelId;
                    }
                }
                
                SemanticKernelAdapter.TrackChatCompletion(
                    chatHistory: chatHistory,
                    result: chatResult,
                    modelName: modelName,
                    latencyMs: stopwatch.ElapsedMilliseconds);
                
                Console.WriteLine("OpenInference telemetry has been recorded for this operation.");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Mock implementation of IChatCompletionService for demonstration purposes
    /// </summary>
    public class MockChatCompletionService : IChatCompletionService
    {
        /// <summary>
        /// Gets the attributes associated with this service
        /// </summary>
        public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>
        {
            ["ModelId"] = "mock-gpt-4"
        };

        /// <summary>
        /// Gets a chat message completion from the mock service
        /// </summary>
        public Task<ChatMessageContent> GetChatMessageContentAsync(
            ChatHistory chatHistory, 
            PromptExecutionSettings? executionSettings = null, 
            Kernel? kernel = null, 
            CancellationToken cancellationToken = default)
        {
            // Simulate some processing time
            Thread.Sleep(500);
            
            var response = "Telemetry in AI applications provides several key benefits:\n\n" +
                "1. Performance monitoring: Track response times, token usage, and system load\n" +
                "2. Cost management: Monitor API calls and token consumption for budget control\n" +
                "3. Quality assurance: Identify patterns in errors or poor responses\n" +
                "4. Usage analytics: Understand how users interact with your AI systems\n" +
                "5. Security monitoring: Detect unusual patterns that might indicate misuse\n" +
                "6. Compliance: Maintain audit trails for regulatory requirements\n\n" +
                "With standardized telemetry like OpenInference conventions, you also gain the ability to use common observability tools across different AI services.";
            
            var result = new ChatMessageContent(
                AuthorRole.Assistant,
                response,
                metadata: new Dictionary<string, object?>
                {
                    ["Usage"] = new OpenAIUsage
                    {
                        PromptTokens = 25,
                        CompletionTokens = 150,
                        TotalTokens = 175
                    }
                });
            
            return Task.FromResult(result);
        }

        /// <summary>
        /// Gets chat message contents from the mock service
        /// </summary>
        public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings? executionSettings = null,
            Kernel? kernel = null,
            CancellationToken cancellationToken = default)
        {
            var content = GetChatMessageContentAsync(chatHistory, executionSettings, kernel, cancellationToken).Result;
            return Task.FromResult<IReadOnlyList<ChatMessageContent>>(new List<ChatMessageContent> { content });
        }

        /// <summary>
        /// Gets streaming chat message content from the mock service
        /// </summary>
        public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings? executionSettings = null,
            Kernel? kernel = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("Streaming is not implemented in the mock service");
        }
    }

    /// <summary>
    /// OpenAI usage information to simulate token counts
    /// </summary>
    public class OpenAIUsage
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
}