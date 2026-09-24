namespace SwissTechTrainer.Infrastructure.LLM;

/// <summary>
/// Configuration options for configuring LLM provider strategies, model parameters, and Polly v8 resilience policies.
/// </summary>
public class LlmOptions
{
    /// <summary>
    /// Configuration section key in appsettings.json.
    /// </summary>
    public const string SectionName = "LLM";

    /// <summary>
    /// Gets or sets the active LLM provider strategy ("Groq", "Ollama", or "Mock").
    /// </summary>
    public string Provider { get; set; } = "Mock";

    /// <summary>
    /// Gets or sets the API authentication key for cloud providers like Groq.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base API endpoint URL.
    /// </summary>
    public string Endpoint { get; set; } = "https://api.groq.com/openai/v1";

    /// <summary>
    /// Gets or sets the model identifier (e.g., "llama-3.3-70b-versatile" or "llama3").
    /// </summary>
    public string Model { get; set; } = "llama-3.3-70b-versatile";

    /// <summary>
    /// Gets or sets the sampling temperature (default 0.1 for high reproducibility).
    /// </summary>
    public double Temperature { get; set; } = 0.1;

    /// <summary>
    /// Gets or sets the maximum completion tokens.
    /// </summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts on rate limiting (HTTP 429) or transient 5xx errors.
    /// </summary>
    public int MaxRetriesOnRateLimit { get; set; } = 3;

    /// <summary>
    /// Gets or sets the initial backoff delay in milliseconds for exponential backoff with jitter.
    /// </summary>
    public int InitialBackoffDelayMs { get; set; } = 1500;

    /// <summary>
    /// Gets or sets the timeout in seconds for individual LLM requests.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;
}
