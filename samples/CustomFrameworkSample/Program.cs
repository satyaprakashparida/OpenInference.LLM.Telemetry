using System.Diagnostics;
using OpenInference.LLM.Telemetry;
using OpenInference.LLM.Telemetry.Core.Models;
using OpenInference.LLM.Telemetry.Providers.Generic;

namespace CustomFrameworkSample
{
    /// <summary>
    /// Sample application demonstrating how to use OpenInference.LLM.Telemetry
    /// with any custom LLM orchestration framework.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main entry point for the application
        /// </summary>
        static async Task Main(string[] args)
        {
            Console.WriteLine("OpenInference.LLM.Telemetry with Custom Framework Sample");
            Console.WriteLine("====================================================");
            Console.WriteLine();

            // Configure global telemetry options
            LLMTelemetry.Configure(options => {
                options.EmitTextContent = true;
                options.MaxTextLength = 1000;
                options.EmitLatencyMetrics = true;
                options.RecordModelName = true;
            });

            // Create our custom services
            var llmService = new CustomLlmService();
            var agent = new MultiStepAgent();
            
            // Example 1: Simple LLM call with OpenInference telemetry
            await llmService.GenerateResponseAsync(
                "Explain the importance of standardized telemetry in LLM applications");
            Console.WriteLine();

            // Example 2: Multi-step chain with OpenInference telemetry
            await agent.RunMultiStepWorkflowAsync(
                "Create a comparison between OpenTelemetry and OpenInference standards");
            
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// Simple implementation of a custom LLM service
    /// This could represent any custom wrapper around LLM APIs like Azure OpenAI, 
    /// Anthropic, or any other model provider
    /// </summary>
    public class CustomLlmService
    {
        private readonly string _modelName = "gpt-4";
        private readonly string _provider = "azure";

        /// <summary>
        /// Generates a response for the given prompt using a simulated LLM
        /// </summary>
        public async Task<string> GenerateResponseAsync(string prompt)
        {
            Console.WriteLine($"Generating response for prompt: {prompt}");

            // In a real service, this would call an actual LLM API
            var stopwatch = Stopwatch.StartNew();
            
            // Simulate API latency
            await Task.Delay(800);
            
            // Mock response
            string response = "Standardized telemetry in LLM applications is crucial for several reasons:\n\n" +
                "1. Performance optimization: By tracking model latency, token usage, and other metrics consistently, you can identify bottlenecks and optimize accordingly.\n\n" +
                "2. Cost management: With standardized usage metrics, you can accurately track and predict API costs across different models and providers.\n\n" +
                "3. Reliability monitoring: Common telemetry standards help detect and respond to issues across the entire LLM stack.\n\n" +
                "4. Cross-platform analytics: Using standards like OpenInference allows for unified dashboards and tooling regardless of which models or frameworks you're using.\n\n" +
                "5. Governance and compliance: Standard telemetry formats make it easier to generate consistent audit trails and usage reports required by regulators.";
            
            stopwatch.Stop();
            
            // Generate mock token counts
            var tokenUsage = new Dictionary<string, int>
            {
                { "prompt_tokens", 15 },
                { "completion_tokens", 170 },
                { "total_tokens", 185 }
            };

            // This is where we record OpenInference telemetry for the operation
            GenericLlmAdapter.TrackLlmOperation(
                prompt: prompt,
                response: response,
                modelName: _modelName,
                latencyMs: stopwatch.ElapsedMilliseconds,
                taskType: "completion",
                provider: _provider, 
                tokenUsage: tokenUsage);
            
            Console.WriteLine($"Response generated in {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine(response);
            return response;
        }
    }

    /// <summary>
    /// Implementation of a multi-step agent that demonstrates tracking a chain of LLM operations
    /// with OpenInference telemetry. This pattern could be used with frameworks like LangChain,
    /// PromptFlow, or any custom orchestration framework.
    /// </summary>
    public class MultiStepAgent
    {
        private readonly string _modelName = "gpt-4";
        private readonly string _provider = "azure";

        /// <summary>
        /// Runs a multi-step workflow that uses multiple LLM calls and tracks
        /// the entire operation with OpenInference telemetry
        /// </summary>
        public async Task<string> RunMultiStepWorkflowAsync(string userQuery)
        {
            Console.WriteLine($"Starting multi-step workflow for: {userQuery}");
            
            var chainSteps = new List<LlmOperationData>();
            var stopwatch = Stopwatch.StartNew();
            
            // Step 1: Generate research points
            var step1Result = await ExecuteStepAsync(
                "Step 1: Generate key points for research",
                $"Generate 3-5 key comparison points between OpenTelemetry and OpenInference standards based on the query: {userQuery}",
                chainSteps);
            
            // Step 2: Elaborate on each point
            var step2Result = await ExecuteStepAsync(
                "Step 2: Elaborate on comparison points",
                $"Elaborate on each of these comparison points with more details:\n\n{step1Result}",
                chainSteps);
            
            // Step 3: Format the final output
            var finalResult = await ExecuteStepAsync(
                "Step 3: Format final response",
                $"Format the following comparison into a well-structured response with introduction and conclusion:\n\n{step2Result}",
                chainSteps);
            
            stopwatch.Stop();
            
            // Track the entire chain as a single operation with multiple steps
            GenericLlmAdapter.TrackLlmChain(
                operationName: "ComparisonWorkflow",
                chainSteps: chainSteps,
                totalLatencyMs: stopwatch.ElapsedMilliseconds,
                additionalAttributes: new Dictionary<string, object>
                {
                    ["openinference.workflow.user_query"] = userQuery,
                    ["openinference.workflow.step_count"] = chainSteps.Count
                });
            
            Console.WriteLine($"Multi-step workflow completed in {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine("Final result:");
            Console.WriteLine(finalResult);
            return finalResult;
        }
        
        /// <summary>
        /// Executes a single step in the workflow and tracks its telemetry
        /// </summary>
        private async Task<string> ExecuteStepAsync(
            string stepName,
            string prompt,
            List<LlmOperationData> chainSteps)
        {
            Console.WriteLine($"Executing {stepName}");
            
            var stopwatch = Stopwatch.StartNew();
            
            // Simulate API call with some delay
            await Task.Delay(500); 
            
            // Generate mock response
            string response = $"Response for {stepName}: {Guid.NewGuid()}\n" +
                "This is a simulated response that would normally come from an LLM.\n" +
                "In a real implementation, this would contain actual content related to the prompt.";
            
            stopwatch.Stop();
            
            // Record telemetry for this step
            var tokenUsage = new Dictionary<string, int>
            {
                { "prompt_tokens", prompt.Length / 4 }, // Simplified token calculation
                { "completion_tokens", response.Length / 4 },
                { "total_tokens", (prompt.Length + response.Length) / 4 }
            };
            
            // Create telemetry data for this step
            var operationData = GenericLlmAdapter.CreateLlmOperationData(
                prompt: prompt,
                response: response,
                modelName: _modelName,
                latencyMs: stopwatch.ElapsedMilliseconds,
                taskType: "completion",
                provider: _provider,
                tokenUsage: tokenUsage);
            
            // Add to our chain steps for later batch processing
            chainSteps.Add(operationData);
            
            return response;
        }
    }
}