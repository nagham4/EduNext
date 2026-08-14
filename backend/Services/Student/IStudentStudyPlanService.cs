using backend.DTOs.Student;

namespace backend.Services.Student;

public interface IStudentStudyPlanService
{
    Task<List<StudyPlanSubjectOptionDto>> GetSubjectsAsync(Guid userId);

    Task<List<StudyPlanLessonOptionDto>> GetLessonsAsync(Guid userId, Guid subjectId);

    Task<List<StudyPlanDto>> GetMyPlansAsync(Guid userId);

    Task<StudyPlanSuggestionDto> GetAiSuggestionAsync(Guid userId);

    Task<StudyPlanDto?> GetByIdAsync(Guid userId, Guid id);

    Task<StudyPlanDto> CreateAsync(Guid userId, CreateStudyPlanDto dto);

    Task<StudyPlanDto?> UpdateAsync(Guid userId, Guid id, UpdateStudyPlanDto dto);

    Task<bool> DeleteAsync(Guid userId, Guid id);

    Task<bool> UpdateItemCompletionAsync(Guid userId, Guid planId, Guid itemId, bool isCompleted);
}
