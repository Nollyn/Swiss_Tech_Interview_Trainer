namespace SwissTechTrainer.Domain.Enums;

/// <summary>
/// Represents the processing state of an automated LLM evaluation.
/// </summary>
public enum EvaluationStatus
{
    /// <summary>
    /// The evaluation has been queued and is waiting for processing.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// The evaluation is currently in-flight with the LLM provider.
    /// </summary>
    Analyzing = 1,

    /// <summary>
    /// The evaluation completed successfully and deterministic scoring has been computed.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// The evaluation failed due to a provider timeout, rate limit exhaustion, or parsing failure.
    /// </summary>
    Failed = 3
}

/// <summary>
/// Specifies the content payload format submitted by the candidate.
/// </summary>
public enum SubmissionType
{
    /// <summary>
    /// A single C# source file (.cs).
    /// </summary>
    SingleFile = 1,

    /// <summary>
    /// A multi-file project archive (.zip) for system design or modular architecture challenges.
    /// </summary>
    ZipArchive = 2,

    /// <summary>
    /// Pure text/markdown submission for behavioral (STAR) or system design justifications.
    /// </summary>
    TextDescription = 3
}
