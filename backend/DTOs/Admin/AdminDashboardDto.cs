namespace backend.DTOs.Admin;

public class AdminDashboardDto
{
    public AdminDashboardHeaderDto Header { get; set; } = new();

    public AdminDashboardSummaryDto Summary { get; set; } = new();

    public AdminLessonCompletionDto LessonCompletion { get; set; } = new();

    public List<AdminMostActiveSubjectDto> MostActiveSubjects { get; set; } = new();

    public List<AdminRecentActivityDto> RecentActivities { get; set; } = new();

    public List<AdminStudentPerformanceTrendDto> StudentPerformanceTrends { get; set; } = new();

    public bool HasData { get; set; }
}

public class AdminDashboardHeaderDto
{
    public string AdminName { get; set; } = "";
    public string LastLoginMessage { get; set; } = "";
}

public class AdminDashboardSummaryDto
{
    public int TotalStudents { get; set; }
    public int TotalSubjects { get; set; }
    public int TotalLessons { get; set; }
    public int TotalExams { get; set; }
    public int CompletedExams { get; set; }

    public int NewStudentsThisMonth { get; set; }
    public int NewSubjectsThisMonth { get; set; }
    public int NewLessonsThisMonth { get; set; }
    public int CompletedExamsThisMonth { get; set; }
}

public class AdminLessonCompletionDto
{
    public int Percentage { get; set; }
    public int CompletedCount { get; set; }
    public int InProgressCount { get; set; }
}

public class AdminMostActiveSubjectDto
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
    public int ActivityValue { get; set; }
    public int Percentage { get; set; }
}

public class AdminRecentActivityDto
{
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class AdminStudentPerformanceTrendDto
{
    public string Day { get; set; } = "";
    public int Value { get; set; }
}