namespace backend.DTOs.Student;

public class AchievementsHubDto
{
    public AchievementStatsDto Stats { get; set; } = new();
    public List<LeaderboardEntryDto> Leaderboard { get; set; } = new();
    public List<AchievementDto> Achievements { get; set; } = new();
}

public class AchievementStatsDto
{
    public int Points { get; set; }
    public int Level { get; set; }
    public double LevelProgressPercent { get; set; }
    public int BestStreakDays { get; set; }
    public int EarnedAchievementsCount { get; set; }
    public int TotalAchievementsCount { get; set; }
}

public class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = "";
    public int Points { get; set; }
}

public class AchievementDto
{
    public Guid AchievementId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }

    public string ConditionType { get; set; } = "";
    public int ConditionValue { get; set; }

    public bool Earned { get; set; }
    public DateTime? EarnedAt { get; set; }

    public int CurrentValue { get; set; }
    public int RemainingToEarn { get; set; }
    public double ProgressPercent { get; set; }
}