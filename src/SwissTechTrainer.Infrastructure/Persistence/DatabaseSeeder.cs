using Microsoft.EntityFrameworkCore;
using SwissTechTrainer.Domain.Entities;
using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Infrastructure.Persistence;

/// <summary>
/// Initializes and seeds default candidate profile data and baseline Level 1 exercises across all 7 Swiss hiring dimensions.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds initial user profile and baseline exercises into the database if not already populated.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="ct">Cancellation token.</param>
    public static async Task SeedAsync(AppDbContext context, CancellationToken ct = default)
    {
        // 1. Seed default User Profile
        var defaultUser = await context.UserProfiles.Include(u => u.Progresses).FirstOrDefaultAsync(ct);
        if (defaultUser == null)
        {
            defaultUser = UserProfile.Create("SwissTechLead_Candidate", "candidate.zurich@swissdev.ch", "Senior .NET Developer / Tech Lead (Zurich)");
            context.UserProfiles.Add(defaultUser);
            await context.SaveChangesAsync(ct);
        }

        // 2. Seed Level 1 initial exercises across all 7 categories if none exist
        if (!await context.Exercises.AnyAsync(ct))
        {
            var exercises = new List<Exercise>
            {
                // 1. Coding / Algorithms
                new(
                    category: CategoryType.Coding,
                    level: DifficultyLevel.Level1,
                    title: "Pragmatic Order Stream Sliding Window Aggregator",
                    description: """
### Zurich Financial Exchange — Order Stream Rate & Volume Tracker

In Swiss electronic trading and fintech platforms, tracking sliding window order volumes with minimal memory allocation is essential.

#### Problem Specification
Implement an in-memory `OrderVolumeTracker` that records trade orders timestamped in milliseconds and calculates:
1. Total volume of trades executed within the last `W` milliseconds (sliding window).
2. Number of trades within the window.

#### Constraints
- Runtime complexity for `RecordOrder` and `GetWindowVolume` must be $O(1)$ amortized.
- Thread-safety is required (concurrent read/write access).
- Memory allocations in the hot path must be minimal.
""",
                    starterCode: """
using System;
using System.Collections.Concurrent;

namespace SwissTrading.Fintech;

public record Order(string OrderId, decimal Amount, long TimestampMs);

public interface IOrderVolumeTracker
{
    void RecordOrder(Order order);
    decimal GetWindowVolume(long currentTimestampMs, long windowDurationMs);
    int GetWindowOrderCount(long currentTimestampMs, long windowDurationMs);
}

public class OrderVolumeTracker : IOrderVolumeTracker
{
    // TODO: Implement thread-safe sliding window aggregator
    public void RecordOrder(Order order)
    {
        throw new NotImplementedException();
    }

    public decimal GetWindowVolume(long currentTimestampMs, long windowDurationMs)
    {
        throw new NotImplementedException();
    }

    public int GetWindowOrderCount(long currentTimestampMs, long windowDurationMs)
    {
        throw new NotImplementedException();
    }
}
""",
                    hints: "Consider using a thread-safe circular buffer or ConcurrentQueue with monotonic timestamps. Evict expired entries during queries or records."
                ),

                // 2. System Design
                new(
                    category: CategoryType.SystemDesign,
                    level: DifficultyLevel.Level1,
                    title: "Swiss QR-Bill Payment Notification & Reconciliation Service",
                    description: """
### Swiss Interbank Payment Notification Service (ISO 20022 / QR-Bill)

You are designing a backend reconciliation microservice for a private Swiss cantonal bank that processes QR-bill payments received via SIX Interbank Clearing (SIC).

#### Requirements
1. Ingest payment notifications from clearing gateway (up to 2,000 notifications/sec during peak morning settlement).
2. Guarantee exactly-once processing (Idempotency) against duplicate clearing webhooks.
3. Decouple notification ingestion from downstream ledger core banking updates.
4. Provide structured architectural justification and ASCII/Mermaid component diagram.

#### Deliverable
Provide a concise architectural design document with:
- Component diagram (Ingress API, Message Broker, Idempotency Store, Outbox/Worker, Core Banking).
- Storage choice & partitioning strategy.
- Failure modes & recovery (Dead Letter Queue, Poison Message handling).
""",
                    starterCode: """
# Swiss QR-Bill Payment Notification & Reconciliation Service Design

## 1. Executive Architecture Overview
<!-- Describe components, data ingestion flow, and async boundaries -->

## 2. Idempotency & Exactly-Once Semantics Strategy
<!-- Detail transaction boundaries, Redis/Postgres deduplication key lifecycle -->

## 3. Resilience & Failure Handling
<!-- Detail retries, circuit breakers, and DLQ remediation -->

## 4. Observability & Swiss Banking Compliance
<!-- Logging, distributed tracing (OpenTelemetry), and audit trail -->
""",
                    expectedOutputFormat: "Markdown Architectural Design & Justification",
                    hints: "Focus on the Outbox pattern combined with transactional idempotency tables. Mention Swiss banking regulatory audit logging requirements."
                ),

                // 3. .NET / C# Deep Dive
                new(
                    category: CategoryType.DotNetDeepDive,
                    level: DifficultyLevel.Level1,
                    title: "High-Throughput Zero-Allocation Financial Message Parser",
                    description: """
### CLR Memory & Span<T> Optimization

A legacy market feed parser allocates hundreds of small strings per FIX/JSON message, causing high Gen 0/Gen 1 GC pauses during market open.

#### Requirements
1. Refactor the parser to use `ReadOnlySpan<char>` and `Span<T>` slicing instead of `string.Substring()` and `string.Split()`.
2. Parse key-value pairs (e.g. `Tag=Value;Tag2=Value2`) without allocating heap strings for keys or numeric values.
3. Ensure zero memory allocations in the parsing loop.
""",
                    starterCode: """
using System;

namespace SwissExchange.DeepDive;

public ref struct MarketMessage
{
    public ReadOnlySpan<char> Symbol;
    public decimal Price;
    public int Volume;
}

public static class MarketMessageParser
{
    // Input format example: "SYM=NESN;PX=98.50;VOL=1200"
    // Requirement: Parse SYM, PX, and VOL without calling string.Split() or string.Substring()
    public static MarketMessage Parse(ReadOnlySpan<char> rawMessage)
    {
        // TODO: Implement zero-allocation span-based parser
        throw new NotImplementedException();
    }
}
""",
                    hints: "Use MemoryExtensions.IndexOf and Span<char>.Slice. Use decimal.TryParse and int.TryParse directly on ReadOnlySpan<char>."
                ),

                // 4. Clean Code & Refactoring
                new(
                    category: CategoryType.CleanCode,
                    level: DifficultyLevel.Level1,
                    title: "Refactor God-Class Wealth Management Fee Calculator",
                    description: """
### Wealth Management Portfolio Fee Engine Refactoring

The following class violates Single Responsibility, Open/Closed Principle, has high cyclomatic complexity, hardcoded database/email calls, and unhandled edge cases.

#### Tasks
1. Refactor into clean, decoupled domain services using Strategy and Dependency Injection.
2. Separate calculation rules (e.g. Tiered Custody Fees, Transaction Fees, Tax Deductions) to adhere to OCP.
3. Add robust defensive validation.
""",
                    starterCode: """
using System;

namespace SwissWealth.Legacy;

public class FeeCalculator
{
    public decimal CalculateAndBill(string clientType, decimal portfolioValue, int tradeCount, string email)
    {
        decimal fee = 0;
        if (clientType == "PRIVATE")
        {
            if (portfolioValue > 1000000) fee = portfolioValue * 0.002m;
            else fee = portfolioValue * 0.005m;
        }
        else if (clientType == "INSTITUTIONAL")
        {
            fee = portfolioValue * 0.0015m;
        }
        else if (clientType == "FAMILY_OFFICE")
        {
            fee = portfolioValue * 0.001m;
        }

        fee += tradeCount * 12.5m;

        // Hardcoded external call smell
        Console.WriteLine($"Sending fee invoice of {fee} CHF to {email}...");
        return fee;
    }
}
""",
                    hints: "Extract an IFeeStrategy per client type. Extract INotificationService. Make calculations pure and testable."
                ),

                // 5. API Design & Distributed Systems
                new(
                    category: CategoryType.ApiDesign,
                    level: DifficultyLevel.Level1,
                    title: "Idempotent Wealth Transfer RESTful API Endpoint",
                    description: """
### Idempotent Swiss SEPA / SIC Transfer API Controller

Design and implement a robust ASP.NET Core Minimal API endpoint or Controller for fund transfers that guarantees idempotency via the `Idempotency-Key` HTTP header.

#### Requirements
1. Accept `POST /api/v1/transfers` with body `{ sourceIban, targetIban, amount, currency }`.
2. Extract `Idempotency-Key` header; if missing, return `400 Bad Request`.
3. If an idempotency key is seen for the first time, process the transfer and cache the response.
4. If the same key is re-sent with the exact same payload, return the cached result with status `200 OK` and custom header `X-Cache-Lookup: Hit`.
5. If the same key is re-sent with a conflicting payload, return `409 Conflict`.
""",
                    starterCode: """
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SwissBank.Api;

public record TransferRequest(string SourceIban, string TargetIban, decimal Amount, string Currency);
public record TransferResponse(Guid TransferId, string Status, decimal Amount, DateTime ProcessedAt);

[ApiController]
[Route("api/v1/transfers")]
public class TransferController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> ExecuteTransfer([FromBody] TransferRequest request, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        // TODO: Implement idempotency validation, collision detection, and response caching
        throw new NotImplementedException();
    }
}
""",
                    hints: "Use an IIdempotencyStore (in-memory or distributed cache) storing hash of the request payload and the response."
                ),

                // 6. Testing & TDD
                new(
                    category: CategoryType.Testing,
                    level: DifficultyLevel.Level1,
                    title: "TDD Implementation of Swiss IBAN Validator & Luhn Modulo 97",
                    description: """
### Swiss IBAN (CH / LI) Checksum Validation via TDD

Write comprehensive unit tests and the production implementation for validating Swiss (`CH...`, 21 characters) and Liechtenstein (`LI...`, 21 characters) IBAN numbers according to ISO 13616 and MOD 97-10 (ISO 7064).

#### Requirements
1. Cover standard valid Swiss IBANs, Liechtenstein IBANs, invalid lengths, incorrect country prefixes, and bad checksum digits.
2. Follow strict AAA test structure with descriptive test names.
""",
                    starterCode: """
using System;
using Xunit;
using FluentAssertions;

namespace SwissBank.Validation.Tests;

public interface IIbanValidator
{
    bool IsValidSwissOrLiechtensteinIban(string iban);
}

public class SwissIbanValidator : IIbanValidator
{
    public bool IsValidSwissOrLiechtensteinIban(string iban)
    {
        // TODO: Implement MOD-97 algorithm for CH and LI IBAN formats
        throw new NotImplementedException();
    }
}

public class SwissIbanValidatorTests
{
    private readonly IIbanValidator _sut = new SwissIbanValidator();

    [Fact]
    public void IsValidSwissOrLiechtensteinIban_WithValidSwissIban_ReturnsTrue()
    {
        // Example valid Swiss IBAN: "CH9300762011623852957"
        // Arrange
        // Act
        // Assert
    }
}
""",
                    hints: "Rearrange the first 4 characters to the end, convert letters to numbers (A=10, Z=35), and compute Modulo 97 (remainder must equal 1)."
                ),

                // 7. Behavioral / Leadership (STAR)
                new(
                    category: CategoryType.Behavioral,
                    level: DifficultyLevel.Level1,
                    title: "Resolving Architecture Conflict & Legacy Migration Resistance",
                    description: """
### Swiss Enterprise Tech Lead Scenario: Modernization vs Delivery Deadlines

#### Scenario
You have joined a Zurich private wealth management firm as Tech Lead. The team is divided: Senior engineers want to halt feature delivery for 4 months to rewrite a legacy monolith to event-driven microservices. Product management and business stakeholders insist on delivering regulatory MIFID II compliance features on an aggressive deadline.

#### Task
Formulate your response using the **STAR method (Situation, Task, Action, Result)** demonstrating:
1. Pragmatic technical leadership and risk mitigation (e.g. Strangler Fig pattern instead of big-bang rewrite).
2. Stakeholder negotiation and transparent trade-off communication.
3. Mentoring the team and establishing evolutionary architecture practices.
""",
                    starterCode: """
# STAR Leadership Response

### 1. Situation (S)
<!-- Briefly set the context, team division, technical debt, and business regulatory deadlines -->

### 2. Task (T)
<!-- Clarify your specific responsibility and ownership as Tech Lead -->

### 3. Action (A)
<!-- Describe concrete technical and organizational steps you took (e.g. Strangler Fig pattern, domain slicing, stakeholder workshops) -->

### 4. Result & Reflection (R)
<!-- Detail the measurable outcomes (on-time regulatory delivery, incremental migration progress, team alignment, lessons learned) -->
""",
                    expectedOutputFormat: "Structured STAR Method Response (Markdown)",
                    hints: "Avoid extreme stances. Focus on evolutionary architecture (Strangler Fig), business-value alignment, and psychological safety for the engineering team."
                )
            };

            context.Exercises.AddRange(exercises);
            await context.SaveChangesAsync(ct);
        }
    }
}
