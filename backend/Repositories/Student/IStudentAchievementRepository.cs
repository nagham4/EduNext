using backend.Models.Generated;

namespace backend.Repositories.Student;

public interface IStudentAchievementRepository
{
    Task<int> GetCompletedLessonsCountAsync(Guid userId);

    Task<int> GetCompletedExamsCountAsync(Guid userId);

    Task<int> GetExamPointsAsync(Guid userId);

    Task<int> GetBestExamScoreAsync(Guid userId);

    Task<List<DateTime>> GetLessonCompletedDaysAsync(Guid userId);

    Task<List<DateTime>> GetExamCompletedDaysAsync(Guid userId);

    Task<List<AchievementData>> GetActiveAchievementsAsync();

    Task<List<UserAchievementData>> GetUserAchievementsAsync(Guid userId);

    Task<List<UserLessonPointsData>> GetLessonPointsByUsersAsync();

    Task<List<UserExamPointsData>> GetExamPointsByUsersAsync();

    Task<List<StudentLeaderboardUserData>> GetActiveStudentsAsync();

    void AddUserAchievements(List<user_achievement> achievements);

    Task SaveChangesAsync();
}

public class AchievementData
{
    public Guid Id { get; set; }

    public string? Title { get; set; }

    public string? TitleAr { get; set; }

    public string? Description { get; set; }

    public string? DescriptionAr { get; set; }

    public string? ConditionType { get; set; }

    public int? ConditionValue { get; set; }
}

public class UserAchievementData
{
    public Guid AchievementId { get; set; }

    public DateTime? EarnedAt { get; set; }
}

public class UserLessonPointsData
{
    public Guid UserId { get; set; }

    public int Points { get; set; }
}

public class UserExamPointsData
{
    public Guid UserId { get; set; }

    public int Points { get; set; }
}

public class StudentLeaderboardUserData
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = "";
}