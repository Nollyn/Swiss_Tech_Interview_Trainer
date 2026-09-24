using SwissTechTrainer.Domain.Enums;
using SwissTechTrainer.Domain.ValueObjects;

namespace SwissTechTrainer.Domain.Entities;

/// <summary>
/// Represents the completed or in-progress evaluation of a submission, encapsulating rubric scores, actionable diffs, and deterministic grade calculations.
/// </summary>
public class Evaluation
{
    /// <summary>
    /// The strict minimum deterministic score percentage required to pass a level (90.0%).
    /// </summary>
    public const double PassScoreThreshold = 90.0;

    private readonly List<CriterionScore> _criteriaScores = [];
    private readonly List<CodeDiffSnippet> _codeSuggestions = [];

    /// <summary>
    /// Gets the unique identifier for the evaluation.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the target submission ID this evaluation belongs to.
    /// </summary>
    public Guid SubmissionId { get; private set; }

    /// <summary>
    /// Gets the current processing status of the evaluation.
    /// </summary>
    public EvaluationStatus Status { get; private set; } = EvaluationStatus.Pending;

    /// <summary>
    /// Gets the aggregated deterministic score computed from weighted criteria (0.0 to 100.0).
    /// </summary>
    public double DeterministicScore { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the deterministic score meets or exceeds the <see cref="PassScoreThreshold"/>.
    /// </summary>
    public bool PassedThreshold { get; private set; }

    /// <summary>
    /// Gets the comprehensive feedback synthesis in Markdown format.
    /// </summary>
    public string GeneralFeedback { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the identifier of the LLM model/provider that performed the analysis.
    /// </summary>
    public string LlmModelUsed { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the total processing duration in milliseconds.
    /// </summary>
    public long ProcessingDurationMs { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the evaluation was completed or failed.
    /// </summary>
    public DateTime EvaluatedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the breakdown of criteria evaluations with cited evidence and individual scores.
    /// </summary>
    public IReadOnlyCollection<CriterionScore> CriteriaScores => _criteriaScores.AsReadOnly();

    /// <summary>
    /// Gets the actionable code improvement recommendations ("before/after" diffs).
    /// </summary>
    public IReadOnlyCollection<CodeDiffSnippet> CodeSuggestions => _codeSuggestions.AsReadOnly();

    /// <summary>
    /// Gets the navigation property to the parent submission.
    /// </summary>
    public Submission? Submission { get; private set; }

    /// <summary>
    /// Parameterless constructor required by EF Core.
    /// </summary>
    private Evaluation() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Evaluation"/> entity in a pending state.
    /// </summary>
    /// <param name="submissionId">The target submission ID.</param>
    /// <exception cref="ArgumentException">Thrown when submissionId is empty.</exception>
    public Evaluation(Guid submissionId)
    {
        if (submissionId == Guid.Empty)
            throw new ArgumentException("SubmissionId cannot be empty.", nameof(submissionId));

        Id = Guid.NewGuid();
        SubmissionId = submissionId;
        Status = EvaluationStatus.Pending;
        EvaluatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to create a new pending <see cref="Evaluation"/> entity.
    /// </summary>
    /// <param name="submissionId">The target submission ID.</param>
    /// <returns>A new <see cref="Evaluation"/> instance in Pending status.</returns>
    public static Evaluation CreatePending(Guid submissionId)
    {
        return new Evaluation(submissionId);
    }

    /// <summary>
    /// Transitions the evaluation status to analyzing.
    /// </summary>
    public void MarkAnalyzing()
    {
        Status = EvaluationStatus.Analyzing;
    }

    /// <summary>
    /// Completes the evaluation, applies deterministic weighted aggregation, and computes the pass/fail result.
    /// </summary>
    /// <param name="criteria">The collection of evaluated criteria.</param>
    /// <param name="suggestions">The actionable refactoring suggestions.</param>
    /// <param name="generalFeedback">The narrative markdown feedback.</param>
    /// <param name="llmModelUsed">The LLM model provider name.</param>
    /// <param name="durationMs">The execution duration in milliseconds.</param>
    /// <exception cref="ArgumentNullException">Thrown when criteria is null.</exception>
    public void Complete(
        IEnumerable<CriterionScore> criteria,
        IEnumerable<CodeDiffSnippet> suggestions,
        string generalFeedback,
        string llmModelUsed,
        long durationMs)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        _criteriaScores.Clear();
        _criteriaScores.AddRange(criteria);

        _codeSuggestions.Clear();
        if (suggestions != null)
        {
            _codeSuggestions.AddRange(suggestions);
        }

        GeneralFeedback = generalFeedback ?? string.Empty;
        LlmModelUsed = string.IsNullOrWhiteSpace(llmModelUsed) ? "Unknown" : llmModelUsed.Trim();
        ProcessingDurationMs = durationMs;
        EvaluatedAt = DateTime.UtcNow;

        // Pure Deterministic Calculation in C#
        DeterministicScore = CalculateWeightedScore(_criteriaScores);
        PassedThreshold = DeterministicScore >= PassScoreThreshold;
        Status = EvaluationStatus.Completed;
    }

    /// <summary>
    /// Marks the evaluation as failed with a diagnostic reason.
    /// </summary>
    /// <param name="reason">The failure description or exception message.</param>
    public void MarkFailed(string reason)
    {
        Status = EvaluationStatus.Failed;
        GeneralFeedback = $"Evaluation failed: {reason}";
        DeterministicScore = 0.0;
        PassedThreshold = false;
        EvaluatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deterministically calculates the weighted average score rounded to one decimal place.
    /// </summary>
    /// <param name="criteria">The collection of criterion scores and weights.</param>
    /// <returns>The mathematical weighted score (0.0 to 100.0).</returns>
    public static double CalculateWeightedScore(IEnumerable<CriterionScore> criteria)
    {
        var list = criteria?.ToList() ?? [];
        if (list.Count == 0)
            return 0.0;

        double totalWeight = list.Sum(c => c.Weight);
        if (totalWeight <= 0)
            return 0.0;

        double weightedSum = list.Sum(c => c.Score * c.Weight);
        double result = weightedSum / totalWeight;

        // Round to 1 decimal place deterministically
        return Math.Round(result, 1, MidpointRounding.AwayFromZero);
    }
}
