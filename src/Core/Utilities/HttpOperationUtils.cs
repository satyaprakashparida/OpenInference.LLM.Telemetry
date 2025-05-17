using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenInference.LLM.Telemetry.Core.Models;

namespace OpenInference.LLM.Telemetry.Core.Utilities
{
    /// <summary>
    /// Provides HTTP operation utilities for LLM telemetry.
    /// </summary>
    public static class HttpOperationUtils
    {        /// <summary>
        /// Creates a CancellationToken for HTTP operations based on the configured timeout.
        /// </summary>
        /// <param name="options">The LLM instrumentation options, can be null.</param>
        /// <param name="originalToken">The original cancellation token.</param>
        /// <returns>A combined cancellation token that respects both the original token and the timeout.</returns>
        public static CancellationToken CreateTimeoutToken(LlmInstrumentationOptions? options, CancellationToken originalToken)
        {
            if (options == null)
            {
                return originalToken;
            }

            // If no timeout is specified or it's set to 0 or negative, just use the original token
            if (options.HttpOperationTimeoutMs <= 0)
            {
                return originalToken;
            }

            // Create a token source for the timeout
            var timeoutSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(options.HttpOperationTimeoutMs));
            
            // Link with the original token so either one can cancel the operation
            return CancellationTokenSource.CreateLinkedTokenSource(originalToken, timeoutSource.Token).Token;
        }        /// <summary>
        /// Executes an HTTP operation with the specified timeout.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="options">The LLM instrumentation options, can be null.</param>
        /// <param name="operation">The HTTP operation to execute.</param>
        /// <param name="originalToken">The original cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        public static async Task<T> ExecuteWithTimeout<T>(
            LlmInstrumentationOptions? options, 
            Func<CancellationToken, Task<T>> operation, 
            CancellationToken originalToken)
        {
            var combinedToken = CreateTimeoutToken(options, originalToken);
            
            try
            {
                return await operation(combinedToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (combinedToken.IsCancellationRequested && !originalToken.IsCancellationRequested)
            {
                // If the combined token was canceled but not the original token, it means our timeout was triggered
                throw new TimeoutException($"The operation timed out after {options?.HttpOperationTimeoutMs ?? 0} ms");
            }
        }
    }
}
