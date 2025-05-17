namespace OpenInference.LLM.Telemetry.Core.Utilities
{
    /// <summary>
    /// Interface for sanitizing text content to remove or mask sensitive information.
    /// </summary>
    public interface ITextSanitizer
    {
        /// <summary>
        /// Sanitizes the provided text by removing or masking sensitive information.
        /// </summary>
        /// <param name="text">The text to sanitize.</param>
        /// <returns>The sanitized text with sensitive information removed or masked.</returns>
        string Sanitize(string text);
    }
}
