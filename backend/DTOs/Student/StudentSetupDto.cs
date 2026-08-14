namespace backend.DTOs.Student;

public class StudentSetupDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;

    public string? Stream { get; set; }
    public string? CurrentGrade { get; set; }
    public int? ExamYear { get; set; }

    public string? PreferredStudyTime { get; set; }
    public string? PreferredStudyPlace { get; set; }
    public string? PrimaryGoal { get; set; }

    public bool IsOnboardingCompleted { get; set; }

    public List<Guid> SelectedSubjectIds { get; set; } = new();
    public List<string> DifficultSubjectNames { get; set; } = new();

    public List<string> LearningMethods { get; set; } = new();
    public string? ExamExperience { get; set; }
}