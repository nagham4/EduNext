namespace backend.DTOs.Student;

public class StudyPlanSuggestionDto
{
    public string RecommendationText { get; set; } = "";
    public List<string> FocusSubjects { get; set; } = new();
    public int WeeklyStudyHours { get; set; }
    public List<string> LessonOrder { get; set; } = new();
    public Guid? SubjectId { get; set; }
    public List<Guid> LessonIds { get; set; } = new();
    public List<StudyPlanSuggestedSubjectDto> SuggestedSubjects { get; set; } = new();
}

public class StudyPlanSuggestedSubjectDto
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
    public List<Guid> LessonIds { get; set; } = new();
    public List<string> LessonOrder { get; set; } = new();
}
