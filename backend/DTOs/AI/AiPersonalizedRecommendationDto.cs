namespace backend.DTOs.AI;

public class AiPersonalizedRecommendationRequestDto
{
    public string ContextType { get; set; } = "";
    public string StudentName { get; set; } = "";
    public string Stream { get; set; } = "";
    public string CurrentLevel { get; set; } = "";
    public string Goal { get; set; } = "";
    public string StudyHours { get; set; } = "";
    public string ExamExperience { get; set; } = "";
    public List<string> LearningMethods { get; set; } = new();
    public List<string> DifficultSubjects { get; set; } = new();
    public double AverageScore { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public List<AiSubjectProgressDto> Subjects { get; set; } = new();
}

public class AiSubjectProgressDto
{
    public Guid? SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
    public double ProgressPercent { get; set; }
    public double AverageScore { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public int RemainingLessons { get; set; }
    public string? NextLessonTitle { get; set; }
    public Guid? NextLessonId { get; set; }
}

public class AiPersonalizedRecommendationResponseDto
{
    public string RecommendationText { get; set; } = "";
    public List<string> FocusSubjects { get; set; } = new();
    public int WeeklyStudyHours { get; set; }
    public List<string> LessonOrder { get; set; } = new();
    public List<string> StrengthAreas { get; set; } = new();
    public List<string> WeakAreas { get; set; } = new();
    public List<AiSubjectAnalysisDto> SubjectAnalyses { get; set; } = new();
}

public class AiSubjectAnalysisDto
{
    public Guid? SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
}
