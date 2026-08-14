using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Student;

public class UpdateStudyPlanDto
{
    public Guid? SubjectId { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsAiGenerated { get; set; }

    public List<string> StudyDays { get; set; } = new();

    [Range(1, 1440)]
    public int? DailyDurationMinutes { get; set; }

    public List<Guid> LessonIds { get; set; } = new();
}