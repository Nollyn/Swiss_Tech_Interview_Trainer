namespace SwissTechTrainer.Domain.Enums;

public enum EvaluationStatus
{
    Pending = 0,
    Analyzing = 1,
    Completed = 2,
    Failed = 3
}

public enum SubmissionType
{
    SingleFile = 1,
    ZipArchive = 2,
    TextDescription = 3
}
