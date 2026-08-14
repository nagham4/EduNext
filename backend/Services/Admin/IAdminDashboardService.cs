using backend.DTOs.Admin;

namespace backend.Services.Admin;

public interface IAdminDashboardService
{
    Task<AdminDashboardDto> GetDashboardAsync(Guid adminId);
}