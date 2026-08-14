namespace backend.DTOs.Student;

public class StudentDashboardDto
{
    public DashboardHeaderDto Header { get; set; } = new();
    public List<DashboardStatsDto> Stats { get; set; } = new();
    public List<SubjectProgressDto> SubjectProgress { get; set; } = new();
    public List<WeeklyProgressItemDto> WeeklyProgress { get; set; } = new();
    public List<DashboardRecommendationDto> Recommendations { get; set; } = new();

    public string MotivationalMessage { get; set; } = "";
    public string RecommendationsTitle { get; set; } = "";
    public string RecommendationsEmptyMessage { get; set; } = "";
    public bool HasAnyProgress { get; set; }
    public bool IsAiRecommendations { get; set; }
}

public class DashboardHeaderDto
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
}

public class DashboardStatsDto
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string Color { get; set; } = "";
    public string Icon { get; set; } = "";
}

public class WeeklyProgressItemDto
{
    public string Day { get; set; } = "";
    public int Value { get; set; }
}

public class DashboardRecommendationDto
{
    public Guid? SubjectId { get; set; }
    public Guid? LessonId { get; set; }
    public string Title { get; set; } = "";
    public string Tag { get; set; } = "";
    public string TagColor { get; set; } = "blue";
    public string Description { get; set; } = "";
}