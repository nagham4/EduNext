using backend.DTOs.Admin;

namespace backend.Services.Admin;

public interface IAdminExamsService
{
    Task<AdminExamsPageDto> GetExamsPageAsync(string? search);

    Task<AdminExamDto?> CreateExamAsync(CreateAdminExamDto dto);

    Task<AdminExamDto?> UpdateExamAsync(Guid id, UpdateAdminExamDto dto);

    Task<bool> DeleteExamAsync(Guid id);
}