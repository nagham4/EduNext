using backend.DTOs.Admin;
using backend.Repositories.Admin;

namespace backend.Services.Admin;

public class AdminSubjectsService : IAdminSubjectsService
{
    private readonly IAdminSubjectsRepository _subjectsRepository;

    public AdminSubjectsService(IAdminSubjectsRepository subjectsRepository)
    {
        _subjectsRepository = subjectsRepository;
    }

    public Task<AdminSubjectsPageDto> GetSubjectsPageAsync(
        string? search,
        string? department,
        string? sortBy,
        int page,
        int pageSize
    )
    {
        return _subjectsRepository.GetSubjectsPageAsync(
            search,
            department,
            sortBy,
            page,
            pageSize
        );
    }

    public Task<List<string>> GetDepartmentsAsync()
    {
        return _subjectsRepository.GetDepartmentsAsync();
    }

    public Task<List<AdminSubjectLessonDto>> GetSubjectLessonsAsync(Guid subjectId)
    {
        return _subjectsRepository.GetSubjectLessonsAsync(subjectId);
    }

    public Task<AdminSubjectDto?> CreateSubjectAsync(CreateAdminSubjectDto dto)
    {
        return _subjectsRepository.CreateSubjectAsync(dto);
    }

    public Task<AdminSubjectDto?> UpdateSubjectAsync(Guid id, UpdateAdminSubjectDto dto)
    {
        return _subjectsRepository.UpdateSubjectAsync(id, dto);
    }

    public Task<bool> DeleteSubjectAsync(Guid id)
    {
        return _subjectsRepository.DeleteSubjectAsync(id);
    }
}