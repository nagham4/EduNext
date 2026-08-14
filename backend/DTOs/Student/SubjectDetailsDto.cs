namespace backend.DTOs.Student;

public class SubjectDetailsDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
    public int Progress { get; set; }
    public int Lessons { get; set; }
    public int Completed { get; set; }
    public string Color { get; set; } = "blue";
    public string IconKey { get; set; } = "BookOpen";

    public List<SubjectUnitDto> Units { get; set; } = new();
}