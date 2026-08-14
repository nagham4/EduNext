using backend.Data.Generated;
using backend.Models.Generated;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Student;

public class StudentAchievementRepository : IStudentAchievementRepository
{
    private readonly AppDbContext _db;

    private const int PointsPerLessonCompleted = 50;

    public StudentAchievementRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<int> GetCompletedLessonsCountAsync(Guid userId)
    {
        return _db.lesson_progresses
            .AsNoTracking()
            .Where(lp => lp.user_id == userId && lp.completed == true)
            .CountAsync();
    }

    public Task<int> GetCompletedExamsCountAsync(Guid userId)
    {
        return _db.exam_results
            .AsNoTracking()
            .Where(r => r.user_id == userId)
            .CountAsync();
    }

    public async Task<int> GetExamPointsAsync(Guid userId)
    {
        return await _db.exam_results
            .AsNoTracking()
            .Where(r => r.user_id == userId)
            .Select(r => (int?)(r.score ?? 0))
            .SumAsync() ?? 0;
    }

    public async Task<int> GetBestExamScoreAsync(Guid userId)
    {
        return await _db.exam_results
            .AsNoTracking()
            .Where(r => r.user_id == userId)
            .Select(r => (int?)(r.score ?? 0))
            .MaxAsync() ?? 0;
    }

    public Task<List<DateTime>> GetLessonCompletedDaysAsync(Guid userId)
    {
        return _db.lesson_progresses
            .AsNoTracking()
            .Where(lp =>
                lp.user_id == userId &&
                lp.completed == true &&
                lp.completed_at != null)
            .Select(lp => lp.completed_at!.Value.Date)
            .ToListAsync();
    }

    public Task<List<DateTime>> GetExamCompletedDaysAsync(Guid userId)
    {
        return _db.exam_results
            .AsNoTracking()
            .Where(r =>
                r.user_id == userId &&
                r.created_at != null)
            .Select(r => r.created_at!.Value.Date)
            .ToListAsync();
    }

    public Task<List<AchievementData>> GetActiveAchievementsAsync()
    {
        return _db.achievements
            .AsNoTracking()
            .Where(a => a.is_active == true)
            .OrderBy(a => a.condition_type)
            .ThenBy(a => a.condition_value)
            .ThenBy(a => a.title_ar ?? a.title)
            .Select(a => new AchievementData
            {
                Id = a.id,
                Title = a.title,
                TitleAr = a.title_ar,
                Description = a.description,
                DescriptionAr = a.description_ar,
                ConditionType = a.condition_type,
                ConditionValue = a.condition_value
            })
            .ToListAsync();
    }

    public Task<List<UserAchievementData>> GetUserAchievementsAsync(Guid userId)
    {
        return _db.user_achievements
            .AsNoTracking()
            .Where(ua => ua.user_id == userId && ua.achievement_id != null)
            .Select(ua => new UserAchievementData
            {
                AchievementId = ua.achievement_id!.Value,
                EarnedAt = ua.earned_at
            })
            .ToListAsync();
    }

    public Task<List<UserLessonPointsData>> GetLessonPointsByUsersAsync()
    {
        return _db.lesson_progresses
            .AsNoTracking()
            .Where(lp => lp.completed == true && lp.user_id != null)
            .GroupBy(lp => lp.user_id!.Value)
            .Select(g => new UserLessonPointsData
            {
                UserId = g.Key,
                Points = g.Count() * PointsPerLessonCompleted
            })
            .ToListAsync();
    }

    public Task<List<UserExamPointsData>> GetExamPointsByUsersAsync()
    {
        return _db.exam_results
            .AsNoTracking()
            .Where(r => r.user_id != null)
            .GroupBy(r => r.user_id!.Value)
            .Select(g => new UserExamPointsData
            {
                UserId = g.Key,
                Points = g.Sum(x => (int)(x.score ?? 0))
            })
            .ToListAsync();
    }

    public Task<List<StudentLeaderboardUserData>> GetActiveStudentsAsync()
    {
        return _db.users
            .AsNoTracking()
            .Where(u => u.role == "student" && u.is_active == true)
            .Select(u => new StudentLeaderboardUserData
            {
                UserId = u.id,
                FullName = u.full_name ?? ""
            })
            .ToListAsync();
    }

    public void AddUserAchievements(List<user_achievement> achievements)
    {
        if (achievements.Count > 0)
        {
            _db.user_achievements.AddRange(achievements);
        }
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}