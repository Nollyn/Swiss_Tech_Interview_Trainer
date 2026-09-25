using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Domain.Entities;

/// <summary>
/// Represents a candidate user profile aggregate, encapsulating user identity, category progresses across all 7 interview dimensions, and historical submissions.
/// </summary>
public class UserProfile
{
    private readonly List<UserCategoryProgress> _progresses = [];
    private readonly List<Submission> _submissions = [];

    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the candidate's username.
    /// </summary>
    public string Username { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the candidate's email address.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the target role description (e.g. Senior Developer / Tech Lead in Zurich).
    /// </summary>
    public string TargetRole { get; private set; } = "Senior Developer / Tech Lead (Zurich)";

    /// <summary>
    /// Gets the UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the UTC timestamp of the user's latest recorded activity.
    /// </summary>
    public DateTime LastActiveAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the read-only collection of category progress records.
    /// </summary>
    public IReadOnlyCollection<UserCategoryProgress> Progresses => _progresses.AsReadOnly();

    /// <summary>
    /// Gets the read-only collection of candidate submissions.
    /// </summary>
    public IReadOnlyCollection<Submission> Submissions => _submissions.AsReadOnly();

    /// <summary>
    /// Parameterless constructor required by EF Core.
    /// </summary>
    private UserProfile() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserProfile"/> entity and seeds progression for all categories.
    /// </summary>
    /// <param name="username">The candidate username.</param>
    /// <param name="email">The candidate email.</param>
    /// <param name="targetRole">The candidate's target job role.</param>
    public UserProfile(string username, string email, string targetRole = "Senior Developer / Tech Lead (Zurich)")
    {
        Id = Guid.NewGuid();
        Username = string.IsNullOrWhiteSpace(username) ? "Swiss Tech Lead Candidate" : username.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? "candidate@swisstech.ch" : email.Trim();
        TargetRole = string.IsNullOrWhiteSpace(targetRole) ? "Senior Developer / Tech Lead (Zurich)" : targetRole.Trim();
        CreatedAt = DateTime.UtcNow;
        LastActiveAt = DateTime.UtcNow;

        // Initialize progress for all 7 categories across all supported languages
        foreach (ProgrammingLanguage lang in Enum.GetValues<ProgrammingLanguage>())
        {
            foreach (CategoryType cat in Enum.GetValues<CategoryType>())
            {
                _progresses.Add(new UserCategoryProgress(Id, cat, lang));
            }
        }
    }

    /// <summary>
    /// Factory method to create a new <see cref="UserProfile"/> aggregate.
    /// </summary>
    /// <param name="username">Candidate username.</param>
    /// <param name="email">Candidate email.</param>
    /// <param name="targetRole">Target role.</param>
    /// <returns>A new <see cref="UserProfile"/> instance.</returns>
    public static UserProfile Create(string username, string email, string targetRole = "Senior Developer / Tech Lead (Zurich)")
    {
        return new UserProfile(username, email, targetRole);
    }

    /// <summary>
    /// Updates the last active timestamp to UTC now.
    /// </summary>
    public void TouchActivity()
    {
        LastActiveAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Retrieves or lazily initializes the progress entity for the specified interview category and programming language.
    /// </summary>
    /// <param name="category">The target interview category.</param>
    /// <param name="language">The target programming language (defaulting to C#).</param>
    /// <returns>The existing or newly created <see cref="UserCategoryProgress"/> record.</returns>
    public UserCategoryProgress GetOrCreateProgress(CategoryType category, ProgrammingLanguage language = ProgrammingLanguage.CSharp)
    {
        var prog = _progresses.FirstOrDefault(p => p.Category == category && p.Language == language);
        if (prog == null)
        {
            prog = new UserCategoryProgress(Id, category, language);
            _progresses.Add(prog);
        }
        return prog;
    }

    /// <summary>
    /// Adds a submission to the user's history and verifies ownership invariants.
    /// </summary>
    /// <param name="submission">The candidate submission.</param>
    /// <exception cref="ArgumentNullException">Thrown when submission is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when submission does not belong to this user.</exception>
    public void AddSubmission(Submission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);

        if (submission.UserId != Id)
            throw new InvalidOperationException($"Submission user ID '{submission.UserId}' does not match user ID '{Id}'.");

        _submissions.Add(submission);
    }
}
