namespace SwissTechTrainer.Infrastructure.LLM;

public class LlmOptions
{
    public const string SectionName = "LLM";

    public string Provider { get; set; } = "Mock"; // "Groq", "Ollama", "Mock"
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = "https://api.groq.com/openai/v1";
    public string Model { get; set; } = "llama-3.3-70b-versatile";
    public double Temperature { get; set; } = 0.1;
    public int MaxTokens { get; set; } = 4096;
    public int MaxRetriesOnRateLimit { get; set; } = 3;
    public int InitialBackoffDelayMs { get; set; } = 1500;
}
