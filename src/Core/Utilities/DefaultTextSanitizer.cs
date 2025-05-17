using System.Text.RegularExpressions;

namespace OpenInference.LLM.Telemetry.Core.Utilities
{
    /// <summary>
    /// Default implementation of ITextSanitizer that masks common sensitive information patterns.
    /// </summary>
    public class DefaultTextSanitizer : ITextSanitizer
    {
        private readonly List<(Regex Pattern, string Replacement)> _sanitizationRules;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTextSanitizer"/> class.
        /// </summary>
        public DefaultTextSanitizer()
        {
            // Initialize with default sanitization rules
            _sanitizationRules = new List<(Regex Pattern, string Replacement)>
            {
                // Credit card numbers (16 digits, may be separated by spaces or dashes)
                (new Regex(@"\b(?:\d[ -]*?){13,16}\b", RegexOptions.Compiled), "[CREDIT_CARD_REDACTED]"),
                
                // API keys and access tokens (common patterns)
                (new Regex(@"\b(?:api[_-]?key|access[_-]?token|secret[_-]?key)[^\w\n]*?[=:]\s*['""`]?\w{16,}['""`]?", RegexOptions.IgnoreCase | RegexOptions.Compiled), "[API_KEY_REDACTED]"),
                
                // Email addresses
                (new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b", RegexOptions.Compiled), "[EMAIL_REDACTED]"),
                
                // Social Security Numbers (US format: XXX-XX-XXXX or XXXXXXXXX)
                (new Regex(@"\b\d{3}[-]?\d{2}[-]?\d{4}\b", RegexOptions.Compiled), "[SSN_REDACTED]"),
                
                // Phone numbers (various formats)
                (new Regex(@"\b(?:\+\d{1,2}\s?)?(?:\(\d{3}\)|\d{3})[-.\s]?\d{3}[-.\s]?\d{4}\b", RegexOptions.Compiled), "[PHONE_NUMBER_REDACTED]"),
                
                // JWT tokens
                (new Regex(@"eyJ[a-zA-Z0-9_-]+\.eyJ[a-zA-Z0-9_-]+\.[a-zA-Z0-9_-]+", RegexOptions.Compiled), "[JWT_TOKEN_REDACTED]"),
                
                // AWS-style access keys
                (new Regex(@"(?i)(?:AKIA|A3T|AGPA|AIDA|AROA|AIPA|ANPA|ANVA|ASIA)[A-Z0-9]{12,}", RegexOptions.Compiled), "[AWS_ACCESS_KEY_REDACTED]"),
                
                // Azure connection strings
                (new Regex(@"DefaultEndpointsProtocol=https?;AccountName=[^;]+;AccountKey=[^;]+;?", RegexOptions.IgnoreCase | RegexOptions.Compiled), "[AZURE_CONNECTION_STRING_REDACTED]"),
                
                // Password fields in JSON/YAML
                (new Regex(@"""(?:password|passwd|pwd)""\s*:\s*""[^""]+""", RegexOptions.IgnoreCase | RegexOptions.Compiled), "\"password\":\"[PASSWORD_REDACTED]\""),
                (new Regex(@"password:\s*[^\s]+", RegexOptions.IgnoreCase | RegexOptions.Compiled), "password: [PASSWORD_REDACTED]")
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTextSanitizer"/> class with custom sanitization rules.
        /// </summary>
        /// <param name="customRules">List of custom sanitization rule patterns and replacements.</param>
        public DefaultTextSanitizer(List<(Regex Pattern, string Replacement)> customRules)
        {
            _sanitizationRules = customRules ?? throw new ArgumentNullException(nameof(customRules));
        }

        /// <summary>
        /// Sanitizes text by applying all configured sanitization rules to mask sensitive information.
        /// </summary>
        /// <param name="text">The input text to sanitize.</param>
        /// <returns>Sanitized text with sensitive information masked.</returns>
        /// <exception cref="ArgumentNullException">Thrown when text is null.</exception>
        public string Sanitize(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            
            if (string.IsNullOrWhiteSpace(text))
                return text;
            
            try
            {
                string sanitizedText = text;
                
                foreach (var (pattern, replacement) in _sanitizationRules)
                {
                    sanitizedText = pattern.Replace(sanitizedText, replacement);
                }
                
                return sanitizedText;
            }
            catch (RegexMatchTimeoutException ex)
            {
                // Log the exception but return the original text to prevent blocking the telemetry pipeline
                System.Diagnostics.Debug.WriteLine($"Regex timeout during text sanitization: {ex.Message}");
                return "[SANITIZATION_FAILED]";
            }
            catch (Exception ex)
            {
                // Log the exception but return safely
                System.Diagnostics.Debug.WriteLine($"Exception during text sanitization: {ex.Message}");
                return "[SANITIZATION_ERROR]";
            }
        }

        /// <summary>
        /// Adds a custom sanitization rule to the existing rules.
        /// </summary>
        /// <param name="pattern">Regular expression pattern to match sensitive information.</param>
        /// <param name="replacement">Text to replace the matched pattern with.</param>
        /// <returns>This instance for method chaining.</returns>
        public DefaultTextSanitizer AddRule(Regex pattern, string replacement)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));
                
            _sanitizationRules.Add((pattern, replacement ?? "[REDACTED]"));
            return this;
        }

        /// <summary>
        /// Adds a custom sanitization rule to the existing rules.
        /// </summary>
        /// <param name="patternString">Regular expression pattern string to match sensitive information.</param>
        /// <param name="replacement">Text to replace the matched pattern with.</param>
        /// <param name="regexOptions">Optional regex options to apply.</param>
        /// <returns>This instance for method chaining.</returns>
        public DefaultTextSanitizer AddRule(string patternString, string replacement, RegexOptions regexOptions = RegexOptions.None)
        {
            if (string.IsNullOrEmpty(patternString))
                throw new ArgumentException("Pattern string cannot be null or empty", nameof(patternString));
                
            var regex = new Regex(patternString, regexOptions | RegexOptions.Compiled);
            return AddRule(regex, replacement);
        }
    }
}
