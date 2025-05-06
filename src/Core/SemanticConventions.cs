namespace OpenInference.LLM.Telemetry.Core
{
    /// <summary>
    /// Contains constant string values for OpenInference semantic conventions.
    /// Based on the OpenInference specification: https://github.com/Arize-ai/openinference/blob/main/spec/semantic_conventions.md
    /// </summary>
    public static class SemanticConventions
    {
        // Core LLM Attributes
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

        // Extended LLM Attributes
        public const string LLM_PROVIDER = "llm.provider";
        public const string LLM_SYSTEM = "llm.system";
        public const string LLM_MODEL_NAME = "llm.model_name";
        public const string LLM_FUNCTION_CALL = "llm.function_call";
        public const string LLM_INVOCATION_PARAMETERS = "llm.invocation_parameters";

        // Input/Output Messages
        public const string LLM_INPUT_MESSAGES = "llm.input_messages";
        public const string LLM_OUTPUT_MESSAGES = "llm.output_messages";
        public const string MESSAGE_ROLE = "message.role";
        public const string MESSAGE_CONTENT = "message.content";
        public const string MESSAGE_CONTENTS = "message.contents";
        public const string MESSAGE_FUNCTION_CALL_ARGUMENTS_JSON = "message.function_call_arguments_json";
        public const string MESSAGE_FUNCTION_CALL_NAME = "message.function_call_name";
        public const string MESSAGE_TOOL_CALL_ID = "message.tool_call_id";
        public const string MESSAGE_TOOL_CALLS = "message.tool_calls";

        // Token Usage Details
        public const string LLM_TOKEN_COUNT_COMPLETION_DETAILS_REASONING = "llm.token_count.completion_details.reasoning";
        public const string LLM_TOKEN_COUNT_COMPLETION_DETAILS_AUDIO = "llm.token_count.completion_details.audio";
        public const string LLM_TOKEN_COUNT_PROMPT_DETAILS_CACHE_READ = "llm.token_count.prompt_details.cache_read";
        public const string LLM_TOKEN_COUNT_PROMPT_DETAILS_CACHE_WRITE = "llm.token_count.prompt_details.cache_write";
        public const string LLM_TOKEN_COUNT_PROMPT_DETAILS_AUDIO = "llm.token_count.prompt_details.audio";

        // Tools and Function Calling
        public const string LLM_TOOLS = "llm.tools";
        public const string TOOL_NAME = "tool.name";
        public const string TOOL_DESCRIPTION = "tool.description";
        public const string TOOL_JSON_SCHEMA = "tool.json_schema";
        public const string TOOL_PARAMETERS = "tool.parameters";
        public const string TOOL_ID = "tool.id";
        public const string TOOL_CALL_FUNCTION_NAME = "tool_call.function.name";
        public const string TOOL_CALL_FUNCTION_ARGUMENTS = "tool_call.function.arguments";
        public const string TOOL_CALL_ID = "tool_call.id";

        // Prompt Template Information
        public const string LLM_PROMPT_TEMPLATE_TEMPLATE = "llm.prompt_template.template";
        public const string LLM_PROMPT_TEMPLATE_VARIABLES = "llm.prompt_template.variables";
        public const string LLM_PROMPT_TEMPLATE_VERSION = "llm.prompt_template.version";

        // Retrieval and Documents
        public const string DOCUMENT_CONTENT = "document.content";
        public const string DOCUMENT_ID = "document.id";
        public const string DOCUMENT_METADATA = "document.metadata";
        public const string DOCUMENT_SCORE = "document.score";
        public const string RETRIEVAL_DOCUMENTS = "retrieval.documents";
        public const string RERANKER_INPUT_DOCUMENTS = "reranker.input_documents";
        public const string RERANKER_OUTPUT_DOCUMENTS = "reranker.output_documents";
        public const string RERANKER_MODEL_NAME = "reranker.model_name";
        public const string RERANKER_QUERY = "reranker.query";
        public const string RERANKER_TOP_K = "reranker.top_k";

        // Embedding Information
        public const string EMBEDDING_EMBEDDINGS = "embedding.embeddings";
        public const string EMBEDDING_MODEL_NAME = "embedding.model_name";
        public const string EMBEDDING_TEXT = "embedding.text";
        public const string EMBEDDING_VECTOR = "embedding.vector";

        // Media and Content Types
        public const string INPUT_MIME_TYPE = "input.mime_type";
        public const string INPUT_VALUE = "input.value";
        public const string OUTPUT_MIME_TYPE = "output.mime_type";
        public const string OUTPUT_VALUE = "output.value";
        public const string IMAGE_URL = "image.url";
        public const string AUDIO_URL = "audio.url";
        public const string AUDIO_MIME_TYPE = "audio.mime_type";
        public const string AUDIO_TRANSCRIPT = "audio.transcript";

        // Message Content Types
        public const string MESSAGECONTENT_TYPE = "messagecontent.type"; 
        public const string MESSAGECONTENT_TEXT = "messagecontent.text";
        public const string MESSAGECONTENT_IMAGE = "messagecontent.image";

        // Exception and Error Handling
        public const string EXCEPTION_ESCAPED = "exception.escaped";
        public const string EXCEPTION_MESSAGE = "exception.message";
        public const string EXCEPTION_STACKTRACE = "exception.stacktrace";
        public const string EXCEPTION_TYPE = "exception.type";

        // Session and User Information
        public const string SESSION_ID = "session.id";
        public const string USER_ID = "user.id";
        public const string METADATA = "metadata";
        public const string TAG_TAGS = "tag.tags";
        public const string OPENINFERENCE_SPAN_KIND = "openinference.span.kind";

        // Well-known values for llm.system
        public static class LlmSystem
        {
            public const string ANTHROPIC = "anthropic";
            public const string OPENAI = "openai";
            public const string VERTEXAI = "vertexai";
            public const string COHERE = "cohere";
            public const string MISTRALAI = "mistralai";
        }

        // Well-known values for llm.provider
        public static class LlmProvider
        {
            public const string ANTHROPIC = "anthropic";
            public const string OPENAI = "openai";
            public const string COHERE = "cohere";
            public const string MISTRALAI = "mistralai";
            public const string AZURE = "azure";
            public const string GOOGLE = "google";
            public const string AWS = "aws";
        }

        // Well-known values for openinference.span.kind
        public static class SpanKind
        {
            public const string CHAIN = "CHAIN";
            public const string LLM = "LLM";
            public const string RETRIEVER = "RETRIEVER";
            public const string RERANKER = "RERANKER";
        }
    }
}