using SwissTechTrainer.Domain.Enums;
using SwissTechTrainer.Domain.ValueObjects;

namespace SwissTechTrainer.Domain.Services;

/// <summary>
/// Provides domain-authoritative weighted rubric specifications for each of the 7 Swiss tech hiring assessment dimensions.
/// </summary>
public static class CategoryRubricCatalog
{
    /// <summary>
    /// Gets the official list of weighted criteria definitions for the specified interview category.
    /// </summary>
    /// <param name="category">The target interview category.</param>
    /// <returns>A read-only collection of <see cref="RubricDefinition"/> objects representing the category's evaluation dimensions.</returns>
    public static IReadOnlyList<RubricDefinition> GetRubricForCategory(CategoryType category) => category switch
    {
        CategoryType.CleanCode =>
        [
            new("SOLID Principles Compliance", 30.0, "Adherence to Single Responsibility, Open/Closed, Liskov, Interface Segregation, and Dependency Inversion."),
            new("Readability, Clean Naming & Structure", 20.0, "Clarity of identifiers, method decomposition, and elimination of cognitive complexity."),
            new("Edge Case & Defensive Handling", 25.0, "Null safety, boundary checks, argument validation, and domain exception handling."),
            new("Performance & Allocation Efficiency", 25.0, "Absence of unnecessary allocations, LINQ over-allocation in hot paths, and memory leaks.")
        ],

        CategoryType.Coding =>
        [
            new("Algorithmic Correctness & Edge Cases", 35.0, "Produces correct results across standard, boundary, and pathological inputs."),
            new("Time & Space Complexity", 30.0, "Optimal asymptotic Big-O runtime and memory utilization for production workloads."),
            new("Code Organization & Modularity", 20.0, "Clean functional/procedural separation and understandable flow."),
            new("Idiomatic Modern C# Usage", 15.0, "Effective use of modern C# features (records, pattern matching, spans where appropriate).")
        ],

        CategoryType.SystemDesign =>
        [
            new("Distributed Architecture & Scalability", 35.0, "Appropriate component boundaries, load balancing, partitioning, and throughput planning."),
            new("Resilience, Failover & Fault Tolerance", 25.0, "Circuit breakers, dead-letter queues, retries, and graceful degradation strategies."),
            new("Data Consistency & Trade-off Justification", 25.0, "Clear justification of CAP theorem choices, event sourcing vs CRUD, and isolation levels."),
            new("Observability, Security & Compliance", 15.0, "Distributed tracing (OpenTelemetry), structured logging, metrics, and Swiss banking compliance awareness.")
        ],

        CategoryType.DotNetDeepDive =>
        [
            new("Memory Management, GC & Allocations", 30.0, "Understanding of Stack vs Heap, GC Generations, IDisposable/ref structs, ArrayPool, Memory<T>."),
            new("Async/Await Internals & Concurrency", 30.0, "SynchronizationContext, Task vs ValueTask, ConfigureAwait, deadlock prevention, thread pool starvation."),
            new("CLR Execution & Framework Idioms", 25.0, "JIT compilation, reflection vs source generators, dependency injection lifecycles, LINQ deferred execution."),
            new("Robust Exception & Cancellation Handling", 15.0, "CancellationToken propagation, AggregateException handling, and resilient error pipelines.")
        ],

        CategoryType.ApiDesign =>
        [
            new("Contract Design & Idempotency", 30.0, "RESTful / gRPC contract clarity, idempotent mutation endpoints (Idempotency-Key headers), schema versioning."),
            new("HTTP/gRPC Semantics & Error Modeling", 25.0, "RFC 7807 Problem Details, correct HTTP status codes, structured error payloads."),
            new("Resilience & Rate Limiting", 25.0, "Polly / resilience pipelines, token bucket rate limiting, timeout policies."),
            new("Security & Observability Integration", 20.0, "Authentication/Authorization (OAuth2/JWT), health checks, correlation IDs, and rate limit telemetry.")
        ],

        CategoryType.Testing =>
        [
            new("Test Coverage & Boundary Conditions", 35.0, "Thorough coverage of happy path, negative branches, and corner cases."),
            new("Test Clarity & AAA (Arrange-Act-Assert)", 25.0, "Readability of unit tests, single assertion focus, clear naming conventions (Given_When_Then)."),
            new("Mutation & Failure Resilience", 20.0, "Tests that genuinely fail when behavior changes rather than brittle implementation assertions."),
            new("Mocking & Test Double Pragmatism", 20.0, "Appropriate use of fakes vs mocks, avoiding over-mocking of domain entities.")
        ],

        CategoryType.Behavioral =>
        [
            new("Situation & Task Definition (STAR S&T)", 25.0, "Concise context setting, high-stakes technical or organizational challenge description."),
            new("Action & Technical Leadership (STAR A)", 35.0, "Explicit individual ownership, architectural mediation, mentorship, and decisive action."),
            new("Measurable Result & Business Impact (STAR R)", 25.0, "Quantifiable outcomes (uptime %, latency reduction, delivery velocity, team growth)."),
            new("Reflection & Communication Maturity", 15.0, "Lessons learned, humility, empathy, and executive-level articulation.")
        ],

        _ =>
        [
            new("Technical Quality", 50.0, "General technical correctness and quality."),
            new("Design & Clarity", 50.0, "Clarity and architectural design.")
        ]
    };
}
