namespace backend.DTOs.Student;

public class SubjectProgressDto
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
    public int CompletedLessons { get; set; }
    public int RemainingLessons { get; set; }
    public double ProgressPercent { get; set; }
}