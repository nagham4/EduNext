namespace backend.DTOs.Student;

public class LessonListItemDto
{
    public Guid Id { get; set; }
    public Guid SubjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int OrderNumber { get; set; }
}