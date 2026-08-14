using backend.Models.Generated;

namespace backend.Repositories.Student;

public interface IStudentAnalyticsRepository
{
    Task<string?> GetStudentStreamAsync(Guid userId);

    Task<StudentAnalyticsExamSummaryData> GetExamSummaryAsync(Guid userId);

    Task<StudentAnalyticsLatestExamData?> GetLatestExamAnalysisAsync(Guid userId);

    Task<string?> GetLatestAiRecommendationAsync(Guid userId);

    Task<int> GetTotalLessonsByStreamAsync(string stream);

    Task<int> GetCompletedLessonsByStreamAsync(Guid userId, string stream);

    Task<List<StudentAnalyticsSubjectScoreData>> GetSubjectScoresByStreamAsync(Guid userId, string stream);

    Task<List<StudentAnalyticsExamScoreData>> GetSubjectExamAveragesAsync(Guid userId, string stream);

    Task<List<StudentAnalyticsSubjectDetailData>> GetSubjectDetailsAsync(Guid userId, string stream);

    Task<List<StudentAnalyticsMonthlyProgressData>> GetMonthlyProgressAsync(Guid userId);
}

public class StudentAnalyticsExamScoreData
{
    public string Name { get; set; } = "";
    public DateTime? Date { get; set; }
    public double Average { get; set; }
    public int SubjectsCount { get; set; }
    public string Level { get; set; } = "";
}

public class StudentAnalyticsSubjectDetailData
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
    public double AverageScore { get; set; }
    public int ExamCount { get; set; }
}

public class StudentAnalyticsExamSummaryData
{
    public int TotalExams { get; set; }
    public int PassedExams { get; set; }
    public double AverageScore { get; set; }
}

public class StudentAnalyticsLatestExamData
{
    public string? StrengthPoints { get; set; }
    public string? WeaknessPoints { get; set; }
    public string? LevelMessage { get; set; }
}

public class StudentAnalyticsSubjectScoreData
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
    public double Score { get; set; }
}

public class StudentAnalyticsMonthlyProgressData
{
    public int Year { get; set; }
    public int Month { get; set; }
    public double Average { get; set; }
}
