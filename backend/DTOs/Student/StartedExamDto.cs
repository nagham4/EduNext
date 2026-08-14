namespace backend.DTOs.Student;

public class StartedExamDto
{
    public Guid ExamId { get; set; }
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
    public string Type { get; set; } = "";
    public string TypeName { get; set; } = "";
    public Guid? LessonId { get; set; }
    public string? LessonTitle { get; set; }
    public List<StartedExamQuestionDto> Questions { get; set; } = new();
}