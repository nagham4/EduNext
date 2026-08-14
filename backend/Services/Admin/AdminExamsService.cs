using backend.DTOs.Admin;
using backend.Repositories.Admin;

namespace backend.Services.Admin;

public class AdminExamsService : IAdminExamsService
{
    private readonly IAdminExamsRepository _examsRepository;

    public AdminExamsService(IAdminExamsRepository examsRepository)
    {
        _examsRepository = examsRepository;
    }

    public Task<AdminExamsPageDto> GetExamsPageAsync(string? search)
    {
        return _examsRepository.GetExamsPageAsync(search);
    }

    public Task<AdminExamDto?> CreateExamAsync(CreateAdminExamDto dto)
    {
        return _examsRepository.CreateExamAsync(dto);
    }

    public Task<AdminExamDto?> UpdateExamAsync(Guid id, UpdateAdminExamDto dto)
    {
        return _examsRepository.UpdateExamAsync(id, dto);
    }

    public Task<bool> DeleteExamAsync(Guid id)
    {
        return _examsRepository.DeleteExamAsync(id);
    }
}