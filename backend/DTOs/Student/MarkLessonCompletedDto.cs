namespace backend.DTOs.Student;

public class MarkLessonCompletedDto
{
    public bool Completed { get; set; } = true;
    public int? DurationSeconds { get; set; }
}
