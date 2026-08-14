namespace backend.DTOs.Student;

public class ExamHistoryDto
{
    public Guid ExamResultId { get; set; }
    public Guid ExamId { get; set; }

    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
    public string SubjectKey { get; set; } = ""; // math / physics / arabic ...

    public Guid? LessonId { get; set; }
    public string? LessonTitle { get; set; }

    public string Type { get; set; } = "";      // quick / comprehensive
    public string TypeName { get; set; } = "";  // قصير / شامل

    public int Score { get; set; }
    public int QuestionsCount { get; set; }
    public int Percentage { get; set; }

    public string Date { get; set; } = "";
}