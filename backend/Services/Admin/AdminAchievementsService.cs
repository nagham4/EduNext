using backend.DTOs.Admin;
using backend.Repositories.Admin;

namespace backend.Services.Admin;

public class AdminAchievementsService : IAdminAchievementsService
{
    private readonly IAdminAchievementsRepository _achievementsRepository;

    public AdminAchievementsService(IAdminAchievementsRepository achievementsRepository)
    {
        _achievementsRepository = achievementsRepository;
    }

    public Task<AdminAchievementsPageDto> GetAchievementsPageAsync(
        string? search,
        string? type,
        string? status,
        int page,
        int pageSize
    )
    {
        return _achievementsRepository.GetAchievementsPageAsync(
            search,
            type,
            status,
            page,
            pageSize
        );
    }

    public Task<AdminAchievementDto?> CreateAchievementAsync(CreateAdminAchievementDto dto)
    {
        return _achievementsRepository.CreateAchievementAsync(dto);
    }

    public Task<AdminAchievementDto?> UpdateAchievementAsync(Guid id, UpdateAdminAchievementDto dto)
    {
        return _achievementsRepository.UpdateAchievementAsync(id, dto);
    }

    public Task<bool> DeleteAchievementAsync(Guid id)
    {
        return _achievementsRepository.DeleteAchievementAsync(id);
    }
}