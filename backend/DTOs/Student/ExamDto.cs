namespace backend.DTOs.Student;

public class ExamDto
{
    public Guid ExamId { get; set; }
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = "";

    public Guid? LessonId { get; set; }
    public string? LessonTitle { get; set; }

    public string Type { get; set; } = ""; // short/comprehensive
    public int QuestionsCount { get; set; }

    public bool HasAttempt { get; set; }
    public int? LastScore { get; set; }
}