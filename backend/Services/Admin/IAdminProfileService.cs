using backend.DTOs.Admin;

namespace backend.Services.Admin;

public interface IAdminProfileService
{
    Task<AdminProfileDto?> GetProfileAsync(Guid adminId);

    Task<AdminProfileDto?> UpdateProfileAsync(Guid adminId, UpdateAdminProfileDto dto);

    Task<bool> ChangePasswordAsync(Guid adminId, ChangeAdminPasswordDto dto);

    Task<bool> DeleteAccountAsync(Guid adminId);
}