using backend.DTOs.Admin;
using backend.Repositories.Admin;

namespace backend.Services.Admin;

public class AdminProfileService : IAdminProfileService
{
    private readonly IAdminProfileRepository _profileRepository;

    public AdminProfileService(IAdminProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public Task<AdminProfileDto?> GetProfileAsync(Guid adminId)
    {
        return _profileRepository.GetProfileAsync(adminId);
    }

    public Task<AdminProfileDto?> UpdateProfileAsync(Guid adminId, UpdateAdminProfileDto dto)
    {
        return _profileRepository.UpdateProfileAsync(adminId, dto);
    }

    public Task<bool> ChangePasswordAsync(Guid adminId, ChangeAdminPasswordDto dto)
    {
        return _profileRepository.ChangePasswordAsync(adminId, dto);
    }

    public Task<bool> DeleteAccountAsync(Guid adminId)
    {
        return _profileRepository.DeleteAccountAsync(adminId);
    }
}