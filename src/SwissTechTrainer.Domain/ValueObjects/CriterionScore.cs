namespace SwissTechTrainer.Domain.ValueObjects;

public sealed record CriterionScore
{
    public string Name { get; init; } = string.Empty;
    public double Weight { get; init; }
    public double Score { get; init; } // 0 to 100
    public string Evidence { get; init; } = string.Empty;
    public string Rationale { get; init; } = string.Empty;

    public CriterionScore() { }

    public CriterionScore(string name, double weight, double score, string evidence, string rationale)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Criterion name cannot be empty.", nameof(name));
        
        if (weight <= 0)
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be greater than 0.");

        Name = name;
        Weight = weight;
        Score = Math.Clamp(score, 0, 100);
        Evidence = evidence ?? string.Empty;
        Rationale = rationale ?? string.Empty;
    }
}
