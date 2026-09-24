# ADR 0004: LLM Provider Abstraction, Rate Limiting & Resilience Strategy

## Status
Accepted

## Context
The application relies on LLMs for dynamic exercise generation and code evaluation. The user environment supports both **Groq Cloud API** (high speed, cost-effective, but subject to free-tier rate limits and 429 status codes) and **Ollama / Llama 3 local** (offline, no external cost, but resource-dependent). Furthermore, unit and integration tests must run without real external LLM dependencies.

## Decision
We implement a **Strategy Pattern** behind the `ILLMClient` interface with specialized resilient adapters:

1. **`ILLMClient` Port**:
   - `GenerateExerciseAsync(ExercisePromptContext context, CancellationToken ct)`
   - `EvaluateSubmissionAsync(EvaluationPromptContext context, CancellationToken ct)`
2. **Implementations**:
   - `GroqLlmClient`: Connects to Groq OpenAI-compatible endpoints (`https://api.groq.com/openai/v1/chat/completions`) utilizing models like `llama-3.3-70b-versatile` or `llama3-8b-8192`.
   - `OllamaLlmClient`: Connects to local Ollama instance (`http://localhost:11434/api/chat` or local OpenAI-compatible endpoint).
   - `DeterministicMockLlmClient`: High-fidelity deterministic simulator used in unit/integration tests and offline demo environments.
3. **Resilience & Rate Limiting Engine**:
   - Decorator pattern wrapping HTTP clients with token-bucket / concurrency throttler.
   - Exponential backoff with jitter on HTTP 429 and transient 5xx errors.
   - Graceful timeout handling returning structured `Result<T>` instead of unhandled exceptions, giving immediate feedback to the candidate.

## Consequences
- **Positive**: Zero coupling to a single AI vendor. Seamless switching via `appsettings.json` (`LLM:Provider = "Groq" | "Ollama" | "Mock"`).
- **Negative/Mitigation**: Small boilerplate in client mappings; mitigated by common JSON serialization models.
