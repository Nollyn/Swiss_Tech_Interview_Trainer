using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Application.Common.Models;

namespace SwissTechTrainer.Infrastructure.LLM;

public class GroqLlmClient : ILLMClient
{
    private readonly HttpClient _httpClient;
    private readonly LlmOptions _options;
    private readonly ILogger<GroqLlmClient> _logger;
    private readonly ILLMClient _fallbackClient;
    private static readonly SemaphoreSlim Throttler = new(2, 2);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string ProviderName => $"Groq Cloud ({_options.Model})";

    public GroqLlmClient(
        HttpClient httpClient,
        IOptions<LlmOptions> options,
        ILogger<GroqLlmClient> logger,
        DeterministicMockLlmClient fallbackClient)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _fallbackClient = fallbackClient;

        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }
    }

    public async Task<GeneratedExerciseDto> GenerateExerciseAsync(ExerciseGenerationContext context, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("No Groq API key configured. Falling back to deterministic generator.");
            return await _fallbackClient.GenerateExerciseAsync(context, ct);
        }

        string systemPrompt = LlmPromptTemplates.BuildExerciseGenerationSystemPrompt();
        string userPrompt = LlmPromptTemplates.BuildExerciseGenerationUserPrompt(context);

        try
        {
            string rawResponse = await ExecuteWithRetryAsync(systemPrompt, userPrompt, ct);
            string cleanJson = ExtractJsonContent(rawResponse);

            var exercise = JsonSerializer.Deserialize<GeneratedExerciseDto>(cleanJson, JsonOptions);
            if (exercise != null && !string.IsNullOrWhiteSpace(exercise.Title) && !string.IsNullOrWhiteSpace(exercise.Description))
            {
                return exercise;
            }

            _logger.LogWarning("Groq response JSON did not populate required exercise fields. Falling back.");
            return await _fallbackClient.GenerateExerciseAsync(context, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Groq for exercise generation. Falling back to mock client.");
            return await _fallbackClient.GenerateExerciseAsync(context, ct);
        }
    }

    public async Task<LlmEvaluationResponseDto> EvaluateSubmissionAsync(EvaluationPromptContext context, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("No Groq API key configured. Falling back to deterministic evaluation.");
            return await _fallbackClient.EvaluateSubmissionAsync(context, ct);
        }

        string systemPrompt = LlmPromptTemplates.BuildEvaluationSystemPrompt();
        string userPrompt = LlmPromptTemplates.BuildEvaluationUserPrompt(context);

        try
        {
            string rawResponse = await ExecuteWithRetryAsync(systemPrompt, userPrompt, ct);
            string cleanJson = ExtractJsonContent(rawResponse);

            var evalResult = JsonSerializer.Deserialize<LlmEvaluationResponseDto>(cleanJson, JsonOptions);
            if (evalResult != null && evalResult.Criteria != null && evalResult.Criteria.Count > 0)
            {
                return evalResult with { ModelUsed = ProviderName };
            }

            _logger.LogWarning("Groq evaluation JSON was empty or malformed. Falling back.");
            return await _fallbackClient.EvaluateSubmissionAsync(context, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Groq for submission evaluation. Falling back to mock client.");
            return await _fallbackClient.EvaluateSubmissionAsync(context, ct);
        }
    }

    private async Task<string> ExecuteWithRetryAsync(string systemPrompt, string userPrompt, CancellationToken ct)
    {
        await Throttler.WaitAsync(ct);
        try
        {
            int maxRetries = Math.Max(1, _options.MaxRetriesOnRateLimit);
            int delayMs = Math.Max(500, _options.InitialBackoffDelayMs);

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var payload = new
                    {
                        model = _options.Model,
                        messages = new[]
                        {
                            new { role = "system", content = systemPrompt },
                            new { role = "user", content = userPrompt }
                        },
                        temperature = _options.Temperature,
                        max_tokens = _options.MaxTokens,
                        response_format = new { type = "json_object" }
                    };

                    string jsonPayload = JsonSerializer.Serialize(payload);
                    using var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.Endpoint.TrimEnd('/')}/chat/completions")
                    {
                        Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                    };

                    using var response = await _httpClient.SendAsync(request, ct);

                    if (response.StatusCode == HttpStatusCode.TooManyRequests) // 429 Rate Limit
                    {
                        _logger.LogWarning("Groq API rate limit hit (429). Attempt {Attempt}/{MaxRetries}. Backing off for {Delay}ms.", attempt, maxRetries, delayMs);
                        if (attempt == maxRetries)
                        {
                            throw new HttpRequestException($"Groq API rate limit exceeded after {maxRetries} attempts.");
                        }

                        await Task.Delay(delayMs, ct);
                        delayMs *= 2; // Exponential backoff
                        continue;
                    }

                    response.EnsureSuccessStatusCode();

                    string body = await response.Content.ReadAsStringAsync(ct);
                    using var doc = JsonDocument.Parse(body);
                    var choices = doc.RootElement.GetProperty("choices");
                    if (choices.GetArrayLength() > 0)
                    {
                        var content = choices[0].GetProperty("message").GetProperty("content").GetString();
                        return content ?? string.Empty;
                    }

                    throw new InvalidOperationException("Groq returned empty choices array.");
                }
                catch (HttpRequestException ex) when (attempt < maxRetries)
                {
                    _logger.LogWarning(ex, "Transient error calling Groq. Retrying in {Delay}ms...", delayMs);
                    await Task.Delay(delayMs, ct);
                    delayMs *= 2;
                }
            }

            throw new InvalidOperationException("Exhausted retries calling Groq API.");
        }
        finally
        {
            Throttler.Release();
        }
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
