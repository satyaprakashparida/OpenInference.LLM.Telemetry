namespace OpenInference.LLM.Telemetry.Core
{
    public static class SemanticConventions
    {
        public const string LLM_REQUEST = "llm.request";
        public const string LLM_RESPONSE = "llm.response";
        public const string LLM_MODEL = "llm.model";
        public const string LLM_MODEL_PROVIDER = "llm.model_provider";
        public const string LLM_REQUEST_TYPE = "llm.request_type";
        public const string LLM_SUCCESS = "llm.success";
        public const string LLM_ERROR_MESSAGE = "llm.error.message";
        public const string LLM_LATENCY_MS = "llm.latency_ms";
        public const string LLM_TOKEN_COUNT_PROMPT = "llm.token_count.prompt";
        public const string LLM_TOKEN_COUNT_COMPLETION = "llm.token_count.completion";
        public const string LLM_TOKEN_COUNT_TOTAL = "llm.token_count.total";
    }
}