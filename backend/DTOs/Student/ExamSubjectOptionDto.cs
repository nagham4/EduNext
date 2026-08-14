namespace backend.DTOs.Student;

public class ExamSubjectOptionDto
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
    public string IconKey { get; set; } = "BookOpen";
    public string Color { get; set; } = "blue";
}