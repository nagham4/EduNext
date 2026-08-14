using backend.DTOs.Student;
using backend.Models.Generated;
using backend.Repositories.Student;

namespace backend.Services.Student;

public class StudentAchievementService : IStudentAchievementService
{
    private readonly IStudentAchievementRepository _repository;

    private const int PointsPerLessonCompleted = 50;
    private const int LevelStep = 500;

    public StudentAchievementService(IStudentAchievementRepository repository)
    {
        _repository = repository;
    }

    public async Task<AchievementsHubDto> GetHubAsync(Guid userId, int leaderboardSize = 10)
    {
        var lessonsCompleted = await _repository.GetCompletedLessonsCountAsync(userId);
        var examsCompleted = await _repository.GetCompletedExamsCountAsync(userId);
        var examPoints = await _repository.GetExamPointsAsync(userId);
        var bestExamScore = await _repository.GetBestExamScoreAsync(userId);

        var points = (lessonsCompleted * PointsPerLessonCompleted) + examPoints;
        var level = (points / LevelStep) + 1;
        var levelProgressPercent = Math.Round((points % LevelStep) * 100.0 / LevelStep, 2);

        var bestStreakDays = await CalculateBestStreakDaysAsync(userId);
        var leaderboard = await BuildLeaderboardAsync(leaderboardSize);

        var achievements = await BuildAchievementsAsync(
            userId,
            points,
            lessonsCompleted,
            examsCompleted,
            bestStreakDays,
            bestExamScore
        );

        return new AchievementsHubDto
        {
            Stats = new AchievementStatsDto
            {
                Points = points,
                Level = level,
                LevelProgressPercent = levelProgressPercent,
                BestStreakDays = bestStreakDays,
                EarnedAchievementsCount = achievements.Count(a => a.Earned),
                TotalAchievementsCount = achievements.Count
            },
            Leaderboard = leaderboard,
            Achievements = achievements
        };
    }

    private async Task<int> CalculateBestStreakDaysAsync(Guid userId)
    {
        var lessonDays = await _repository.GetLessonCompletedDaysAsync(userId);
        var examDays = await _repository.GetExamCompletedDaysAsync(userId);

        var days = lessonDays
            .Concat(examDays)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        if (days.Count == 0)
        {
            return 0;
        }

        var best = 1;
        var current = 1;

        for (var i = 1; i < days.Count; i++)
        {
            var diff = (days[i] - days[i - 1]).TotalDays;

            if (diff == 1)
            {
                current++;

                if (current > best)
                {
                    best = current;
                }
            }
            else if (diff > 1)
            {
                current = 1;
            }
        }

        return best;
    }

    private async Task<List<LeaderboardEntryDto>> BuildLeaderboardAsync(int top)
    {
        var lessonPoints = await _repository.GetLessonPointsByUsersAsync();
        var examPoints = await _repository.GetExamPointsByUsersAsync();
        var students = await _repository.GetActiveStudentsAsync();

        var lessonDict = lessonPoints.ToDictionary(x => x.UserId, x => x.Points);
        var examDict = examPoints.ToDictionary(x => x.UserId, x => x.Points);

        var scored = students
            .Select(student =>
            {
                lessonDict.TryGetValue(student.UserId, out var lessonScore);
                examDict.TryGetValue(student.UserId, out var examScore);

                return new
                {
                    student.UserId,
                    student.FullName,
                    Points = lessonScore + examScore
                };
            })
            .OrderByDescending(x => x.Points)
            .ThenBy(x => x.FullName)
            .Take(top)
            .ToList();

        var leaderboard = new List<LeaderboardEntryDto>();

        var rank = 0;
        var index = 0;
        int? lastPoints = null;

        foreach (var item in scored)
        {
            index++;

            if (lastPoints == null || item.Points != lastPoints.Value)
            {
                rank = index;
            }

            lastPoints = item.Points;

            leaderboard.Add(new LeaderboardEntryDto
            {
                Rank = rank,
                UserId = item.UserId,
                FullName = item.FullName,
                Points = item.Points
            });
        }

        return leaderboard;
    }

    private async Task<List<AchievementDto>> BuildAchievementsAsync(
        Guid userId,
        int points,
        int lessonsCompleted,
        int examsCompleted,
        int bestStreakDays,
        int bestExamScore
    )
    {
        var allAchievements = await _repository.GetActiveAchievementsAsync();
        var earnedAchievements = await _repository.GetUserAchievementsAsync(userId);

        var earnedDict = earnedAchievements
            .GroupBy(x => x.AchievementId)
            .ToDictionary(g => g.Key, g => g.First().EarnedAt);

        var newEarned = new List<user_achievement>();
        var list = new List<AchievementDto>();

        foreach (var achievement in allAchievements)
        {
            var conditionType = NormalizeConditionType(achievement.ConditionType);
            var targetValue = achievement.ConditionValue ?? 0;

            var currentValue = GetCurrentValueForAchievement(
                conditionType,
                targetValue,
                points,
                lessonsCompleted,
                examsCompleted,
                bestStreakDays,
                bestExamScore
            );

            var alreadyEarned = earnedDict.ContainsKey(achievement.Id);
            var reached = targetValue > 0 && currentValue >= targetValue;

            DateTime? earnedAt = alreadyEarned
                ? earnedDict[achievement.Id]
                : null;

            if (reached && !alreadyEarned)
            {
                earnedAt = GetAchievementNow();

                newEarned.Add(new user_achievement
                {
                    id = Guid.NewGuid(),
                    user_id = userId,
                    achievement_id = achievement.Id,
                    earned_at = earnedAt
                });

                earnedDict[achievement.Id] = earnedAt;
            }

            var remaining = targetValue <= 0
                ? 0
                : Math.Max(0, targetValue - currentValue);

            var progressPercent = targetValue <= 0
                ? 0
                : Math.Min(100.0, currentValue * 100.0 / targetValue);

            list.Add(new AchievementDto
            {
                AchievementId = achievement.Id,
                Title = achievement.TitleAr ?? achievement.Title ?? "",
                Description = achievement.DescriptionAr ?? achievement.Description,
                ConditionType = conditionType,
                ConditionValue = targetValue,
                Earned = alreadyEarned || reached,
                EarnedAt = earnedAt,
                CurrentValue = currentValue,
                RemainingToEarn = remaining,
                ProgressPercent = Math.Round(progressPercent, 2)
            });
        }

        if (newEarned.Count > 0)
        {
            _repository.AddUserAchievements(newEarned);
            await _repository.SaveChangesAsync();
        }

        return list
            .OrderByDescending(x => x.Earned)
            .ThenBy(x => x.RemainingToEarn)
            .ThenBy(x => x.Title)
            .ToList();
    }

    private static int GetCurrentValueForAchievement(
        string conditionType,
        int targetValue,
        int points,
        int lessonsCompleted,
        int examsCompleted,
        int bestStreakDays,
        int bestExamScore
    )
    {
        return conditionType switch
        {
            "lessons" => lessonsCompleted,

            "streaks" => bestStreakDays,

            "points" => points,

            "exams" => targetValue >= 100
                ? bestExamScore
                : examsCompleted,

            "collaboration" => 0,

            _ => 0
        };
    }

    private static string NormalizeConditionType(string? conditionType)
    {
        var value = conditionType?.Trim() ?? "";

        return value switch
        {
            "lessons_completed" => "lessons",
            "weekly_lessons_completed" => "lessons",
            "complete_subject" => "lessons",

            "exams_completed" => "exams",
            "exam_score" => "exams",
            "subject_exam_score" => "exams",

            "streak_days" => "streaks",

            "correct_answers_streak" => "points",
            "all_subjects_above_score" => "exams",
            "fast_exam_finish" => "exams",
            "study_plan_commitment_month" => "streaks",

            "lessons" => "lessons",
            "exams" => "exams",
            "streaks" => "streaks",
            "points" => "points",
            "collaboration" => "collaboration",

            _ => value
        };
    }

    private static DateTime GetAchievementNow()
    {
        return DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
    }
}