using backend.DTOs.Admin;

namespace backend.Repositories.Admin;

public interface IAdminAnalyticsRepository
{
    Task<AdminAnalyticsDto> GetAnalyticsAsync(int days);
}