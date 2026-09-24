namespace SwissTechTrainer.Domain.ValueObjects;

/// <summary>
/// Represents an immutable evaluation rubric definition with an assigned relative weight and criteria description.
/// </summary>
public sealed record RubricDefinition
{
    /// <summary>
    /// Gets the unique criterion title (e.g., "SOLID Principles Compliance").
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the positive weight assigned to this rubric dimension (e.g., 30.0 for 30%).
    /// </summary>
    public double Weight { get; init; }

    /// <summary>
    /// Gets the detailed guidance description for assessing this dimension.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="RubricDefinition"/> record. Parameterless constructor for serialization.
    /// </summary>
    public RubricDefinition() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RubricDefinition"/> record with explicit validation of domain invariants.
    /// </summary>
    /// <param name="name">The name of the rubric dimension.</param>
    /// <param name="weight">The positive weight assigned to the rubric (must be greater than 0).</param>
    /// <param name="description">The guidance text explaining how to evaluate this dimension.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="weight"/> is less than or equal to 0.</exception>
    public RubricDefinition(string name, double weight, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Rubric criterion name cannot be empty.", nameof(name));

        if (weight <= 0)
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be greater than zero.");

        Name = name.Trim();
        Weight = weight;
        Description = description?.Trim() ?? string.Empty;
    }
}
