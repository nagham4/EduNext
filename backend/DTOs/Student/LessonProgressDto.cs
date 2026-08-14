namespace backend.DTOs.Student;

public class LessonProgressDto
{
    public int CompletedLessons { get; set; }
    public int RemainingLessons { get; set; }
    public double ProgressPercent { get; set; }
}