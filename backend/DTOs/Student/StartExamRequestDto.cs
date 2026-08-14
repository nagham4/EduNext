namespace backend.DTOs.Student;

public class StartExamRequestDto
{
    public Guid SubjectId { get; set; }
    public string Type { get; set; } = ""; // quick / comprehensive
    public Guid? LessonId { get; set; }
}