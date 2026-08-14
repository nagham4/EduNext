namespace backend.DTOs.Student;

public class StudentAnalyticsDto
{
    public AnalyticsOverviewDto Overview { get; set; } = new();
    public List<AnalyticsSubjectScoreDto> SubjectScores { get; set; } = new();
    public List<StudentAnalyticsExamScoreDto> ExamScores { get; set; } = new();
    public List<AnalyticsSubjectDetailDto> SubjectDetails { get; set; } = new();
    public List<string> StrengthAreas { get; set; } = new();
    public List<string> WeakAreas { get; set; } = new();
    public string? RecommendationText { get; set; }
    public List<MonthlyProgressPointDto> MonthlyProgress { get; set; } = new();
}

public class AnalyticsOverviewDto
{
    public string OverallLevel { get; set; } = "";
    public double AverageScore { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public int PassedExams { get; set; }
    public int TotalExams { get; set; }
}

public class AnalyticsSubjectScoreDto
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
    public double Score { get; set; }
}

public class StudentAnalyticsExamScoreDto
{
    public string Name { get; set; } = "";
    public string Date { get; set; } = "";
    public double Average { get; set; }
    public int SubjectsCount { get; set; }
    public string Level { get; set; } = "";
}

public class AnalyticsSubjectDetailDto
{
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = "";
    public double AverageScore { get; set; }
    public int ExamCount { get; set; }
    public string Level { get; set; } = "";
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
}

public class MonthlyProgressPointDto
{
    public string Month { get; set; } = "";
    public double Value { get; set; }
}
