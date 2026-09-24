using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using SwissTechTrainer.Application.Common.Interfaces;
using SwissTechTrainer.Application.Common.Models;

namespace SwissTechTrainer.Infrastructure.LLM;

/// <summary>
/// Production LLM client implementation communicating with Groq Cloud endpoints, protected by a Polly v8 resilience pipeline.
/// </summary>
/// <param name="httpClient">The injected HTTP client instance.</param>
/// <param name="options">Configuration options for Groq API parameters and resilience.</param>
/// <param name="logger">Structured logger instance.</param>
/// <param name="fallbackClient">Deterministic fallback client used when API keys are absent or outages occur.</param>
public class GroqLlmClient(
    HttpClient httpClient,
    IOptions<LlmOptions> options,
    ILogger<GroqLlmClient> logger,
    DeterministicMockLlmClient fallbackClient) : ILLMClient
{
    private readonly LlmOptions _options = options.Value;
    private static readonly SemaphoreSlim Throttler = new(2, 2);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly ResiliencePipeline<HttpResponseMessage> _resiliencePipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
        .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
        {
            MaxRetryAttempts = Math.Max(1, options.Value.MaxRetriesOnRateLimit),
            Delay = TimeSpan.FromMilliseconds(Math.Max(500, options.Value.InitialBackoffDelayMs)),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .Handle<TimeoutRejectedException>()
                .HandleResult(res => res.StatusCode == HttpStatusCode.TooManyRequests ||
                                     res.StatusCode == HttpStatusCode.RequestTimeout ||
                                     (int)res.StatusCode >= 500),
            OnRetry = args =>
            {
                logger.LogWarning(
                    "Groq API call retry #{AttemptNumber} due to {Reason}. Waiting {DelayMs} ms.",
                    args.AttemptNumber,
                    args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString() ?? "Unknown",
                    args.RetryDelay.TotalMilliseconds);
                return ValueTask.CompletedTask;
            }
        })
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            MinimumThroughput = 4,
            BreakDuration = TimeSpan.FromSeconds(15),
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .HandleResult(res => (int)res.StatusCode >= 500 || res.StatusCode == HttpStatusCode.TooManyRequests)
        })
        .AddTimeout(TimeSpan.FromSeconds(options.Value.TimeoutSeconds > 0 ? options.Value.TimeoutSeconds : 60))
        .Build();

    /// <inheritdoc />
    public string ProviderName => $"Groq Cloud ({_options.Model})";

    /// <inheritdoc />
    public async Task<GeneratedExerciseDto> GenerateExerciseAsync(ExerciseGenerationContext context, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            logger.LogWarning("No Groq API key configured. Falling back to deterministic generator.");
            return await fallbackClient.GenerateExerciseAsync(context, ct);
        }

        string systemPrompt = LlmPromptTemplates.BuildExerciseGenerationSystemPrompt();
        string userPrompt = LlmPromptTemplates.BuildExerciseGenerationUserPrompt(context);

        try
        {
            string rawResponse = await ExecuteWithResilienceAsync(systemPrompt, userPrompt, ct);
            string cleanJson = ExtractJsonContent(rawResponse);

            var exercise = JsonSerializer.Deserialize<GeneratedExerciseDto>(cleanJson, JsonOptions);
            if (exercise != null && !string.IsNullOrWhiteSpace(exercise.Title) && !string.IsNullOrWhiteSpace(exercise.Description))
            {
                return exercise;
            }

            logger.LogWarning("Groq response JSON did not populate required exercise fields. Falling back.");
            return await fallbackClient.GenerateExerciseAsync(context, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling Groq for exercise generation. Falling back to mock client.");
            return await fallbackClient.GenerateExerciseAsync(context, ct);
        }
    }

    /// <inheritdoc />
    public async Task<LlmEvaluationResponseDto> EvaluateSubmissionAsync(EvaluationPromptContext context, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            logger.LogWarning("No Groq API key configured. Falling back to deterministic evaluation.");
            return await fallbackClient.EvaluateSubmissionAsync(context, ct);
        }

        string systemPrompt = LlmPromptTemplates.BuildEvaluationSystemPrompt();
        string userPrompt = LlmPromptTemplates.BuildEvaluationUserPrompt(context);

        try
        {
            string rawResponse = await ExecuteWithResilienceAsync(systemPrompt, userPrompt, ct);
            string cleanJson = ExtractJsonContent(rawResponse);

            var evalResult = JsonSerializer.Deserialize<LlmEvaluationResponseDto>(cleanJson, JsonOptions);
            if (evalResult != null && evalResult.Criteria != null && evalResult.Criteria.Count > 0)
            {
                return evalResult with { ModelUsed = ProviderName };
            }

            logger.LogWarning("Groq evaluation JSON was empty or malformed. Falling back.");
            return await fallbackClient.EvaluateSubmissionAsync(context, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling Groq for submission evaluation. Falling back to mock client.");
            return await fallbackClient.EvaluateSubmissionAsync(context, ct);
        }
    }

    private async Task<string> ExecuteWithResilienceAsync(string systemPrompt, string userPrompt, CancellationToken ct)
    {
        await Throttler.WaitAsync(ct);
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

            var httpResponse = await _resiliencePipeline.ExecuteAsync(async cancellation =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.Endpoint.TrimEnd('/')}/chat/completions")
                {
                    Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                };

                if (!string.IsNullOrWhiteSpace(_options.ApiKey))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
                }

                return await httpClient.SendAsync(request, cancellation);
            }, ct);

            httpResponse.EnsureSuccessStatusCode();

            string body = await httpResponse.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(body);
            var choices = doc.RootElement.GetProperty("choices");
            if (choices.GetArrayLength() > 0)
            {
                var content = choices[0].GetProperty("message").GetProperty("content").GetString();
                return content ?? string.Empty;
            }

            throw new InvalidOperationException("Groq returned empty choices array.");
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
