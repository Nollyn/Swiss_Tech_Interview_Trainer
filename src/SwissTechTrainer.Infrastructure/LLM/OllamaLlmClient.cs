using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Application.Common.Models;

namespace SwissTechTrainer.Infrastructure.LLM;

/// <summary>
/// Local LLM client implementation communicating with an Ollama endpoint (e.g. Llama-3 local).
/// </summary>
/// <param name="httpClient">The injected HTTP client instance.</param>
/// <param name="options">Configuration options for Ollama endpoint parameters.</param>
/// <param name="logger">Structured logger instance.</param>
/// <param name="fallbackClient">Deterministic fallback client used when Ollama daemon is offline.</param>
public class OllamaLlmClient(
    HttpClient httpClient,
    IOptions<LlmOptions> options,
    ILogger<OllamaLlmClient> logger,
    DeterministicMockLlmClient fallbackClient) : ILLMClient
{
    private readonly LlmOptions _options = options.Value;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly ResiliencePipeline<HttpResponseMessage> _resiliencePipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
        .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
        {
            MaxRetryAttempts = 2,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential,
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .Handle<TimeoutRejectedException>()
                .HandleResult(res => (int)res.StatusCode >= 500),
            OnRetry = args =>
            {
                logger.LogWarning(
                    "Ollama API call retry #{AttemptNumber} due to {Reason}.",
                    args.AttemptNumber,
                    args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString() ?? "Unknown");
                return ValueTask.CompletedTask;
            }
        })
        .AddTimeout(TimeSpan.FromSeconds(options.Value.TimeoutSeconds > 0 ? options.Value.TimeoutSeconds : 60))
        .Build();

    /// <inheritdoc />
    public string ProviderName => $"Ollama Local ({_options.Model})";

    /// <inheritdoc />
    public async Task<GeneratedExerciseDto> GenerateExerciseAsync(ExerciseGenerationContext context, CancellationToken ct = default)
    {
        string systemPrompt = LlmPromptTemplates.BuildExerciseGenerationSystemPrompt(context.Language);
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

            logger.LogWarning("Ollama generated invalid exercise JSON. Falling back.");
            return await fallbackClient.GenerateExerciseAsync(context, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed calling Ollama for exercise generation. Falling back.");
            return await fallbackClient.GenerateExerciseAsync(context, ct);
        }
    }

    /// <inheritdoc />
    public async Task<LlmEvaluationResponseDto> EvaluateSubmissionAsync(EvaluationPromptContext context, CancellationToken ct = default)
    {
        string systemPrompt = LlmPromptTemplates.BuildEvaluationSystemPrompt(context.Language);
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

            logger.LogWarning("Ollama returned malformed evaluation JSON. Falling back.");
            return await fallbackClient.EvaluateSubmissionAsync(context, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed calling Ollama for submission evaluation. Falling back.");
            return await fallbackClient.EvaluateSubmissionAsync(context, ct);
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

        var response = await _resiliencePipeline.ExecuteAsync(async cancellation =>
        {
            using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            return await httpClient.PostAsync(endpoint, content, cancellation);
        }, ct);

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
