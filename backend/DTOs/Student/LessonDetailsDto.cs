namespace backend.DTOs.Student;

public class LessonDetailsDto
{
    public Guid LessonId { get; set; }
    public Guid SubjectId { get; set; }
    public string SubjectTitle { get; set; } = string.Empty;
    public int LessonNumber { get; set; }
    public int TotalLessons { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public string? VideoUrl { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public List<string> Summary { get; set; } = new();
    public string? PdfUrl { get; set; }
    public string? ResourcesUrl { get; set; }
}