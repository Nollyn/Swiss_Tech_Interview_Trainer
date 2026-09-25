# ADR 0005: Adaptive Difficulty Progression Model & Anti-Frustration Mechanics

## Status
Accepted

## Context
Senior interview preparation requires disciplined progression. Allowing candidates to skip ahead without true mastery diminishes preparation efficacy, while getting permanently stuck causes demoralization.

Swiss tech recruitment for Senior and Tech Lead positions emphasizes depth of knowledge, pragmatic trade-offs, and resilience.

## Decision
We implement a **Specification-driven Adaptive Progression Model**:

1. **Category Taxonomy**:
   - `Coding`: Algorithms & Data Structures (Pragmatic / European Senior style)
   - `SystemDesign`: Distributed Service Design & Trade-offs (Tech Lead scope)
   - `LanguageDeepDive`: CLR Memory Management, Concurrency, Performance, Internals
   - `CleanCode`: Refactoring legacy/smelly code, SOLID compliance
   - `ApiDesign`: REST/gRPC, Idempotency, Distributed Resilience
   - `Testing`: TDD, boundary analysis, resilient test suites
   - `Behavioral`: STAR Method leadership & technical conflict resolution
2. **Progression Ladder (Levels 1 to 5)**:
   - Level 1: Core fundamentals and idioms.
   - Level 2: Real-world business requirements with error handling.
   - Level 3: Concurrency, performance, edge-case pressure.
   - Level 4: Distributed failure modes, architectural refactoring, trade-offs.
   - Level 5: Complex multi-constraint optimization, resilience, senior leadership scenario.
3. **Unlock Gate & Rules**:
   - Progression Rule: Score $\ge 90\%$ is strictly required to unlock Level $N+1$.
   - Failure Handling: Score $< 90\%$ maintains the current level and logs the attempt.
   - **Anti-Frustration Trigger**: If a candidate records $\ge 3$ consecutive failed attempts on the same category and level, the system activates **Hint Mode**. The next exercise or retry provides architectural hints and diagnostic questions to guide the candidate.

## Consequences
- **Positive**: Encourages deep mastery while offering structured scaffolding when candidates struggle.
- **Negative/Mitigation**: High standard may feel demanding; addressed by rich constructive feedback and hint generation.
