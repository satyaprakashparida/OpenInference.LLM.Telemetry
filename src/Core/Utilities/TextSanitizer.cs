using System.Text.RegularExpressions;

namespace OpenInference.LLM.Telemetry.Core.Utilities
{
    /// <summary>
    /// Provides robust text sanitization capabilities to remove sensitive information from telemetry data
    /// </summary>
    public class TextSanitizer
    {
        private readonly List<SanitizationRule> _rules = new List<SanitizationRule>();
        private bool _isInitialized = false;

        /// <summary>
        /// Creates a new TextSanitizer with default sanitization rules
        /// </summary>
        public TextSanitizer()
        {
            InitializeDefaultRules();
        }

        /// <summary>
        /// Creates a new TextSanitizer with custom sanitization rules
        /// </summary>
        /// <param name="rules">The custom sanitization rules to use</param>
        public TextSanitizer(IEnumerable<SanitizationRule> rules)
        {
            if (rules == null)
                throw new ArgumentNullException(nameof(rules));
                
            _rules.AddRange(rules);
            _isInitialized = true;
        }

        /// <summary>
        /// Adds a sanitization rule
        /// </summary>
        /// <param name="pattern">Regular expression pattern to match</param>
        /// <param name="replacement">Replacement text (e.g., "[REDACTED]")</param>
        /// <param name="description">Description of what is being sanitized</param>
        /// <returns>This TextSanitizer for chaining</returns>
        public TextSanitizer AddRule(string pattern, string replacement, string description)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentNullException(nameof(pattern));
                
            if (replacement == null)
                throw new ArgumentNullException(nameof(replacement));
                
            _rules.Add(new SanitizationRule(pattern, replacement, description));
            return this;
        }

        /// <summary>
        /// Adds a sanitization rule
        /// </summary>
        /// <param name="regex">Regular expression to match</param>
        /// <param name="replacement">Replacement text (e.g., "[REDACTED]")</param>
        /// <param name="description">Description of what is being sanitized</param>
        /// <returns>This TextSanitizer for chaining</returns>
        public TextSanitizer AddRule(Regex regex, string replacement, string description)
        {
            if (regex == null)
                throw new ArgumentNullException(nameof(regex));
                
            if (replacement == null)
                throw new ArgumentNullException(nameof(replacement));
                
            _rules.Add(new SanitizationRule(regex, replacement, description));
            return this;
        }

        /// <summary>
        /// Sanitizes the given text by applying all sanitization rules
        /// </summary>
        /// <param name="text">The text to sanitize</param>
        /// <returns>The sanitized text</returns>
        public string Sanitize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
                
            // Initialize default rules if not already initialized
            if (!_isInitialized)
                InitializeDefaultRules();
                
            string sanitized = text;
            
            // Apply each rule
            foreach (var rule in _rules)
            {
                sanitized = rule.Apply(sanitized);
            }
            
            return sanitized;
        }

        /// <summary>
        /// Initialize default sanitization rules for common sensitive information
        /// </summary>
        private void InitializeDefaultRules()
        {
            // Email addresses
            _rules.Add(new SanitizationRule(
                @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b",
                "[EMAIL]",
                "Email address"));

            // Credit card numbers (with or without spaces/dashes)
            _rules.Add(new SanitizationRule(
                @"\b(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|6(?:011|5[0-9]{2})[0-9]{12}|(?:2131|1800|35\d{3})\d{11}|(?:(?:5[06789]|6)[0-9]{10,17}))(?:[ -]?[0-9]{4}){3,4}\b",
                "[CREDIT_CARD]",
                "Credit card number"));

            // US Social Security Numbers (###-##-####)
            _rules.Add(new SanitizationRule(
                @"\b\d{3}-\d{2}-\d{4}\b",
                "[SSN]",
                "Social Security Number"));

            // US Phone numbers
            _rules.Add(new SanitizationRule(
                @"\b(?:\+?1[-. ]?)?\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})\b",
                "[PHONE]",
                "Phone number"));

            // API Keys/Tokens (common patterns)
            _rules.Add(new SanitizationRule(
                @"\b(?:sk-|pk-|api-|token-|key-)[a-zA-Z0-9]{10,}\b",
                "[API_KEY]",
                "API Key or Token"));

            // Azure OpenAI API Keys
            _rules.Add(new SanitizationRule(
                @"\b[a-f0-9]{32}\b",
                "[API_KEY]",
                "API Key (hexadecimal)"));
                
            // OAuth Tokens (Bearer token format)
            _rules.Add(new SanitizationRule(
                @"Bearer\s+[a-zA-Z0-9\._\-]+",
                "Bearer [API_TOKEN]",
                "Bearer token"));
                
            // IP Addresses (IPv4)
            _rules.Add(new SanitizationRule(
                @"\b(?:[0-9]{1,3}\.){3}[0-9]{1,3}\b",
                "[IP_ADDRESS]",
                "IP address"));
                
            // Basic IPv6 addresses
            _rules.Add(new SanitizationRule(
                @"\b(?:[A-F0-9]{1,4}:){7}[A-F0-9]{1,4}\b",
                "[IPv6_ADDRESS]",
                "IPv6 address"));
                
            // Passwords in URL
            _rules.Add(new SanitizationRule(
                @"(?:pass(?:word)?|pwd|secret)=([^&\s]+)",
                "password=[REDACTED]",
                "Password in URL parameter"));
                
            // URLs with credentials
            _rules.Add(new SanitizationRule(
                @"(https?://)([^:@\s]+):([^@\s]+)@([^/\s]+)",
                "$1$2:[REDACTED]@$4",
                "Credentials in URL"));
                
            // AWS Access Key Pattern
            _rules.Add(new SanitizationRule(
                @"\b(AKIA[0-9A-Z]{16})\b",
                "[AWS_KEY]",
                "AWS Access Key ID"));

            _isInitialized = true;
        }

        /// <summary>
        /// Represents a single text sanitization rule
        /// </summary>
        public class SanitizationRule
        {
            private readonly Regex _regex;
            private readonly string _replacement;
            private readonly string _description;

            /// <summary>
            /// Creates a new sanitization rule
            /// </summary>
            /// <param name="pattern">Regular expression pattern</param>
            /// <param name="replacement">Replacement text</param>
            /// <param name="description">Rule description</param>
            public SanitizationRule(string pattern, string replacement, string description)
            {
                _regex = new Regex(pattern, RegexOptions.Compiled);
                _replacement = replacement;
                _description = description ?? "Unnamed rule";
            }

            /// <summary>
            /// Creates a new sanitization rule with an existing Regex
            /// </summary>
            /// <param name="regex">Regular expression</param>
            /// <param name="replacement">Replacement text</param>
            /// <param name="description">Rule description</param>
            public SanitizationRule(Regex regex, string replacement, string description)
            {
                _regex = regex ?? throw new ArgumentNullException(nameof(regex));
                _replacement = replacement;
                _description = description ?? "Unnamed rule";
            }

            /// <summary>
            /// Applies the sanitization rule to the given text
            /// </summary>
            /// <param name="input">Text to sanitize</param>
            /// <returns>Sanitized text</returns>
            public string Apply(string input)
            {
                if (string.IsNullOrEmpty(input))
                    return input;
                    
                return _regex.Replace(input, _replacement);
            }

            /// <summary>
            /// Gets the description of this rule
            /// </summary>
            public string Description => _description;
        }
    }
}
