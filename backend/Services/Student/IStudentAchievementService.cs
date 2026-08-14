using backend.DTOs.Student;

namespace backend.Services.Student;

public interface IStudentAchievementService
{
    Task<AchievementsHubDto> GetHubAsync(Guid userId, int leaderboardSize = 10);
}