using SwissTechTrainer.Domain.Enums;
using SwissTechTrainer.Domain.ValueObjects;

namespace SwissTechTrainer.Domain.Entities;

public class Evaluation
{
    public const double PassScoreThreshold = 90.0;

    public Guid Id { get; private set; }
    public Guid SubmissionId { get; private set; }
    public EvaluationStatus Status { get; private set; } = EvaluationStatus.Pending;
    public double DeterministicScore { get; private set; }
    public bool PassedThreshold { get; private set; }
    public string GeneralFeedback { get; private set; } = string.Empty;
    public string LlmModelUsed { get; private set; } = string.Empty;
    public long ProcessingDurationMs { get; private set; }
    public DateTime EvaluatedAt { get; private set; } = DateTime.UtcNow;

    private readonly List<CriterionScore> _criteriaScores = new();
    public IReadOnlyCollection<CriterionScore> CriteriaScores => _criteriaScores.AsReadOnly();

    private readonly List<CodeDiffSnippet> _codeSuggestions = new();
    public IReadOnlyCollection<CodeDiffSnippet> CodeSuggestions => _codeSuggestions.AsReadOnly();

    // Navigation
    public Submission? Submission { get; private set; }

    private Evaluation() { }

    public Evaluation(Guid submissionId)
    {
        Id = Guid.NewGuid();
        SubmissionId = submissionId;
        Status = EvaluationStatus.Pending;
        EvaluatedAt = DateTime.UtcNow;
    }

    public void MarkAnalyzing()
    {
        Status = EvaluationStatus.Analyzing;
    }

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
        LlmModelUsed = llmModelUsed ?? "Unknown";
        ProcessingDurationMs = durationMs;
        EvaluatedAt = DateTime.UtcNow;

        // Pure Deterministic Calculation in C#
        DeterministicScore = CalculateWeightedScore(_criteriaScores);
        PassedThreshold = DeterministicScore >= PassScoreThreshold;
        Status = EvaluationStatus.Completed;
    }

    public void MarkFailed(string reason)
    {
        Status = EvaluationStatus.Failed;
        GeneralFeedback = $"Evaluation failed: {reason}";
        DeterministicScore = 0;
        PassedThreshold = false;
        EvaluatedAt = DateTime.UtcNow;
    }

    public static double CalculateWeightedScore(IEnumerable<CriterionScore> criteria)
    {
        var list = criteria?.ToList() ?? new List<CriterionScore>();
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
