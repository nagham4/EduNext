using backend.DTOs.Admin;

namespace backend.Services.Admin;

public interface IAdminLessonsService
{
    Task<AdminLessonsPageDto> GetLessonsPageAsync(
        string? search,
        string? department,
        Guid? subjectId,
        string? sortBy,
        int page,
        int pageSize
    );

    Task<AdminLessonDto?> CreateLessonAsync(CreateAdminLessonDto dto);

    Task<AdminLessonDto?> UpdateLessonAsync(Guid id, UpdateAdminLessonDto dto);

    Task<bool> DeleteLessonAsync(Guid id);
}