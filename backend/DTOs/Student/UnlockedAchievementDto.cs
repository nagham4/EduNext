namespace backend.DTOs.Student;

public class UnlockedAchievementDto
{
    public Guid AchievementId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Type { get; set; } = "lessons";

    public string Reward { get; set; } = "+50 نقطة";
}