using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Domain.Entities;

public class UserProfile
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string TargetRole { get; private set; } = "Senior .NET Developer / Tech Lead (Zurich)";
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime LastActiveAt { get; private set; } = DateTime.UtcNow;

    private readonly List<UserCategoryProgress> _progresses = new();
    public IReadOnlyCollection<UserCategoryProgress> Progresses => _progresses.AsReadOnly();

    private readonly List<Submission> _submissions = new();
    public IReadOnlyCollection<Submission> Submissions => _submissions.AsReadOnly();

    private UserProfile() { }

    public UserProfile(string username, string email, string targetRole = "Senior .NET Developer / Tech Lead (Zurich)")
    {
        Id = Guid.NewGuid();
        Username = string.IsNullOrWhiteSpace(username) ? "SwissCandidate" : username;
        Email = string.IsNullOrWhiteSpace(email) ? "candidate@swisstech.ch" : email;
        TargetRole = targetRole;
        CreatedAt = DateTime.UtcNow;
        LastActiveAt = DateTime.UtcNow;

        // Initialize progress for all 7 categories
        foreach (CategoryType cat in Enum.GetValues<CategoryType>())
        {
            _progresses.Add(new UserCategoryProgress(Id, cat));
        }
    }

    public void TouchActivity()
    {
        LastActiveAt = DateTime.UtcNow;
    }

    public UserCategoryProgress GetOrCreateProgress(CategoryType category)
    {
        var prog = _progresses.FirstOrDefault(p => p.Category == category);
        if (prog == null)
        {
            prog = new UserCategoryProgress(Id, category);
            _progresses.Add(prog);
        }
        return prog;
    }
}
