using backend.DTOs.Student;

namespace backend.Services.Student;

public interface IStudentSubjectService
{
    Task<List<SubjectDto>> GetAllAsync(Guid userId);

    Task<SubjectDetailsDto?> GetByIdAsync(Guid userId, Guid id);

    Task<LessonDetailsDto?> GetLessonDetailsAsync(Guid userId, Guid subjectId, Guid lessonId);

    Task<LessonCompletionResponseDto?> SetLessonCompletedAsync(
        Guid userId,
        Guid subjectId,
        Guid lessonId,
        bool completed,
        int? durationSeconds = null
    );

    Task<List<OnboardingSubjectDto>> GetSubjectsByBranchAsync(string branch);
}
