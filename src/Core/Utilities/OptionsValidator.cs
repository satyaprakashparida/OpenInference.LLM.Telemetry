using System;
using System.Collections.Generic;
using OpenInference.LLM.Telemetry.Core.Models;

namespace OpenInference.LLM.Telemetry.Core.Utilities
{
    /// <summary>
    /// Provides validation for LLM instrumentation options.
    /// </summary>
    public static class OptionsValidator
    {
        /// <summary>
        /// Validates the LLM instrumentation options and returns any validation errors.
        /// </summary>
        /// <param name="options">The options to validate.</param>
        /// <returns>A list of validation errors, or an empty list if validation succeeds.</returns>
        /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
        public static IReadOnlyList<string> ValidateOptions(LlmInstrumentationOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
                
            var errors = new List<string>();
            
            // Validate MaxTextLength
            if (options.MaxTextLength < 0)
            {
                errors.Add($"MaxTextLength must be non-negative, current value: {options.MaxTextLength}");
            }
            
            // Validate HttpOperationTimeoutMs
            if (options.HttpOperationTimeoutMs < 0)
            {
                errors.Add($"HttpOperationTimeoutMs must be non-negative, current value: {options.HttpOperationTimeoutMs}");
            }
            
            // Add additional validation rules as needed
            
            return errors;
        }
        
        /// <summary>
        /// Validates the LLM instrumentation options and throws an exception if validation fails.
        /// </summary>
        /// <param name="options">The options to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
        /// <exception cref="ArgumentException">Thrown when options validation fails.</exception>
        public static void ValidateOptionsAndThrow(LlmInstrumentationOptions options)
        {
            var errors = ValidateOptions(options);
            
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"LLM instrumentation options validation failed: {string.Join("; ", errors)}",
                    nameof(options));
            }
        }
    }
}
