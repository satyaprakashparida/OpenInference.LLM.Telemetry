namespace OpenInference.LLM.Telemetry.Core
{
    /// <summary>
    /// Defines semantic conventions for LLM telemetry according to OpenInference specification.
    /// </summary>
    public static class SemanticConventions
    {
        /// <summary>
        /// Span kind attribute.
        /// </summary>
        public const string OPENINFERENCE_SPAN_KIND = "openinference.span.kind";
        
        /// <summary>
        /// LLM model name attribute.
        /// </summary>
        public const string LLM_MODEL = "llm.model";
        
        /// <summary>
        /// LLM model provider attribute.
        /// </summary>
        public const string LLM_MODEL_PROVIDER = "llm.model_provider";
        
        /// <summary>
        /// LLM request type attribute.
        /// </summary>
        public const string LLM_REQUEST_TYPE = "llm.request.type";
        
        /// <summary>
        /// LLM request content attribute.
        /// </summary>
        public const string LLM_REQUEST = "llm.request";
        
        /// <summary>
        /// LLM response content attribute.
        /// </summary>
        public const string LLM_RESPONSE = "llm.response";
        
        /// <summary>
        /// LLM operation success status attribute.
        /// </summary>
        public const string LLM_SUCCESS = "llm.success";
        
        /// <summary>
        /// LLM operation latency in milliseconds attribute.
        /// </summary>
        public const string LLM_LATENCY_MS = "llm.latency_ms";
        
        /// <summary>
        /// LLM error message attribute.
        /// </summary>
        public const string LLM_ERROR_MESSAGE = "llm.error.message";
        
        /// <summary>
        /// LLM prompt token count attribute.
        /// </summary>
        public const string LLM_TOKEN_COUNT_PROMPT = "llm.token_count.prompt";
        
        /// <summary>
        /// LLM completion token count attribute.
        /// </summary>
        public const string LLM_TOKEN_COUNT_COMPLETION = "llm.token_count.completion";
        
        /// <summary>
        /// LLM total token count attribute.
        /// </summary>
        public const string LLM_TOKEN_COUNT_TOTAL = "llm.token_count.total";
        
        /// <summary>
        /// LLM usage cost attribute.
        /// </summary>
        public const string LLM_USAGE_COST = "llm.usage.cost";
        
        /// <summary>
        /// LLM usage currency attribute.
        /// </summary>
        public const string LLM_USAGE_CURRENCY = "llm.usage.currency";
        
        /// <summary>
        /// LLM input messages attribute.
        /// </summary>
        public const string LLM_INPUT_MESSAGES = "llm.input.messages";
        
        /// <summary>
        /// LLM output messages attribute.
        /// </summary>
        public const string LLM_OUTPUT_MESSAGES = "llm.output.messages";
        
        /// <summary>
        /// LLM tools attribute.
        /// </summary>
        public const string LLM_TOOLS = "llm.tools";
        
        /// <summary>
        /// LLM invocation parameters attribute.
        /// </summary>
        public const string LLM_INVOCATION_PARAMETERS = "llm.invocation.parameters";
        
        /// <summary>
        /// LLM streaming flag attribute.
        /// </summary>
        public const string LLM_IS_STREAMING = "llm.streaming";
        
        /// <summary>
        /// LLM streaming chunk ID attribute.
        /// </summary>
        public const string LLM_STREAM_CHUNK_ID = "llm.stream.chunk_id";
        
        /// <summary>
        /// LLM streaming total chunks attribute.
        /// </summary>
        public const string LLM_STREAM_TOTAL_CHUNKS = "llm.stream.total_chunks";
        
        /// <summary>
        /// LLM prompt template attribute.
        /// </summary>
        public const string LLM_PROMPT_TEMPLATE_TEMPLATE = "llm.prompt_template.template";
        
        /// <summary>
        /// LLM prompt template version attribute.
        /// </summary>
        public const string LLM_PROMPT_TEMPLATE_VERSION = "llm.prompt_template.version";
        
        /// <summary>
        /// LLM prompt template name attribute.
        /// </summary>
        public const string LLM_PROMPT_TEMPLATE_NAME = "llm.prompt_template.name";
        
        /// <summary>
        /// LLM prompt template type attribute.
        /// </summary>
        public const string LLM_PROMPT_TEMPLATE_TYPE = "llm.prompt_template.type";
        
        /// <summary>
        /// LLM prompt template variables attribute.
        /// </summary>
        public const string LLM_PROMPT_TEMPLATE_VARIABLES = "llm.prompt_template.variables";
        
        /// <summary>
        /// Embedding model name attribute.
        /// </summary>
        public const string EMBEDDING_MODEL_NAME = "embedding.model";
        
        /// <summary>
        /// Embedding dimensions attribute.
        /// </summary>
        public const string EMBEDDING_DIMENSIONS = "embedding.dimensions";
        
        /// <summary>
        /// Embedding truncated flag attribute.
        /// </summary>
        public const string EMBEDDING_TRUNCATED = "embedding.truncated";
        
        /// <summary>
        /// Embedding vector attribute.
        /// </summary>
        public const string EMBEDDING_VECTOR = "embedding.vector";
        
        /// <summary>
        /// Retriever query attribute.
        /// </summary>
        public const string RETRIEVER_QUERY = "retrieval.query";
        
        /// <summary>
        /// Retriever type attribute.
        /// </summary>
        public const string RETRIEVER_TYPE = "retrieval.type";
        
        /// <summary>
        /// Retriever topK attribute.
        /// </summary>
        public const string RETRIEVER_TOP_K = "retrieval.top_k";
        
        /// <summary>
        /// Retrieval documents attribute.
        /// </summary>
        public const string RETRIEVAL_DOCUMENTS = "retrieval.documents";

        /// <summary>
        /// Chain name attribute.
        /// </summary>
        public const string CHAIN_NAME = "chain.name";

        /// <summary>
        /// Chain type attribute.
        /// </summary>
        public const string CHAIN_TYPE = "chain.type";

        /// <summary>
        /// Chain step index attribute.
        /// </summary>
        public const string CHAIN_STEP_INDEX = "chain.step.index";

        /// <summary>
        /// Chain step name attribute.
        /// </summary>
        public const string CHAIN_STEP_NAME = "chain.step.name";

        /// <summary>
        /// Span kind values.
        /// </summary>
        public static class SpanKind
        {
            /// <summary>
            /// LLM span kind.
            /// </summary>
            public const string LLM = "llm";
            
            /// <summary>
            /// Embedding span kind.
            /// </summary>
            public const string EMBEDDING = "embedding";
            
            /// <summary>
            /// Retrieval span kind.
            /// </summary>
            public const string RETRIEVAL = "retrieval";
            
            /// <summary>
            /// Agent span kind.
            /// </summary>
            public const string AGENT = "agent";
            
            /// <summary>
            /// Tool span kind.
            /// </summary>
            public const string TOOL = "tool";
            
            /// <summary>
            /// Workflow span kind.
            /// </summary>
            public const string WORKFLOW = "workflow";

            /// <summary>
            /// Chain span kind.
            /// </summary>
            public const string CHAIN = "chain";
        }
    }
}
