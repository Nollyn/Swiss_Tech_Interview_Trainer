using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Application.Common.Models;

namespace SwissTechTrainer.Infrastructure.LLM;

public class OllamaLlmClient : ILLMClient
{
    private readonly HttpClient _httpClient;
    private readonly LlmOptions _options;
    private readonly ILogger<OllamaLlmClient> _logger;
    private readonly ILLMClient _fallbackClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string ProviderName => $"Ollama Local ({_options.Model})";

    public OllamaLlmClient(
        HttpClient httpClient,
        IOptions<LlmOptions> options,
        ILogger<OllamaLlmClient> logger,
        DeterministicMockLlmClient fallbackClient)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _fallbackClient = fallbackClient;
    }

    public async Task<GeneratedExerciseDto> GenerateExerciseAsync(ExerciseGenerationContext context, CancellationToken ct = default)
    {
        string systemPrompt = LlmPromptTemplates.BuildExerciseGenerationSystemPrompt();
        string userPrompt = LlmPromptTemplates.BuildExerciseGenerationUserPrompt(context);

        try
        {
            string rawResponse = await SendOllamaChatAsync(systemPrompt, userPrompt, ct);
            string cleanJson = ExtractJsonContent(rawResponse);

            var exercise = JsonSerializer.Deserialize<GeneratedExerciseDto>(cleanJson, JsonOptions);
            if (exercise != null && !string.IsNullOrWhiteSpace(exercise.Title))
            {
                return exercise;
            }

            _logger.LogWarning("Ollama generated invalid exercise JSON. Falling back.");
            return await _fallbackClient.GenerateExerciseAsync(context, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed calling Ollama for exercise generation. Falling back.");
            return await _fallbackClient.GenerateExerciseAsync(context, ct);
        }
    }

    public async Task<LlmEvaluationResponseDto> EvaluateSubmissionAsync(EvaluationPromptContext context, CancellationToken ct = default)
    {
        string systemPrompt = LlmPromptTemplates.BuildEvaluationSystemPrompt();
        string userPrompt = LlmPromptTemplates.BuildEvaluationUserPrompt(context);

        try
        {
            string rawResponse = await SendOllamaChatAsync(systemPrompt, userPrompt, ct);
            string cleanJson = ExtractJsonContent(rawResponse);

            var evalResult = JsonSerializer.Deserialize<LlmEvaluationResponseDto>(cleanJson, JsonOptions);
            if (evalResult != null && evalResult.Criteria != null && evalResult.Criteria.Count > 0)
            {
                return evalResult with { ModelUsed = ProviderName };
            }

            _logger.LogWarning("Ollama returned malformed evaluation JSON. Falling back.");
            return await _fallbackClient.EvaluateSubmissionAsync(context, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed calling Ollama for submission evaluation. Falling back.");
            return await _fallbackClient.EvaluateSubmissionAsync(context, ct);
        }
    }

    private async Task<string> SendOllamaChatAsync(string systemPrompt, string userPrompt, CancellationToken ct)
    {
        var payload = new
        {
            model = string.IsNullOrWhiteSpace(_options.Model) ? "llama3" : _options.Model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            format = "json",
            stream = false,
            options = new
            {
                temperature = _options.Temperature
            }
        };

        string jsonPayload = JsonSerializer.Serialize(payload);
        string endpoint = $"{_options.Endpoint.TrimEnd('/')}/api/chat";

        using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync(endpoint, content, ct);
        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(responseBody);
        if (doc.RootElement.TryGetProperty("message", out var msgElement) &&
            msgElement.TryGetProperty("content", out var contentElement))
        {
            return contentElement.GetString() ?? string.Empty;
        }

        throw new InvalidOperationException("Ollama response did not contain message content.");
    }

    private static string ExtractJsonContent(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "{}";

        string trimmed = raw.Trim();
        if (trimmed.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            int firstNewline = trimmed.IndexOf('\n');
            int lastBackticks = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (firstNewline >= 0 && lastBackticks > firstNewline)
            {
                return trimmed.Substring(firstNewline + 1, lastBackticks - firstNewline - 1).Trim();
            }
        }
        else if (trimmed.StartsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            int firstNewline = trimmed.IndexOf('\n');
            int lastBackticks = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (firstNewline >= 0 && lastBackticks > firstNewline)
            {
                return trimmed.Substring(firstNewline + 1, lastBackticks - firstNewline - 1).Trim();
            }
        }

        return trimmed;
    }
}
