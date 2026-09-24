namespace SwissTechTrainer.Domain.ValueObjects;

public sealed record RubricDefinition
{
    public string Name { get; init; } = string.Empty;
    public double Weight { get; init; }
    public string Description { get; init; } = string.Empty;

    public RubricDefinition() { }

    public RubricDefinition(string name, double weight, string description)
    {
        Name = name;
        Weight = weight;
        Description = description;
    }
}
