using backend.DTOs.Student;

namespace backend.Services.Student;

public interface IStudentExamService
{
    Task<List<ExamHistoryDto>> GetExamHistoryAsync(Guid userId);

    Task<List<ExamSubjectOptionDto>> GetExamSubjectsAsync(Guid userId);

    Task<List<ExamLessonOptionDto>> GetSubjectLessonsForQuickExamAsync(Guid userId, Guid subjectId);

    Task<StartedExamDto> StartExamAsync(Guid userId, StartExamRequestDto dto);

    Task<ExamResultDto> SubmitExamAsync(Guid userId, Guid examId, SubmitExamDto dto);

    Task<ExamResultDto?> GetExamResultAsync(Guid userId, Guid resultId);

    Task<bool> DeleteExamResultAsync(Guid userId, Guid resultId);

    Task<int> DeleteExamResultsAsync(Guid userId);
}
