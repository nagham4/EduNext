using backend.DTOs.Admin;
using backend.Repositories.Admin;

namespace backend.Services.Admin;

public class AdminUsersService : IAdminUsersService
{
    private readonly IAdminUsersRepository _usersRepository;

    public AdminUsersService(IAdminUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public Task<AdminUsersPageDto> GetUsersPageAsync(
        string? search,
        string? role,
        string? status,
        int page,
        int pageSize
    )
    {
        return _usersRepository.GetUsersPageAsync(
            search,
            role,
            status,
            page,
            pageSize
        );
    }

    public Task<AdminUserDto?> GetUserByIdAsync(Guid id)
    {
        return _usersRepository.GetUserByIdAsync(id);
    }

    public Task<List<AdminRoleOptionDto>> GetRolesAsync()
    {
        return _usersRepository.GetRolesAsync();
    }

    public Task<AdminUserDto?> UpdateRoleAsync(Guid id, string role)
    {
        return _usersRepository.UpdateRoleAsync(id, role);
    }

    public Task<AdminUserDto?> UpdateStatusAsync(Guid id, bool isActive)
    {
        return _usersRepository.UpdateStatusAsync(id, isActive);
    }

    public Task<bool> DeleteUserAsync(Guid id)
    {
        return _usersRepository.DeleteUserAsync(id);
    }
}