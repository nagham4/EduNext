namespace backend.DTOs.Student;

public class SubjectUnitDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int OrderNumber { get; set; }
    public List<SubjectLessonDto> Lessons { get; set; } = new();
}