using backend.DTOs.Admin;
using backend.Repositories.Admin;

namespace backend.Services.Admin;

public class AdminLessonsService : IAdminLessonsService
{
    private readonly IAdminLessonsRepository _lessonsRepository;

    public AdminLessonsService(IAdminLessonsRepository lessonsRepository)
    {
        _lessonsRepository = lessonsRepository;
    }

    public Task<AdminLessonsPageDto> GetLessonsPageAsync(
        string? search,
        string? department,
        Guid? subjectId,
        string? sortBy,
        int page,
        int pageSize
    )
    {
        return _lessonsRepository.GetLessonsPageAsync(
            search,
            department,
            subjectId,
            sortBy,
            page,
            pageSize
        );
    }

    public Task<AdminLessonDto?> CreateLessonAsync(CreateAdminLessonDto dto)
    {
        return _lessonsRepository.CreateLessonAsync(dto);
    }

    public Task<AdminLessonDto?> UpdateLessonAsync(Guid id, UpdateAdminLessonDto dto)
    {
        return _lessonsRepository.UpdateLessonAsync(id, dto);
    }

    public Task<bool> DeleteLessonAsync(Guid id)
    {
        return _lessonsRepository.DeleteLessonAsync(id);
    }
}