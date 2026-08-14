using backend.DTOs.Admin;

namespace backend.Repositories.Admin;

public interface IAdminProfileRepository
{
    Task<AdminProfileDto?> GetProfileAsync(Guid adminId);

    Task<AdminProfileDto?> UpdateProfileAsync(Guid adminId, UpdateAdminProfileDto dto);

    Task<bool> ChangePasswordAsync(Guid adminId, ChangeAdminPasswordDto dto);

    Task<bool> DeleteAccountAsync(Guid adminId);
}