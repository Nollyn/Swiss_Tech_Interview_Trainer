using SwissTechTrainer.Domain.Enums;
using SwissTechTrainer.Domain.ValueObjects;

namespace SwissTechTrainer.Domain.Services;

/// <summary>
/// Provides domain-authoritative weighted rubric specifications for each of the 7 Swiss tech hiring assessment dimensions.
/// </summary>
public static class CategoryRubricCatalog
{
    /// <summary>
    /// Gets the official list of weighted criteria definitions for the specified interview category using the default C# (.NET) profile.
    /// </summary>
    /// <param name="category">The target interview category.</param>
    /// <returns>A read-only collection of <see cref="RubricDefinition"/> objects representing the category's evaluation dimensions.</returns>
    public static IReadOnlyList<RubricDefinition> GetRubricForCategory(CategoryType category) =>
        GetRubricForCategory(category, ProgrammingLanguage.CSharp);

    /// <summary>
    /// Gets the official list of weighted criteria definitions for the specified interview category and target backend programming language.
    /// </summary>
    /// <param name="category">The target interview category.</param>
    /// <param name="language">The candidate's target programming language.</param>
    /// <returns>A read-only collection of <see cref="RubricDefinition"/> objects calibrated for the specified language.</returns>
    public static IReadOnlyList<RubricDefinition> GetRubricForCategory(CategoryType category, ProgrammingLanguage language) => category switch
    {
        CategoryType.CleanCode => language switch
        {
            ProgrammingLanguage.Python =>
            [
                new("SOLID & Pythonic Design Principles", 30.0, "Adherence to Single Responsibility, duck typing with protocols, and idiomatic Pythonic abstractions."),
                new("PEP 8, Readability & Modern Type Hinting", 20.0, "Clarity of naming, clean function decomposition, and strict PEP 484/585 type annotations."),
                new("Exception Handling & Defensive Boundaries", 25.0, "Custom domain exceptions, clean context managers (with statements), and robust input validation."),
                new("Memory & Computational Efficiency", 25.0, "Efficient generator usage, avoiding quadratic complexity, and reference cycle prevention.")
            ],
            ProgrammingLanguage.Java =>
            [
                new("SOLID Principles & Clean Architecture", 30.0, "Adherence to SOLID, clean interface segregation, and dependency inversion in Java."),
                new("Readability, Clean Naming & Structure", 20.0, "Clarity of identifiers, method decomposition, and avoidance of excessive boilerplate."),
                new("Edge Case & Exception Hierarchy", 25.0, "Appropriate use of unchecked vs checked exceptions, Optional handling, and defensive copying."),
                new("Allocation & Collection Efficiency", 25.0, "Optimal collection choices, stream usage without unnecessary boxing, and memory efficiency.")
            ],
            ProgrammingLanguage.Rust =>
            [
                new("Idiomatic Rust & Trait Design", 30.0, "Effective trait definition, clean separation of concerns, and composition over inheritance."),
                new("Readability & Expressive Pattern Matching", 20.0, "Clean naming, concise match expressions, and minimal cognitive complexity."),
                new("Robust Error Handling (Result/Option)", 25.0, "Idiomatic error propagation (using '?' operator, thiserror/anyhow), avoiding unwrap/panic in library code."),
                new("Zero-Cost Memory & Allocation Efficiency", 25.0, "Minimal heap allocations, borrowing over cloning (&str/[T] vs String/Vec), and cache efficiency.")
            ],
            ProgrammingLanguage.Go =>
            [
                new("Idiomatic Go Simplicity & Interfaces", 30.0, "Small focused interfaces, composition over deep nesting, and adherence to Go package layout."),
                new("Readability & Clear Naming Conventions", 20.0, "Go naming conventions, short variable names in local scopes, and clear exported identifiers."),
                new("Explicit Error Handling & Guard Rails", 25.0, "Explicit 'if err != nil' handling, error wrapping (fmt.Errorf %w), and defensive nil checks."),
                new("Memory Allocation & Garbage Efficiency", 25.0, "Minimizing heap escapes, pre-allocating slices/maps, and avoiding unnecessary sync/mutex contention.")
            ],
            ProgrammingLanguage.NodeJs =>
            [
                new("SOLID & Modular JavaScript/TypeScript", 30.0, "Single Responsibility, clean modular exports, and strong TypeScript interfaces/types."),
                new("Readability, Clean Naming & Structure", 20.0, "Clean functional/class decomposition, strict linting compliance, and readable async flow."),
                new("Defensive Boundary & Error Handling", 25.0, "Explicit domain error classes, unhandled rejection prevention, and schema validation (Zod/Joi)."),
                new("Event Loop & Memory Leak Prevention", 25.0, "Absence of event listener leaks, closure memory retention, and non-blocking CPU operations.")
            ],
            _ => // C#
            [
                new("SOLID Principles Compliance", 30.0, "Adherence to Single Responsibility, Open/Closed, Liskov, Interface Segregation, and Dependency Inversion."),
                new("Readability, Clean Naming & Structure", 20.0, "Clarity of identifiers, method decomposition, and elimination of cognitive complexity."),
                new("Edge Case & Defensive Handling", 25.0, "Null safety, boundary checks, argument validation, and domain exception handling."),
                new("Performance & Allocation Efficiency", 25.0, "Absence of unnecessary allocations, LINQ over-allocation in hot paths, and memory leaks.")
            ]
        },

        CategoryType.Coding =>
        [
            new("Algorithmic Correctness & Edge Cases", 35.0, "Produces correct results across standard, boundary, and pathological inputs."),
            new("Time & Space Complexity", 30.0, "Optimal asymptotic Big-O runtime and memory utilization for production workloads."),
            new("Code Organization & Modularity", 20.0, "Clean functional/procedural separation and understandable flow."),
            new($"Idiomatic {language.GetDisplayName()} Usage", 15.0, $"Effective use of modern {language.GetDisplayName()} language features, standard libraries, and idioms.")
        ],

        CategoryType.SystemDesign =>
        [
            new("Distributed Architecture & Scalability", 35.0, "Appropriate component boundaries, load balancing, partitioning, and throughput planning."),
            new("Resilience, Failover & Fault Tolerance", 25.0, "Circuit breakers, dead-letter queues, retries, and graceful degradation strategies."),
            new("Data Consistency & Trade-off Justification", 25.0, "Clear justification of CAP theorem choices, event sourcing vs CRUD, and isolation levels."),
            new("Observability, Security & Compliance", 15.0, "Distributed tracing (OpenTelemetry), structured logging, metrics, and Swiss banking compliance awareness.")
        ],

        CategoryType.LanguageDeepDive => language switch
        {
            ProgrammingLanguage.Python =>
            [
                new("GIL, Memory & Reference Counting", 30.0, "Understanding of the Global Interpreter Lock (GIL), cyclic garbage collection, and __slots__ optimization."),
                new("Asyncio & Event Loop Architecture", 30.0, "Asyncio event loops, coroutines, Tasks, Futures, async context managers, and non-blocking I/O."),
                new("Decorators, Generators & Metaprogramming", 25.0, "Custom decorators, generator pipelines (yield), contextlib, and dunder/magic method mechanics."),
                new("Type System & Exception Resilience", 15.0, "Modern type annotations (typing / Protocol / Generic), clean error hierarchies, and defensive logging.")
            ],
            ProgrammingLanguage.Java =>
            [
                new("JVM Architecture & Modern GC (G1/ZGC)", 30.0, "Understanding of JVM heap layout, GC algorithms (G1, ZGC, Shenandoah), and memory tuning."),
                new("Concurrency Utilities & Java Memory Model", 30.0, "Executors, Virtual Threads (Project Loom), volatile/synchronized, java.util.concurrent locks and atomics."),
                new("JIT Compilation & Framework Mechanics", 25.0, "HotSpot JIT compilation (C1/C2 tiers), ClassLoader hierarchy, reflection overhead, and Spring/Quarkus lifecycle."),
                new("Modern Java Idioms & Fault Tolerance", 15.0, "Records, pattern matching, Sealed classes, Streams performance, and structured concurrency.")
            ],
            ProgrammingLanguage.Rust =>
            [
                new("Ownership, Borrow Checker & Lifetimes", 35.0, "Exemplary understanding of ownership transfers, mutable vs immutable borrows, and explicit lifetime annotations."),
                new("Zero-Cost Abstractions & Memory Layout", 25.0, "Stack vs Heap (Box, Rc, Arc), struct memory alignment, traits vs trait objects (dyn), and monomorphization."),
                new("Fearless Concurrency & Thread Safety", 25.0, "Send and Sync traits, Arc/Mutex vs RwLock, channel communication (crossbeam/std::sync::mpsc), and atomic primitives."),
                new("Resilient Error Handling & Panic Prevention", 15.0, "Idiomatic Result/Option handling, custom error types, preventing panics in production code.")
            ],
            ProgrammingLanguage.Go =>
            [
                new("Goroutines, Channels & CSP Concurrency", 30.0, "Goroutine lifecycle, buffered/unbuffered channels, select statements, deadlock/race detection (go test -race)."),
                new("Memory Allocation & Escape Analysis", 30.0, "Stack vs heap escapes (escape analysis), pointer semantics, slice memory headers, and sync.Pool utilization."),
                new("Interfaces & Composition Mechanics", 25.0, "Implicit interface implementation, struct embedding, type assertions, and reflection tradeoffs."),
                new("Context Propagation & Cancellation", 15.0, "Idiomatic context.Context propagation, timeout/deadline enforcement, and graceful shutdown patterns.")
            ],
            ProgrammingLanguage.NodeJs =>
            [
                new("V8 Engine Internals & Garbage Collection", 30.0, "V8 heap spaces (New/Old space), Scavenge vs Mark-Sweep GC, inline caching, and hidden classes."),
                new("Event Loop Phases & Microtask Queue", 30.0, "Event loop phases (timers, I/O polling, check), process.nextTick vs Promise microtasks, and blocking avoidance."),
                new("Stream Processing & Async I/O", 25.0, "Node.js Streams (Readable, Writable, Transform), pipeline backpressure handling, and Buffer allocation safety."),
                new("Worker Threads & Clustering", 15.0, "Worker threads for CPU-bound tasks, cluster module multi-core utilization, and IPC messaging resilience.")
            ],
            _ => // C# (.NET)
            [
                new("Memory Management, GC & Allocations", 30.0, "Understanding of Stack vs Heap, GC Generations, IDisposable/ref structs, ArrayPool, Memory<T>."),
                new("Async/Await Internals & Concurrency", 30.0, "SynchronizationContext, Task vs ValueTask, ConfigureAwait, deadlock prevention, thread pool starvation."),
                new("CLR Execution & Framework Idioms", 25.0, "JIT compilation, reflection vs source generators, dependency injection lifecycles, LINQ deferred execution."),
                new("Robust Exception & Cancellation Handling", 15.0, "CancellationToken propagation, AggregateException handling, and resilient error pipelines.")
            ]
        },

        CategoryType.ApiDesign =>
        [
            new("Contract Design & Idempotency", 30.0, "RESTful / gRPC contract clarity, idempotent mutation endpoints (Idempotency-Key headers), schema versioning."),
            new("HTTP/gRPC Semantics & Error Modeling", 25.0, "RFC 7807 Problem Details, correct HTTP status codes, structured error payloads."),
            new("Resilience & Rate Limiting", 25.0, "Resilience pipelines, token bucket rate limiting, timeout and circuit breaker policies."),
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
