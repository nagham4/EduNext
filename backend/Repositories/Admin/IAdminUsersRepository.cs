using backend.DTOs.Admin;

namespace backend.Repositories.Admin;

public interface IAdminUsersRepository
{
    Task<AdminUsersPageDto> GetUsersPageAsync(
        string? search,
        string? role,
        string? status,
        int page,
        int pageSize
    );

    Task<AdminUserDto?> GetUserByIdAsync(Guid id);

    Task<List<AdminRoleOptionDto>> GetRolesAsync();

    Task<AdminUserDto?> UpdateRoleAsync(Guid id, string role);

    Task<AdminUserDto?> UpdateStatusAsync(Guid id, bool isActive);

    Task<bool> DeleteUserAsync(Guid id);
}