using backend.DTOs.Admin;

namespace backend.Services.Admin;

public interface IAdminSubjectsService
{
    Task<AdminSubjectsPageDto> GetSubjectsPageAsync(
        string? search,
        string? department,
        string? sortBy,
        int page,
        int pageSize
    );

    Task<List<string>> GetDepartmentsAsync();

    Task<List<AdminSubjectLessonDto>> GetSubjectLessonsAsync(Guid subjectId);

    Task<AdminSubjectDto?> CreateSubjectAsync(CreateAdminSubjectDto dto);

    Task<AdminSubjectDto?> UpdateSubjectAsync(Guid id, UpdateAdminSubjectDto dto);

    Task<bool> DeleteSubjectAsync(Guid id);
}