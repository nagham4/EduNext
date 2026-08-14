using Microsoft.EntityFrameworkCore;
using backend.Data.Generated;
using backend.DTOs.Student;

namespace backend.Services.Student;

public class ProgressService
{
    private readonly AppDbContext _db;

    public ProgressService(AppDbContext db)
    {
        _db = db;
    }

    // ✅ Overall progress (MVP)
    public async Task<LessonProgressDto> GetProgressAsync(Guid userId)
    {
        var totalLessons = await _db.lessons
            .AsNoTracking()
            .CountAsync();

        var completedLessons = await _db.lesson_progresses
            .AsNoTracking()
            .Where(lp => lp.user_id == userId && lp.completed == true)
            .CountAsync();

        var remainingLessons = Math.Max(0, totalLessons - completedLessons);

        var progressPercent = totalLessons == 0
            ? 0
            : (completedLessons * 100.0 / totalLessons);

        return new LessonProgressDto
        {
            CompletedLessons = completedLessons,
            RemainingLessons = remainingLessons,
            ProgressPercent = Math.Round(progressPercent, 2)
        };
    }

    // ✅ Progress by subject (EF-translatable version)
    public async Task<List<SubjectProgressDto>> GetProgressBySubjectAsync(Guid userId)
    {
        // 1) Total lessons per subject
        var totals = await _db.lessons.AsNoTracking()
            .Where(l => l.subject_id != null)
            .GroupBy(l => l.subject_id!.Value)
            .Select(g => new { SubjectId = g.Key, Total = g.Count() })
            .ToListAsync();

        // 2) Completed lessons per subject for this user
        var completed = await _db.lesson_progresses.AsNoTracking()
            .Where(lp => lp.user_id == userId && lp.completed == true && lp.lesson_id != null)
            .Join(_db.lessons.AsNoTracking(),
                lp => lp.lesson_id!.Value,
                l => l.id,
                (lp, l) => new { l.subject_id })
            .Where(x => x.subject_id != null)
            .GroupBy(x => x.subject_id!.Value)
            .Select(g => new { SubjectId = g.Key, Completed = g.Count() })
            .ToListAsync();

        // 3) Subjects list
        var subjects = await _db.subjects.AsNoTracking()
            .OrderBy(s => s.name)
            .Select(s => new { s.id, s.name })
            .ToListAsync();

        var totalDict = totals.ToDictionary(x => x.SubjectId, x => x.Total);
        var completedDict = completed.ToDictionary(x => x.SubjectId, x => x.Completed);

        // 4) Merge in memory
        var result = subjects.Select(s =>
        {
            totalDict.TryGetValue(s.id, out var total);
            completedDict.TryGetValue(s.id, out var comp);

            var remaining = Math.Max(0, total - comp);
            var pct = total == 0 ? 0 : Math.Round(comp * 100.0 / total, 2);

            return new SubjectProgressDto
            {
                SubjectId = s.id,
                SubjectName = s.name ?? "",
                CompletedLessons = comp,
                RemainingLessons = remaining,
                ProgressPercent = pct
            };
        }).ToList();

        return result;
    }
}