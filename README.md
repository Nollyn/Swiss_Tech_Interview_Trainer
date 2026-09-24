# Swiss Tech Interview Trainer 🇨🇭
> Specialized Technical Interview Preparation Platform for Senior .NET Developers and Tech Leads targeting the Zurich & Swiss Tech Market.

<!-- Architectural Badges -->
![Architecture](https://shields.io/badge/Architecture-Clean%20Architecture%20%7C%20CQRS%20%7C%20DDD-darkgreen)
![Tech-Stack](https://shields.io/badge/Tech%20Stack-.NET%2010%20%7C%20Blazor%20Server%20%7C%20MediatR-indigo)
![Design-Patterns](https://shields.io/badge/Patterns-Evaluator--Optimizer%20%7C%20Specification%20%7C%20CQRS-blueviolet)
![AI-Engine](https://shields.io/badge/AI%20Engine-Groq%20%7C%20Llama%203.3%20%7C%20Ollama-blue)
![Database](https://shields.io/badge/Database-PostgreSQL%20%7C%20SQLite%20%7C%20EF%20Core-orange)
![Evaluation](https://shields.io/badge/Evaluation-Deterministic%20Scoring%20%7C%207%20Rubrics-darkred)

---

## 📌 Overview & Value Proposition
The **Swiss Tech Interview Trainer** prepares candidates for the rigorous standards of Swiss private banking, electronic trading, and fintech institutions (SIX Group, UBS, Avaloq, Swissquote, Zühlke).

The platform enforces a deterministic, objective **Evaluator-Optimizer pipeline** across **7 core technical dimensions**, requiring a strict **$\ge 90\%$ mastery threshold** to advance across 5 progressive difficulty levels.

---

## 🎯 The 7 Swiss Tech Lead Categories

| Category | Description | Primary Evaluation Rubric |
| :--- | :--- | :--- |
| **1. Coding / Algorithms** | Pragmatic algorithms, sliding windows, and Big-O efficiency. | Algorithmic Correctness (35%), Time/Space Complexity (30%), Modularity (20%), Idiomatic C# (15%) |
| **2. System Design** | Distributed architectures, idempotency, event sourcing, and CAP trade-offs. | Scalability & Architecture (35%), Resilience & Fault Tolerance (25%), Data Consistency (25%), Observability (15%) |
| **3. .NET Deep Dive** | CLR memory management, GC internals, Span&lt;T&gt;, async/await mechanics. | Memory/GC & Allocations (30%), Async/Await & Concurrency (30%), CLR Execution (25%), Error Handling (15%) |
| **4. Clean Code** | Refactoring legacy code smells, SOLID compliance, and cognitive load. | SOLID Compliance (30%), Readability (20%), Defensive Handling (25%), Performance Efficiency (25%) |
| **5. API Design** | REST/gRPC contracts, idempotency keys, Problem Details, and rate limits. | Contract & Idempotency (30%), HTTP/gRPC Semantics (25%), Resilience (25%), Security & Telemetry (20%) |
| **6. Testing & TDD** | Strict TDD, AAA pattern, boundary resilience, and pragmatic mocking. | Coverage & Boundaries (35%), AAA & Clarity (25%), Mutation Resilience (20%), Mocking Pragmatism (20%) |
| **7. Behavioral (STAR)** | High-stakes engineering leadership, conflict resolution, and architectural ownership. | Situation & Task (25%), Action & Leadership (35%), Result & Impact (25%), Reflection (15%) |

---

## 🧠 Evaluator-Optimizer & Deterministic Scoring Pipeline
1. **Dynamic Generation with Negative Context**: Generates distinct exercises per level without repeating recently seen challenges.
2. **Multi-Criteria LLM Scoring**: Low temperature ($0.1$) assessment with cited code snippets and concrete before/after code refactorings.
3. **Deterministic C# Aggregation**: Overall final scores are computed mathematically inside pure domain code via $\text{FinalScore} = \text{Round}\left(\frac{\sum s_i \cdot w_i}{\sum w_i}\right)$.
4. **Adaptive Progression & Anti-Frustration**:
   - $\text{Score} \ge 90\%$: Unlocks the next difficulty level ($N+1$).
   - $\text{Failures} \ge 3$: Activates **Hint Mode**, unlocking diagnostic questions and architectural guidance.

---

## 🏛️ Architecture & Clean Code
- **`SwissTechTrainer.Domain`**: Pure domain entities (`Exercise`, `Submission`, `Evaluation`, `UserCategoryProgress`), value objects, rubrics, and specifications.
- **`SwissTechTrainer.Application`**: CQRS features with MediatR, FluentValidation, DTOs, and port interfaces (`ILLMClient`, `IApplicationDbContext`).
- **`SwissTechTrainer.Infrastructure`**: EF Core persistence (PostgreSQL / SQLite), resilient Groq / Ollama / Mock LLM clients with 429 exponential backoff, and `.cs`/`.zip` file parsers.
- **`SwissTechTrainer.Web`**: Modern Blazor Interactive Server UI with syntax highlighting, live stage evaluation progress, and before/after code comparison.

---

## 🚀 Quickstart & Execution

### 1. Run with .NET CLI (Default Portable SQLite & Mock LLM)
```bash
dotnet run --project src/SwissTechTrainer.Web
```
Navigate to: `http://localhost:5000` or `https://localhost:5001`.

### 2. Run with Docker Compose (PostgreSQL + Web Container)
```bash
docker-compose up --build
```
Access the application at `http://localhost:5000`.

### 3. Enabling Groq Cloud LLM (Llama 3.3 70B)
Set your environment variable or update `appsettings.json`:
```json
{
  "LLM": {
    "Provider": "Groq",
    "ApiKey": "YOUR_GROQ_API_KEY",
    "Model": "llama-3.3-70b-versatile"
  }
}
```

---

## 🧪 Running Automated Tests
```bash
dotnet test Swiss_Tech_Interview_Trainer.sln
```
All unit tests and integration tests validate scoring determinism, progression rules, file parsing, and pipeline orchestration.
