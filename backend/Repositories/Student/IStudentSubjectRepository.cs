using backend.Models.Generated;

namespace backend.Repositories.Student;

public interface IStudentSubjectRepository
{
    Task<string?> GetStudentStreamAsync(Guid userId);

    Task<List<SubjectListData>> GetSubjectsByStreamAsync(string stream);

    Task<List<CompletedLessonData>> GetCompletedLessonsForSubjectsAsync(Guid userId, List<Guid> subjectIds);

    Task<subject?> GetSubjectByIdAndStreamAsync(Guid subjectId, string stream);

    Task<List<subject_unit>> GetUnitsBySubjectIdAsync(Guid subjectId);

    Task<List<lesson>> GetLessonsBySubjectIdAsync(Guid subjectId);

    Task<bool> LessonBelongsToSubjectAsync(Guid subjectId, Guid lessonId);

    Task<lesson?> GetLessonByIdAsync(Guid lessonId);

    Task<bool> IsLessonCompletedAsync(Guid userId, Guid lessonId);

    Task<lesson_progress?> GetLessonProgressAsync(Guid userId, Guid lessonId);

    void AddLessonProgress(lesson_progress progress);

    Task<int> CountCompletedLessonsAsync(Guid userId);

    Task<List<achievement>> GetAchievementsByConditionTypeAsync(string conditionType);

    Task<HashSet<Guid>> GetEarnedAchievementIdsAsync(Guid userId);

    void AddUserAchievement(user_achievement userAchievement);

    Task<bool> HasStudySessionForLessonAsync(Guid userId, Guid lessonId);

    void AddStudySession(study_session studySession);

    Task<List<subject>> GetSubjectsByBranchAsync(string branch);

    Task SaveChangesAsync();
}

public class SubjectListData
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
    public string Description { get; set; } = "";
    public int LessonsCount { get; set; }
}

public class CompletedLessonData
{
    public Guid SubjectId { get; set; }
    public Guid LessonId { get; set; }
}