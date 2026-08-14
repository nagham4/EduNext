using backend.Models.Generated;

namespace backend.Repositories.Student;

public interface IStudentStudyPlanRepository
{
    Task<string?> GetStudentStreamAsync(Guid userId);

    Task<List<study_plan>> GetPlansByUserAsync(Guid userId);

    Task<study_plan?> GetPlanByUserAsync(Guid userId, Guid planId);

    Task<study_plan?> GetPlanForUpdateAsync(Guid userId, Guid planId);

    Task<subject?> GetSubjectByIdAndStreamAsync(Guid subjectId, string stream);

    Task<List<StudyPlanSubjectOptionData>> GetSubjectsByStreamAsync(string stream);

    Task<StudentStudyPlanProfileData?> GetProfileAsync(Guid userId);

    Task<List<StudentStudyPlanSubjectProgressData>> GetSubjectProgressAsync(Guid userId, string stream);

    Task<List<StudyPlanLessonOptionData>> GetLessonsBySubjectAsync(Guid subjectId);

    Task<List<lesson>> GetValidLessonsAsync(Guid subjectId, List<Guid> lessonIds);

    void AddPlan(study_plan plan);

    void AddPlanItem(study_plan_item item);

    void RemovePlanItems(IEnumerable<study_plan_item> items);

    void RemovePlan(study_plan plan);

    Task<study_plan?> GetPlanWithDetailsAsync(Guid planId);

    Task<study_plan_item?> GetPlanItemForUpdateAsync(Guid userId, Guid planId, Guid itemId);

    Task SaveChangesAsync();
}

public class StudyPlanSubjectOptionData
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
}

public class StudyPlanLessonOptionData
{
    public Guid LessonId { get; set; }
    public string LessonTitle { get; set; } = "";
    public int OrderNumber { get; set; }
    public int DisplayOrder { get; set; }
    public string UnitTitle { get; set; } = "";
}

public class StudentStudyPlanProfileData
{
    public string Stream { get; set; } = "";
    public string CurrentLevel { get; set; } = "";
    public string Goal { get; set; } = "";
    public string StudyHours { get; set; } = "";
    public string ExamExperience { get; set; } = "";
    public List<string> LearningMethods { get; set; } = new();
    public List<string> DifficultSubjects { get; set; } = new();
}

public class StudentStudyPlanSubjectProgressData
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public double AverageScore { get; set; }
    public Guid? NextLessonId { get; set; }
    public string? NextLessonTitle { get; set; }
}

public class StudentStudyPlanLessonProgressData
{
    public Guid LessonId { get; set; }
    public Guid SubjectId { get; set; }
    public string LessonTitle { get; set; } = "";
}
