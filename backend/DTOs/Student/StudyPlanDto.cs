namespace backend.DTOs.Student;

public class StudyPlanDto
{
    public Guid Id { get; set; }
    public Guid? SubjectId { get; set; }
    public string SubjectName { get; set; } = "";

    public string? Title { get; set; }
    public string? Description { get; set; }

    public bool IsAiGenerated { get; set; }

    public List<string> StudyDays { get; set; } = new();
    public int? DailyDurationMinutes { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int TotalItems { get; set; }
    public int CompletedItems { get; set; }
    public double ProgressPercent { get; set; }

    public List<StudyPlanItemDto> Items { get; set; } = new();
}