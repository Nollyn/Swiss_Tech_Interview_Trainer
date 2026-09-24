namespace SwissTechTrainer.Domain.ValueObjects;

/// <summary>
/// Represents an immutable evaluation result for a single rubric criterion, containing its mathematical score, textual evidence, and rationale.
/// </summary>
public sealed record CriterionScore
{
    /// <summary>
    /// Gets the rubric dimension identifier (e.g. "SOLID Principles Compliance").
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the relative weight of the criterion in the overall calculation (e.g., 30.0 for 30%).
    /// </summary>
    public double Weight { get; init; }

    /// <summary>
    /// Gets the numerical score between 0.0 and 100.0 awarded by the evaluation engine.
    /// </summary>
    public double Score { get; init; }

    /// <summary>
    /// Gets the cited textual code snippet or quotation from the candidate's submission that motivated the score.
    /// </summary>
    public string Evidence { get; init; } = string.Empty;

    /// <summary>
    /// Gets the architectural reasoning and constructive feedback explaining the assigned score.
    /// </summary>
    public string Rationale { get; init; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="CriterionScore"/> record. Parameterless constructor for serialization.
    /// </summary>
    public CriterionScore() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CriterionScore"/> record with domain validation.
    /// </summary>
    /// <param name="name">The name of the evaluated criterion.</param>
    /// <param name="weight">The positive weight factor for this criterion.</param>
    /// <param name="score">The score from 0.0 to 100.0 (clamped if out of range).</param>
    /// <param name="evidence">The cited code evidence or snippet from the user's solution.</param>
    /// <param name="rationale">The actionable architectural explanation for the score.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="weight"/> is less than or equal to 0.</exception>
    public CriterionScore(string name, double weight, double score, string evidence, string rationale)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Criterion name cannot be empty.", nameof(name));
        
        if (weight <= 0)
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be greater than zero.");

        Name = name.Trim();
        Weight = weight;
        Score = Math.Clamp(score, 0.0, 100.0);
        Evidence = evidence?.Trim() ?? string.Empty;
        Rationale = rationale?.Trim() ?? string.Empty;
    }
}
