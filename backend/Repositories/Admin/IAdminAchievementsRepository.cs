using backend.DTOs.Admin;

namespace backend.Repositories.Admin;

public interface IAdminAchievementsRepository
{
    Task<AdminAchievementsPageDto> GetAchievementsPageAsync(
        string? search,
        string? type,
        string? status,
        int page,
        int pageSize
    );

    Task<AdminAchievementDto?> CreateAchievementAsync(CreateAdminAchievementDto dto);

    Task<AdminAchievementDto?> UpdateAchievementAsync(Guid id, UpdateAdminAchievementDto dto);

    Task<bool> DeleteAchievementAsync(Guid id);
}