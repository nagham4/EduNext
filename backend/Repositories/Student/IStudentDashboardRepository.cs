using backend.DTOs.Student;

namespace backend.Repositories.Student;

public interface IStudentDashboardRepository
{
    Task<DashboardIdentityData?> GetIdentityAsync(Guid userId);

    Task<List<DashboardWeeklySessionData>> GetWeeklySessionsAsync(
        Guid userId,
        DateTime startOfWeek,
        DateTime endOfWeek
    );

    Task<int> GetTotalStudyMinutesAsync(Guid userId);

    Task<List<DashboardSubjectData>> GetSubjectsByStreamAsync(string stream);

    Task<List<DashboardSubjectLessonTotalData>> GetSubjectLessonTotalsAsync(List<Guid> subjectIds);

    Task<List<DashboardSubjectCompletedData>> GetSubjectCompletedLessonsAsync(
        Guid userId,
        List<Guid> subjectIds
    );

    Task<DashboardExamSummaryData> GetExamSummaryAsync(Guid userId, List<Guid> subjectIds);

    Task<List<DashboardSubjectExamAverageData>> GetSubjectExamAveragesAsync(Guid userId, List<Guid> subjectIds);

    Task<string?> GetLatestAiRecommendationAsync(Guid userId);

    Task<List<DashboardLessonCandidateData>> GetNextLessonsForSubjectsAsync(
        Guid userId,
        List<Guid> subjectIds
    );
}

public class DashboardIdentityData
{
    public string FullName { get; set; } = "";
    public string? Stream { get; set; }
    public string CurrentLevel { get; set; } = "";
    public string Goal { get; set; } = "";
    public string StudyHours { get; set; } = "";
    public string ExamExperience { get; set; } = "";
    public List<string> LearningMethods { get; set; } = new();
    public List<string> DifficultSubjects { get; set; } = new();
}

public class DashboardWeeklySessionData
{
    public DateTime Date { get; set; }
    public int Minutes { get; set; }
}

public class DashboardSubjectData
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
}

public class DashboardSubjectLessonTotalData
{
    public Guid SubjectId { get; set; }
    public int Total { get; set; }
}

public class DashboardSubjectCompletedData
{
    public Guid SubjectId { get; set; }
    public int Completed { get; set; }
}

public class DashboardExamSummaryData
{
    public int Count { get; set; }
    public double AverageScore { get; set; }
}

public class DashboardSubjectExamAverageData
{
    public Guid SubjectId { get; set; }
    public double AverageScore { get; set; }
}

public class DashboardLessonCandidateData
{
    public Guid SubjectId { get; set; }
    public Guid LessonId { get; set; }
    public string LessonTitle { get; set; } = "";
}
