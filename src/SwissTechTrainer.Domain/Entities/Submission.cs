using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Domain.Entities;

/// <summary>
/// Represents a candidate's code or text submission attempt for an interview exercise.
/// </summary>
public class Submission
{
    /// <summary>
    /// Gets the unique identifier for the submission.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the target exercise ID.
    /// </summary>
    public Guid ExerciseId { get; private set; }

    /// <summary>
    /// Gets the user ID of the candidate who submitted this attempt.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the backend programming language used in this submission.
    /// </summary>
    public ProgrammingLanguage Language { get; private set; } = ProgrammingLanguage.CSharp;

    /// <summary>
    /// Gets the raw submitted source code, project summary, or behavioral text.
    /// </summary>
    public string SubmittedCode { get; private set; } = string.Empty;

    /// <summary>
    /// Gets optional candidate notes or architectural justifications.
    /// </summary>
    public string AdditionalNotes { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the format type of the submission (e.g. SingleFile, ZipArchive, TextDescription).
    /// </summary>
    public SubmissionType SubmissionType { get; private set; } = SubmissionType.SingleFile;

    /// <summary>
    /// Gets the UTC timestamp when the submission was recorded.
    /// </summary>
    public DateTime SubmittedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the navigation property to the associated exercise.
    /// </summary>
    public Exercise? Exercise { get; private set; }

    /// <summary>
    /// Gets the navigation property to the associated user profile.
    /// </summary>
    public UserProfile? User { get; private set; }

    /// <summary>
    /// Gets the navigation property to the evaluation generated for this submission.
    /// </summary>
    public Evaluation? Evaluation { get; private set; }

    /// <summary>
    /// Parameterless constructor required by EF Core.
    /// </summary>
    private Submission() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Submission"/> entity with invariant checks.
    /// </summary>
    /// <param name="exerciseId">The ID of the target exercise.</param>
    /// <param name="userId">The ID of the candidate.</param>
    /// <param name="submittedCode">The code or content submitted.</param>
    /// <param name="additionalNotes">Optional notes from the candidate.</param>
    /// <param name="submissionType">The format of the submission.</param>
    /// <param name="language">The programming language used.</param>
    /// <exception cref="ArgumentException">Thrown when exerciseId or userId is empty.</exception>
    public Submission(
        Guid exerciseId,
        Guid userId,
        string submittedCode,
        string additionalNotes = "",
        SubmissionType submissionType = SubmissionType.SingleFile,
        ProgrammingLanguage language = ProgrammingLanguage.CSharp)
    {
        if (exerciseId == Guid.Empty)
            throw new ArgumentException("ExerciseId cannot be empty.", nameof(exerciseId));
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        Id = Guid.NewGuid();
        ExerciseId = exerciseId;
        UserId = userId;
        Language = language;
        SubmittedCode = submittedCode ?? string.Empty;
        AdditionalNotes = additionalNotes?.Trim() ?? string.Empty;
        SubmissionType = submissionType;
        SubmittedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to create a valid <see cref="Submission"/> entity.
    /// </summary>
    /// <param name="exerciseId">Target exercise ID.</param>
    /// <param name="userId">Candidate user ID.</param>
    /// <param name="submittedCode">Submitted solution text/code.</param>
    /// <param name="additionalNotes">Optional justification notes.</param>
    /// <param name="submissionType">Submission payload format.</param>
    /// <param name="language">The programming language used.</param>
    /// <returns>A new validated <see cref="Submission"/> instance.</returns>
    public static Submission Create(
        Guid exerciseId,
        Guid userId,
        string submittedCode,
        string additionalNotes = "",
        SubmissionType submissionType = SubmissionType.SingleFile,
        ProgrammingLanguage language = ProgrammingLanguage.CSharp)
    {
        return new Submission(exerciseId, userId, submittedCode, additionalNotes, submissionType, language);
    }

    /// <summary>
    /// Attaches an evaluation to this submission and updates navigation references.
    /// </summary>
    /// <param name="evaluation">The completed or in-flight evaluation entity.</param>
    /// <exception cref="ArgumentNullException">Thrown when evaluation is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when evaluation submission ID does not match this submission ID.</exception>
    public void AttachEvaluation(Evaluation evaluation)
    {
        ArgumentNullException.ThrowIfNull(evaluation);

        if (evaluation.SubmissionId != Id)
            throw new InvalidOperationException($"Evaluation submission ID '{evaluation.SubmissionId}' does not match submission ID '{Id}'.");

        Evaluation = evaluation;
    }
}
