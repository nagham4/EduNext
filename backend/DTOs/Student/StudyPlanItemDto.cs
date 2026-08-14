namespace backend.DTOs.Student;

public class StudyPlanItemDto
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public int OrderNumber { get; set; }
    public bool IsCompleted { get; set; }
}