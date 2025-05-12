namespace OpenInference.LLM.Telemetry.Core.Models
{
    /// <summary>
    /// Represents comprehensive telemetry data for an LLM operation
    /// </summary>
    public class LlmOperationData
    {
        /// <summary>
        /// Gets or sets the LLM model name used for the operation
        /// </summary>
        public string? ModelName { get; set; }

        /// <summary>
        /// Gets or sets the prompt text
        /// </summary>
        public string? Prompt { get; set; }

        /// <summary>
        /// Gets or sets the response text from the model
        /// </summary>
        public string? Response { get; set; }

        /// <summary>
        /// Gets or sets the task type (e.g., "chat", "completion", "embedding")
        /// </summary>
        public string? TaskType { get; set; }

        /// <summary>
        /// Gets or sets the model provider (e.g., "azure", "openai", "anthropic")
        /// </summary>
        public string? Provider { get; set; }

        /// <summary>
        /// Gets or sets whether the operation was successful
        /// </summary>
        public bool IsSuccess { get; set; } = true;

        /// <summary>
        /// Gets or sets the error message if the operation failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the latency of the operation in milliseconds
        /// </summary>
        public long LatencyMs { get; set; }

        /// <summary>
        /// Gets or sets the number of tokens in the prompt
        /// </summary>
        public int? PromptTokens { get; set; }

        /// <summary>
        /// Gets or sets the number of tokens in the completion
        /// </summary>
        public int? CompletionTokens { get; set; }

        /// <summary>
        /// Gets or sets the total number of tokens used
        /// </summary>
        public int? TotalTokens { get; set; }

        /// <summary>
        /// Gets or sets the estimated cost of the operation
        /// </summary>
        public decimal? Cost { get; set; }

        /// <summary>
        /// Gets or sets the currency of the cost (e.g., "USD")
        /// </summary>
        public string? Currency { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the operation
        /// </summary>
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets or sets the input messages for chat operations
        /// </summary>
        public List<LlmChatMessage>? InputMessages { get; set; }

        /// <summary>
        /// Gets or sets the output messages from chat operations
        /// </summary>
        public List<LlmChatMessage>? OutputMessages { get; set; }

        /// <summary>
        /// Gets or sets the tools used or available in the operation
        /// </summary>
        public List<LlmTool>? Tools { get; set; }

        /// <summary>
        /// Gets or sets the streaming data for streaming operations
        /// </summary>
        public LlmStreamingData? StreamingData { get; set; }

        /// <summary>
        /// Gets or sets embedding data for embedding operations
        /// </summary>
        public LlmEmbeddingData? Embedding { get; set; }

        /// <summary>
        /// Gets or sets retrieval data for retrieval operations
        /// </summary>
        public LlmRetrievalData? Retrieval { get; set; }

        /// <summary>
        /// Gets or sets prompt template information
        /// </summary>
        public LlmPromptTemplate? PromptTemplate { get; set; }

        /// <summary>
        /// Gets or sets custom attributes to include in telemetry
        /// </summary>
        public Dictionary<string, object>? CustomAttributes { get; set; }

        /// <summary>
        /// Gets or sets parameters used for the invocation
        /// </summary>
        public Dictionary<string, object>? InvocationParameters { get; set; }
    }

    /// <summary>
    /// Represents a chat message in an LLM conversation
    /// </summary>
    public class LlmChatMessage
    {
        /// <summary>
        /// Gets or sets the role of the message sender (e.g., "user", "assistant", "system")
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// Gets or sets the content of the message
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Gets or sets the index of the message in the conversation
        /// </summary>
        public int? Index { get; set; }

        /// <summary>
        /// Gets or sets function call information if the message contains a function call
        /// </summary>
        public LlmFunctionCall? FunctionCall { get; set; }

        /// <summary>
        /// Gets or sets tool calls if the message contains tool calls
        /// </summary>
        public List<LlmToolCall>? ToolCalls { get; set; }
    }

    /// <summary>
    /// Represents a function call made by an LLM
    /// </summary>
    public class LlmFunctionCall
    {
        /// <summary>
        /// Gets or sets the name of the function
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the arguments passed to the function as a JSON string
        /// </summary>
        public string? Arguments { get; set; }
    }

    /// <summary>
    /// Represents a tool call made by an LLM
    /// </summary>
    public class LlmToolCall
    {
        /// <summary>
        /// Gets or sets the ID of the tool call
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the type of tool call (e.g., "function")
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the function call information if the tool call is a function
        /// </summary>
        public LlmFunctionCall? FunctionCall { get; set; }
    }

    /// <summary>
    /// Represents a tool available to an LLM
    /// </summary>
    public class LlmTool
    {
        /// <summary>
        /// Gets or sets the name of the tool
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the tool
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the JSON schema of the tool
        /// </summary>
        public string? JsonSchema { get; set; }

        /// <summary>
        /// Gets or sets the ID of the tool
        /// </summary>
        public string? Id { get; set; }
    }

    /// <summary>
    /// Represents streaming data for an LLM operation
    /// </summary>
    public class LlmStreamingData
    {
        /// <summary>
        /// Gets or sets whether the operation is streaming
        /// </summary>
        public bool IsStreaming { get; set; }

        /// <summary>
        /// Gets or sets the chunk ID for streaming
        /// </summary>
        public string? ChunkId { get; set; }

        /// <summary>
        /// Gets or sets the total number of chunks in the stream
        /// </summary>
        public int? TotalChunks { get; set; }
    }

    /// <summary>
    /// Represents embedding data for an LLM operation
    /// </summary>
    public class LlmEmbeddingData
    {
        /// <summary>
        /// Gets or sets the model name used for embedding
        /// </summary>
        public string? ModelName { get; set; }

        /// <summary>
        /// Gets or sets the texts to embed
        /// </summary>
        public List<string>? Texts { get; set; }

        /// <summary>
        /// Gets or sets the embedding vectors
        /// </summary>
        public List<List<float>>? Vectors { get; set; }

        /// <summary>
        /// Gets or sets the dimensions of the embeddings
        /// </summary>
        public int? Dimensions { get; set; }

        /// <summary>
        /// Gets or sets whether the input was truncated
        /// </summary>
        public bool? Truncated { get; set; }
    }

    /// <summary>
    /// Represents retrieval data for an LLM operation
    /// </summary>
    public class LlmRetrievalData
    {
        /// <summary>
        /// Gets or sets the query used for retrieval
        /// </summary>
        public string? Query { get; set; }

        /// <summary>
        /// Gets or sets the documents retrieved
        /// </summary>
        public List<LlmDocument>? Documents { get; set; }

        /// <summary>
        /// Gets or sets the type of retriever used
        /// </summary>
        public string? RetrieverType { get; set; }

        /// <summary>
        /// Gets or sets the top-k value used for retrieval
        /// </summary>
        public int? TopK { get; set; }
    }

    /// <summary>
    /// Represents a document retrieved during an LLM operation
    /// </summary>
    public class LlmDocument
    {
        /// <summary>
        /// Gets or sets the ID of the document
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the content of the document
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Gets or sets the metadata of the document
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// Gets or sets the relevance score of the document
        /// </summary>
        public float? Score { get; set; }

        /// <summary>
        /// Gets or sets the URI of the document
        /// </summary>
        public string? Uri { get; set; }

        /// <summary>
        /// Gets or sets the chunk ID of the document
        /// </summary>
        public string? ChunkId { get; set; }

        /// <summary>
        /// Gets or sets the size of the chunk
        /// </summary>
        public int? ChunkSize { get; set; }

        /// <summary>
        /// Gets or sets the index of the document in the result list
        /// </summary>
        public int? Index { get; set; }
    }

    /// <summary>
    /// Represents a prompt template used in an LLM operation
    /// </summary>
    public class LlmPromptTemplate
    {
        /// <summary>
        /// Gets or sets the template text
        /// </summary>
        public string? Template { get; set; }

        /// <summary>
        /// Gets or sets the version of the template
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// Gets or sets the name of the template
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the template
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the variables used in the template
        /// </summary>
        public Dictionary<string, object>? Variables { get; set; }
    }
}