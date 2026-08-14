namespace backend.DTOs.Student;

public class OnboardingOptionsDto
{
    public List<OnboardingBranchDto> Branches { get; set; } = new();

    public List<string> StudyHours { get; set; } = new();

    public List<string> Goals { get; set; } = new();

    public List<string> Levels { get; set; } = new();

    public List<string> ExamExperiences { get; set; } = new();
}

public class OnboardingBranchDto
{
    public string Name { get; set; } = string.Empty;

    public List<OnboardingSubjectDto> Subjects { get; set; } = new();
}