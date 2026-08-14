using backend.DTOs.Admin;

namespace backend.Services.Admin;

public interface IAdminAnalyticsService
{
    Task<AdminAnalyticsDto> GetAnalyticsAsync(int days);
}