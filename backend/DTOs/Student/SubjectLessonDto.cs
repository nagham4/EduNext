namespace backend.DTOs.Student;

public class SubjectLessonDto
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public bool Completed { get; set; }
}