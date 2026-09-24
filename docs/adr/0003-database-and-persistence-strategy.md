# ADR 0003: Database and Persistence Strategy (PostgreSQL & EF Core with Local In-Memory / SQLite Fallback)

## Status
Accepted

## Context
The application must persist user sessions, current progress across 7 categories (Levels 1-5), generated exercises, candidate submission code, evaluation criteria scores, and attempt histories.

In production and containerized environments, PostgreSQL is the standard enterprise relational database. However, for zero-friction local development, CI test runs, and standalone mode, the persistence layer must be decoupled and flexible.

## Decision
We use **Entity Framework Core** with **PostgreSQL** (`Npgsql.EntityFrameworkCore.PostgreSQL`) as the primary database provider, while architecting DbContext configurations to seamlessly support **SQLite / In-Memory** for local quickstart and automated integration testing without requiring an external container.

### Persistence Boundaries:
1. **Candidate Progress Context**: Tracks `UserId`, `CategoryType`, `CurrentLevel` (1-5), `ConsecutiveFailures`, and `UnlockedDate`.
2. **Exercise Catalog Context**: Stores `Exercise` entities with `Category`, `Level`, `Title`, `Description`, `StarterCode`, `RubricDefinition`, and `ExpectedOutputFormat`.
3. **Submission & Evaluation Context**: Stores `SubmissionAttempt`, raw submitted code/payload, `EvaluationResult`, breakdown of `CriterionScore`s, generated suggestions, and `CreatedAt` timestamp.

## Consequences
- **Positive**: Clean relational schema with relational integrity and transaction boundaries. Fully compatible with standard Docker Compose setups and CI pipelines.
- **Negative/Mitigation**: SQLite dialect differences in complex JSON operations are avoided by storing structured sub-entities or standard serializations.
