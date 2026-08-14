using backend.DTOs.Admin;
using backend.Repositories.Admin;

namespace backend.Services.Admin;

public class AdminAnalyticsService : IAdminAnalyticsService
{
    private readonly IAdminAnalyticsRepository _analyticsRepository;

    public AdminAnalyticsService(IAdminAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository;
    }

    public Task<AdminAnalyticsDto> GetAnalyticsAsync(int days)
    {
        return _analyticsRepository.GetAnalyticsAsync(days);
    }
}