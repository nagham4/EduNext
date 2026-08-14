namespace backend.DTOs.Student;

public class LessonCompletionResponseDto
{
    public string Message { get; set; } = string.Empty;

    public LessonDetailsDto Lesson { get; set; } = new();

    public List<UnlockedAchievementDto> NewAchievements { get; set; } = new();
}