# ADR 0002: Design of LLM Code Evaluation Pipeline (Evaluator-Optimizer & Deterministic Scoring)

## Status
Accepted

## Context
Evaluating senior/tech lead candidate code submissions via LLMs presents critical engineering challenges:
1. **Subjectivity and Non-Determinism**: If an LLM is asked to assign a single overall score (e.g. "Rate 1-100"), the resulting grades fluctuate wildly across calls and lack actionable explanation.
2. **Hallucinated or Vague Feedback**: Feedback must pinpoint exact line numbers, snippets, and specific technical reasons (e.g., memory allocations in hot paths, violated Open/Closed principle, lack of idempotency).
3. **JSON Schema Non-Conformance**: LLM outputs might occasionally fail JSON formatting rules or omit required criteria.

## Decision
We implement a multi-stage **Evaluator-Optimizer** pipeline with **Deterministic Weighted Scoring** and **Strict JSON Schema Validation**:

```
[Candidate Submission] 
       │
       ▼
[Stage 1: Prompt Builder (Category-Specific Rubric + Context)]
       │
       ▼
[Stage 2: LLM Multi-Criteria Assessment (Low Temperature: 0.1)]
       │
       ├──> Criterion 1 (Score 0-100, Weight W1, Evidence, Rationale)
       ├──> Criterion 2 (Score 0-100, Weight W2, Evidence, Rationale)
       ├──> Criterion 3 (Score 0-100, Weight W3, Evidence, Rationale)
       └──> Corrected Code Suggestions ("Before / After" Diff Snippets)
       │
       ▼
[Stage 3: Schema Validation & Auto-Correction Loop (Max Retries: 2)]
       │
       ▼
[Stage 4: Deterministic Aggregate Calculation in Domain Code]
       │  Formula: FinalScore = Round( SUM(CriterionScore_i * Weight_i) / SUM(Weights) )
       │  Threshold Gate: >= 90% unlocks next level
       ▼
[Stage 5: Result Persistence & Anti-Frustration Hint Trigger (if >= 3 failures)]
```

### Key Architectural Tenets:
1. **Weighted Domain Rubrics per Category**:
   - *Clean Code & Refactoring*: SOLID Compliance (30%), Readability/Naming (20%), Edge Case Handling (25%), Performance/Allocations (25%).
   - *Coding / Algorithms*: Correctness & Edge Cases (35%), Time/Space Complexity (30%), Code Organization (20%), Idiomatic C# (15%).
   - *System Design*: Architecture & Scalability (35%), Resilience & Fault Tolerance (25%), Data Consistency & Trade-offs (25%), Security & Observability (15%).
   - *.NET Deep Dive*: Memory/GC & Allocations (30%), Async/Await & Concurrency (30%), Framework Idioms (25%), Exception Handling (15%).
   - *API & Distributed Systems*: Contract & Idempotency (30%), Error Handling & HTTP/gRPC semantics (25%), Resilience Patterns (25%), Observability/Security (20%).
   - *Testing & TDD*: Test Coverage & Boundary Cases (35%), Test Clarity & AAA Pattern (25%), Mutation/Failure Resilience (20%), Mocking Pragmatism (20%).
   - *Behavioral (STAR)*: Situation & Task Clarity (25%), Action & Leadership Ownership (35%), Measurable Result & Impact (25%), Communication & Reflection (15%).
2. **Deterministic Aggregation**: The LLM *never* computes or outputs the final overall grade. It only scores individual criteria with cited textual evidence. The domain engine computes the weighted mathematical sum deterministically.
3. **Low Temperature & Strict Formatting**: LLM calls run at temperature `0.1` to maximize consistency.
4. **Self-Correction & Fallback**: If the LLM generates unparseable JSON, a corrective prompt is executed automatically. If the provider is unavailable, graceful degradation is surfaced to the candidate.

## Consequences
- **Positive**: Completely auditable and transparent scoring, zero hallucinated final scores, reproducible feedback with cited code lines.
- **Negative/Mitigation**: Additional tokens evaluated per submission; mitigated by optimized JSON schemas and Groq/Llama-3 fast inference.
