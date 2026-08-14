namespace backend.DTOs.Student;

public class StudyPlanLessonOptionDto
{
    public Guid LessonId { get; set; }
    public string LessonTitle { get; set; } = "";
    public int OrderNumber { get; set; }
    public int DisplayOrder { get; set; }
    public string UnitTitle { get; set; } = "";
}