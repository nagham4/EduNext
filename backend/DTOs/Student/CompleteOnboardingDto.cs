namespace backend.DTOs.Student;

public class CompleteOnboardingDto
{
    public string Branch { get; set; } = string.Empty;
    public List<string> Difficult { get; set; } = new();
    public string Hours { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public List<string> Methods { get; set; } = new();
    public string Level { get; set; } = string.Empty;
    public string ExamExp { get; set; } = string.Empty;
}