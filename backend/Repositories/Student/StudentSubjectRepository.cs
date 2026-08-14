using backend.Data.Generated;
using backend.Models.Generated;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Student;

public class StudentSubjectRepository : IStudentSubjectRepository
{
    private readonly AppDbContext _db;

    public StudentSubjectRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<string?> GetStudentStreamAsync(Guid userId)
    {
        return _db.student_profiles
            .AsNoTracking()
            .Where(p => p.user_id == userId)
            .Select(p => p.stream)
            .FirstOrDefaultAsync();
    }

    public Task<List<SubjectListData>> GetSubjectsByStreamAsync(string stream)
    {
        return _db.subjects
            .AsNoTracking()
            .Where(s => s.stream == stream)
            .OrderBy(s => s.name)
            .Select(s => new SubjectListData
            {
                SubjectId = s.id,
                SubjectName = s.name,
                Description = s.description ?? "",
                LessonsCount = s.lessons.Count()
            })
            .ToListAsync();
    }

    public Task<List<CompletedLessonData>> GetCompletedLessonsForSubjectsAsync(
        Guid userId,
        List<Guid> subjectIds
    )
    {
        if (subjectIds.Count == 0)
        {
            return Task.FromResult(new List<CompletedLessonData>());
        }

        return _db.lesson_progresses
            .AsNoTracking()
            .Where(lp => lp.user_id == userId && lp.completed == true && lp.lesson_id != null)
            .Join(
                _db.lessons.AsNoTracking()
                    .Where(l => l.subject_id != null && subjectIds.Contains(l.subject_id.Value)),
                lp => lp.lesson_id!.Value,
                l => l.id,
                (lp, l) => new CompletedLessonData
                {
                    SubjectId = l.subject_id!.Value,
                    LessonId = l.id
                }
            )
            .ToListAsync();
    }

    public Task<subject?> GetSubjectByIdAndStreamAsync(Guid subjectId, string stream)
    {
        return _db.subjects
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.id == subjectId && s.stream == stream);
    }

    public Task<List<subject_unit>> GetUnitsBySubjectIdAsync(Guid subjectId)
    {
        return _db.subject_units
            .AsNoTracking()
            .Where(u => u.subject_id == subjectId)
            .OrderBy(u => u.order_number)
            .ToListAsync();
    }

    public Task<List<lesson>> GetLessonsBySubjectIdAsync(Guid subjectId)
    {
        return _db.lessons
            .AsNoTracking()
            .Where(l => l.subject_id == subjectId)
            .OrderBy(l => l.subject_unit_id)
            .ThenBy(l => l.order_number ?? int.MaxValue)
            .ThenBy(l => l.title)
            .ToListAsync();
    }

    public Task<bool> LessonBelongsToSubjectAsync(Guid subjectId, Guid lessonId)
    {
        return _db.lessons
            .AsNoTracking()
            .AnyAsync(l => l.id == lessonId && l.subject_id == subjectId);
    }

    public Task<lesson?> GetLessonByIdAsync(Guid lessonId)
    {
        return _db.lessons
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.id == lessonId);
    }

    public Task<bool> IsLessonCompletedAsync(Guid userId, Guid lessonId)
    {
        return _db.lesson_progresses
            .AsNoTracking()
            .AnyAsync(lp =>
                lp.user_id == userId &&
                lp.lesson_id == lessonId &&
                lp.completed == true);
    }

    public Task<lesson_progress?> GetLessonProgressAsync(Guid userId, Guid lessonId)
    {
        return _db.lesson_progresses
            .FirstOrDefaultAsync(lp =>
                lp.user_id == userId &&
                lp.lesson_id == lessonId);
    }

    public void AddLessonProgress(lesson_progress progress)
    {
        _db.lesson_progresses.Add(progress);
    }

    public Task<int> CountCompletedLessonsAsync(Guid userId)
    {
        return _db.lesson_progresses
            .AsNoTracking()
            .Where(lp => lp.user_id == userId && lp.completed == true)
            .CountAsync();
    }

    public Task<List<achievement>> GetAchievementsByConditionTypeAsync(string conditionType)
    {
        return _db.achievements
            .AsNoTracking()
            .Where(a =>
                a.condition_type == conditionType &&
                (a.is_active == null || a.is_active == true))
            .OrderBy(a => a.condition_value)
            .ToListAsync();
    }

    public async Task<HashSet<Guid>> GetEarnedAchievementIdsAsync(Guid userId)
    {
        var earnedAchievementIds = await _db.user_achievements
            .AsNoTracking()
            .Where(ua => ua.user_id == userId && ua.achievement_id != null)
            .Select(ua => ua.achievement_id!.Value)
            .ToListAsync();

        return new HashSet<Guid>(earnedAchievementIds);
    }

    public void AddUserAchievement(user_achievement userAchievement)
    {
        _db.user_achievements.Add(userAchievement);
    }

    public Task<bool> HasStudySessionForLessonAsync(Guid userId, Guid lessonId)
    {
        return _db.study_sessions
            .AsNoTracking()
            .AnyAsync(s =>
                s.user_id == userId &&
                s.lesson_id == lessonId &&
                s.session_type == "study");
    }

    public void AddStudySession(study_session studySession)
    {
        _db.study_sessions.Add(studySession);
    }

    public Task<List<subject>> GetSubjectsByBranchAsync(string branch)
    {
        return _db.subjects
            .AsNoTracking()
            .Where(s => s.stream == branch)
            .OrderBy(s => s.name)
            .ToListAsync();
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}