# Swiss Tech Interview Trainer 🇨🇭
> Specialized Technical Interview Preparation Platform for Senior .NET Developers and Tech Leads targeting the Zurich & Swiss Tech Market.

<!-- Architectural Badges -->
![Architecture](https://shields.io/badge/Architecture-Clean%20Architecture%20%7C%20CQRS%20%7C%20DDD-darkgreen)
![Tech-Stack](https://shields.io/badge/Tech%20Stack-.NET%2010%20%7C%20Blazor%20Server%20%7C%20MediatR-indigo)
![Design-Patterns](https://shields.io/badge/Patterns-Evaluator--Optimizer%20%7C%20Specification%20%7C%20CQRS-blueviolet)
![AI-Engine](https://shields.io/badge/AI%20Engine-Groq%20%7C%20Llama%203.3%20%7C%20Ollama-blue)
![Database](https://shields.io/badge/Database-PostgreSQL%20%7C%20SQLite%20%7C%20EF%20Core%20(Split%20Queries)-orange)
![I18N](https://shields.io/badge/I18N-English%20%7C%20German%20%7C%20Spanish-teal)
![Evaluation](https://shields.io/badge/Evaluation-Deterministic%20Scoring%20%7C%207%20Rubrics-darkred)

---

## 📌 Overview & Value Proposition
The **Swiss Tech Interview Trainer** prepares candidates for the rigorous standards of Swiss private banking, electronic trading, and fintech institutions (SIX Group, UBS, Avaloq, Swissquote, Zühlke).

The platform enforces a deterministic, objective **Evaluator-Optimizer pipeline** across **7 core technical dimensions**, requiring a strict **$\ge 90\%$ mastery threshold** to advance across 5 progressive difficulty levels.

---

## 🌐 Multi-Language Runtime Tracks & UI Internationalization (I18N)

### 1. Polyglot Backend Evaluation Tracks
The system natively supports adaptive programming language selection (`C#`, `Python`, `Rust`, `Java`, `Go`, `TypeScript`) with isolated progression tracking:
- **Language-Specific Rubrics**: Evaluation criteria adapt dynamically to the target ecosystem (e.g., Memory/Span/Async in C#, GIL/Asyncio/Typing in Python, Borrow Checker/Lifetimes in Rust).
- **Targeted LLM Prompt Templates**: Prompt engineering templates (`LlmPromptTemplates`) supply language-specific negative context, idiom guidelines, and starter boilerplate.
- **Isolated Progression**: Category level progression, mastery scores, and attempt histories are strictly segregated per `(UserId, Category, Language)` composite keys (see [ADR 0007](docs/adr/0007-multi-language-and-i18n-architecture.md)).

### 2. UI Internationalization (I18N)
- **Supported Cultures**: English (`en`), German (`de-CH`), and Spanish (`es-ES`).
- **Resource Management**: Managed via strongly typed ASP.NET Core `SharedResources.resx` localizers injected across all Blazor components (`IStringLocalizer<SharedResources>`).
- **Culture Persistence**: Persisted using standard `.AspNetCore.Culture` cookies via `/api/culture/set` endpoint, ensuring seamless hot-swapping and SignalR circuit stability.

---

## 🎯 The 7 Swiss Tech Lead Categories

| Category | Description | Primary Evaluation Rubric |
| :--- | :--- | :--- |
| **1. Coding / Algorithms** | Pragmatic algorithms, sliding windows, and Big-O efficiency. | Algorithmic Correctness (35%), Time/Space Complexity (30%), Modularity (20%), Idiomatic Syntax (15%) |
| **2. System Design** | Distributed architectures, idempotency, event sourcing, and CAP trade-offs. | Scalability & Architecture (35%), Resilience & Fault Tolerance (25%), Data Consistency (25%), Observability (15%) |
| **3. Deep Dive / Internals** | Runtime memory management, GC internals, Span&lt;T&gt;, concurrency mechanics. | Memory/GC & Allocations (30%), Concurrency & Async (30%), Runtime Execution (25%), Error Handling (15%) |
| **4. Clean Code** | Refactoring legacy code smells, SOLID compliance, and cognitive load. | SOLID Compliance (30%), Readability (20%), Defensive Handling (25%), Performance Efficiency (25%) |
| **5. API Design** | REST/gRPC contracts, idempotency keys, Problem Details, and rate limits. | Contract & Idempotency (30%), HTTP/gRPC Semantics (25%), Resilience (25%), Security & Telemetry (20%) |
| **6. Testing & TDD** | Strict TDD, AAA pattern, boundary resilience, and pragmatic mocking. | Coverage & Boundaries (35%), AAA & Clarity (25%), Mutation Resilience (20%), Mocking Pragmatism (20%) |
| **7. Behavioral (STAR)** | High-stakes engineering leadership, conflict resolution, and architectural ownership. | Situation & Task (25%), Action & Leadership (35%), Result & Impact (25%), Reflection (15%) |

---

## 🧠 Evaluator-Optimizer & Deterministic Scoring Pipeline
1. **Dynamic Generation with Negative Context**: Generates distinct exercises per level without repeating recently seen challenges.
2. **Multi-Criteria LLM Scoring**: Low temperature ($0.1$) assessment with cited code snippets and concrete before/after code refactorings.
3. **Deterministic Score Aggregation**: Overall final scores are computed mathematically inside pure domain code via $\text{FinalScore} = \text{Round}\left(\frac{\sum s_i \cdot w_i}{\sum w_i}\right)$.
4. **Adaptive Progression & Anti-Frustration**:
   - $\text{Score} \ge 90\%$: Unlocks the next difficulty level ($N+1$).
   - $\text{Failures} \ge 3$: Activates **Hint Mode**, unlocking diagnostic questions and architectural guidance.

---

## 🏛️ Architecture & Clean Code Organization

The solution adheres strictly to **Clean Architecture** and **CQRS (Command Query Responsibility Segregation)** principles:

```
src/
├── SwissTechTrainer.Domain/          # Pure Domain Layer (zero external dependencies)
│   ├── Entities/                     # UserProfile, UserCategoryProgress, Exercise, Submission, Evaluation
│   ├── Enums/                        # ProgrammingLanguage, CategoryType, DifficultyLevel, SubmissionType
│   ├── Services/                     # CategoryRubricCatalog (dynamic per-language rubrics)
│   ├── Specifications/               # ProgressionSpecification (domain advancement rules)
│   └── ValueObjects/                 # CriterionScore, CodeDiffSnippet
│
├── SwissTechTrainer.Application/     # CQRS Application Layer (MediatR, FluentValidation)
│   ├── Common/                       # Pipeline behaviors (Logging, Performance, Validation), Ports
│   ├── Features/Dashboard/           # GetUserDashboardQuery & Handler (Split queries, metrics)
│   ├── Features/Exercises/           # GetOrCreateCurrentExerciseQuery, GenerateNewExerciseVariantCommand
│   └── Features/Submissions/         # SubmitExerciseCommand (Evaluator-Optimizer orchestration)
│
├── SwissTechTrainer.Infrastructure/  # Infrastructure & External Adapters
│   ├── LLM/                          # GroqLlmClient, OllamaLlmClient, DeterministicMockLlmClient
│   ├── Persistence/                  # AppDbContext, DatabaseInitializer, DatabaseSeeder, Migrations
│   └── Services/                     # CurrentUserService, CodeFileParser (.cs, .py, .zip extraction)
│
└── SwissTechTrainer.Web/             # Blazor Interactive Server UI Layer
    ├── Components/Pages/             # Home (Dashboard), ExerciseRoom (Workspace), History
    ├── Components/Shared/            # CultureSelector, MarkdownViewer, CodeDiffViewer
    └── Resources/                    # SharedResources.resx (en, de, es I18N resource catalogs)
```

---

## 💾 Database Querying & EF Core Architecture Strategies

Following [ADR 0003](docs/adr/0003-database-and-persistence-strategy.md), the database persistence strategy optimizes for high concurrency and relational data integrity:

1. **Split Query Execution (`QuerySplittingBehavior.SplitQuery`)**:
   - Configured globally in `DependencyInjection.cs` and applied on complex queries via `.AsSplitQuery()`.
   - Eliminates Cartesian explosion and duplicate data transfer when loading multiple collection navigations (`Progresses`, `Submissions`, `Evaluations`).
2. **Deterministic Query Ordering**:
   - Explicit `.OrderBy(e => e.Id)` / `.OrderByDescending(e => e.CreatedAt)` clauses precede all `First` / `FirstOrDefault` operations, guaranteeing predictable query plan caching and consistent data resolution.
3. **Optimized JSON Serialization & Value Comparers**:
   - Structured JSON-mapped columns (`CriteriaScores`, `CodeSuggestions`) use custom `ValueComparer<IReadOnlyCollection<T>>` instances to ensure precise change tracking and eliminate EF Core snapshot comparison warnings.
4. **Resilient Multi-Provider Migrations & Schema Initialization**:
   - `DatabaseInitializer` manages automatic schema migration and backwards-compatible runtime upgrades for both PostgreSQL (production) and SQLite (offline development).

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
Access the application at `http://localhost:8080`.

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
All unit tests and integration tests validate scoring determinism, progression rules, file parsing, query optimization, I18N resource localization, and pipeline orchestration.
