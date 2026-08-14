using backend.Data.Generated;
using backend.Models.Generated;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Student;

public class StudentProfileRepository : IStudentProfileRepository
{
    private readonly AppDbContext _db;

    public StudentProfileRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<user?> GetUserWithProfileAsync(Guid userId)
    {
        return _db.users
            .AsNoTracking()
            .Include(u => u.student_profile)
            .FirstOrDefaultAsync(u => u.id == userId);
    }

    public Task<user?> GetUserForUpdateAsync(Guid userId)
    {
        return _db.users
            .FirstOrDefaultAsync(u => u.id == userId);
    }

    public Task<List<StudentProfileActivityData>> GetLessonActivitiesAsync(Guid userId, int limit)
    {
        return _db.lesson_progresses
            .AsNoTracking()
            .Where(lp =>
                lp.user_id == userId &&
                lp.completed == true &&
                lp.completed_at != null)
            .OrderByDescending(lp => lp.completed_at)
            .Take(limit)
            .Select(lp => new StudentProfileActivityData
            {
                Date = lp.completed_at!.Value,
                Type = "lesson",
                Text = lp.lesson != null
                    ? $"أكملت درس {lp.lesson.title}"
                    : "أكملت درسًا",
                Color = "blue"
            })
            .ToListAsync();
    }

    public Task<List<StudentProfileActivityData>> GetExamActivitiesAsync(Guid userId, int limit)
    {
        return _db.exam_results
            .AsNoTracking()
            .Where(r =>
                r.user_id == userId &&
                r.created_at != null)
            .OrderByDescending(r => r.created_at)
            .Take(limit)
            .Select(r => new StudentProfileActivityData
            {
                Date = r.created_at!.Value,
                Type = "exam",
                Text = r.exam != null
                    ? $"حصلت على {ToArabicNumber(r.score.GetValueOrDefault())}٪ في اختبار {r.exam.title}"
                    : $"حصلت على {ToArabicNumber(r.score.GetValueOrDefault())}٪ في اختبار",
                Color = "amber"
            })
            .ToListAsync();
    }

    public Task<List<StudentProfileActivityData>> GetAchievementActivitiesAsync(Guid userId, int limit)
    {
        return _db.user_achievements
            .AsNoTracking()
            .Where(a =>
                a.user_id == userId &&
                a.earned_at != null)
            .OrderByDescending(a => a.earned_at)
            .Take(limit)
            .Select(a => new StudentProfileActivityData
            {
                Date = a.earned_at!.Value,
                Type = "achievement",
                Text = a.achievement != null
                    ? $"حصلت على شارة '{(a.achievement.title_ar ?? a.achievement.title)}'"
                    : "حصلت على إنجاز جديد",
                Color = "purple"
            })
            .ToListAsync();
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }

    private static string ToArabicNumber(int number)
    {
        return number.ToString()
            .Replace('0', '٠')
            .Replace('1', '١')
            .Replace('2', '٢')
            .Replace('3', '٣')
            .Replace('4', '٤')
            .Replace('5', '٥')
            .Replace('6', '٦')
            .Replace('7', '٧')
            .Replace('8', '٨')
            .Replace('9', '٩');
    }
}
