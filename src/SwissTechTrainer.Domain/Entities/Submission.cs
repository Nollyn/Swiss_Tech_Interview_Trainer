using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Domain.Entities;

public class Submission
{
    public Guid Id { get; private set; }
    public Guid ExerciseId { get; private set; }
    public Guid UserId { get; private set; }
    public string SubmittedCode { get; private set; } = string.Empty;
    public string AdditionalNotes { get; private set; } = string.Empty;
    public SubmissionType SubmissionType { get; private set; } = SubmissionType.SingleFile;
    public DateTime SubmittedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public Exercise? Exercise { get; private set; }
    public UserProfile? User { get; private set; }
    public Evaluation? Evaluation { get; private set; }

    private Submission() { }

    public Submission(
        Guid exerciseId,
        Guid userId,
        string submittedCode,
        string additionalNotes = "",
        SubmissionType submissionType = SubmissionType.SingleFile)
    {
        if (exerciseId == Guid.Empty)
            throw new ArgumentException("ExerciseId cannot be empty.", nameof(exerciseId));
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        Id = Guid.NewGuid();
        ExerciseId = exerciseId;
        UserId = userId;
        SubmittedCode = submittedCode ?? string.Empty;
        AdditionalNotes = additionalNotes ?? string.Empty;
        SubmissionType = submissionType;
        SubmittedAt = DateTime.UtcNow;
    }

    public void AttachEvaluation(Evaluation evaluation)
    {
        Evaluation = evaluation ?? throw new ArgumentNullException(nameof(evaluation));
    }
}
