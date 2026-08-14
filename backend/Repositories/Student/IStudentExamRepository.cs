using backend.Models.Generated;

namespace backend.Repositories.Student;

public interface IStudentExamRepository
{
    Task<string?> GetStudentStreamAsync(Guid userId);

    Task<List<StudentExamHistoryData>> GetExamHistoryAsync(Guid userId);

    Task<List<StudentExamSubjectOptionData>> GetExamSubjectsAsync(string stream);

    Task<List<StudentExamLessonOptionData>> GetQuickExamLessonsAsync(Guid subjectId);

    Task<subject?> GetSubjectByIdAndStreamAsync(Guid subjectId, string stream);

    Task<exam?> GetExamForStartAsync(Guid subjectId, string normalizedType, Guid? lessonId);

    Task<exam?> GetExamByIdAsync(Guid examId);

    Task<List<question>> GetQuestionsByExamIdAsync(Guid examId);

    Task<exam_result?> GetExamResultByUserAsync(Guid userId, Guid resultId);

    Task<List<exam_result_answer>> GetResultAnswersAsync(Guid resultId);

    Task<string?> GetLatestAiRecommendationAsync(Guid userId);

    Task<bool> DeleteExamResultAsync(Guid userId, Guid resultId);

    Task<int> DeleteExamResultsAsync(Guid userId);

    void AddExamResult(exam_result result);

    void AddExamResultAnswer(exam_result_answer answer);

    void AddAiRecommendation(ai_recommendation recommendation);

    Task SaveChangesAsync();
}

public class StudentExamHistoryData
{
    public Guid ResultId { get; set; }
    public Guid? ExamId { get; set; }
    public int Score { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string ExamType { get; set; } = "";
    public Guid? SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
    public Guid? LessonId { get; set; }
    public string? LessonTitle { get; set; }
    public int QuestionsCount { get; set; }
}

public class StudentExamSubjectOptionData
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = "";
}

public class StudentExamLessonOptionData
{
    public Guid LessonId { get; set; }
    public string LessonTitle { get; set; } = "";
    public int OrderNumber { get; set; }
    public int DisplayOrder { get; set; }
    public string UnitTitle { get; set; } = "";
}
