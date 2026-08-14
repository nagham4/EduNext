using backend.Data.Generated;
using backend.DTOs.Admin;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Admin;

public class AdminAnalyticsRepository : IAdminAnalyticsRepository
{
    private readonly AppDbContext _context;

    private readonly string[] _colors =
    {
        "#135bec",
        "#8b5cf6",
        "#f59e0b",
        "#22c55e",
        "#ef4444"
    };

    public AdminAnalyticsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminAnalyticsDto> GetAnalyticsAsync(int days)
    {
        days = days <= 0 ? 30 : days;
        days = days > 90 ? 90 : days;

        return new AdminAnalyticsDto
        {
            Stats = await GetStatsAsync(),
            DailyExams = await GetDailyExamsAsync(days),
            PopularSubjects = await GetPopularSubjectsAsync(days),
            ActivityTimes = await GetActivityTimesAsync(days),
            LessonCompletion = await GetLessonCompletionAsync()
        };
    }

    private async Task<AdminAnalyticsStatsDto> GetStatsAsync()
    {
        var completedExams = await _context.exam_results
            .AsNoTracking()
            .CountAsync();

        var activeStudents = await _context.users
            .AsNoTracking()
            .CountAsync(u => u.role == "student" && u.is_active == true);

        return new AdminAnalyticsStatsDto
        {
            CompletedExams = completedExams,
            ActiveStudents = activeStudents,
            CompletedExamsChange = "عدد محاولات الامتحانات المكتملة",
            ActiveStudentsChange = "حسب الطلاب النشطين حالياً"
        };
    }

    private async Task<List<AdminDailyExamDto>> GetDailyExamsAsync(int days)
    {
        var today = DateTime.Now.Date;

        var startDate = DateTime.SpecifyKind(
            today.AddDays(-days + 1),
            DateTimeKind.Unspecified
        );

        var endDate = DateTime.SpecifyKind(
            today.AddDays(1),
            DateTimeKind.Unspecified
        );

        var results = await _context.exam_results
            .AsNoTracking()
            .Where(x =>
                x.created_at != null &&
                x.created_at >= startDate &&
                x.created_at < endDate)
            .Select(x => x.created_at!.Value)
            .ToListAsync();

        var grouped = results
            .GroupBy(x => x.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var data = new List<AdminDailyExamDto>();

        for (var i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i).Date;

            data.Add(new AdminDailyExamDto
            {
                Name = FormatArabicDate(date),
                Value = grouped.TryGetValue(date, out var count) ? count : 0
            });
        }

        return data;
    }

    private async Task<List<AdminPopularSubjectDto>> GetPopularSubjectsAsync(int days)
    {
        var today = DateTime.Now.Date;

        var startDate = DateTime.SpecifyKind(
            today.AddDays(-days + 1),
            DateTimeKind.Unspecified
        );

        var endDate = DateTime.SpecifyKind(
            today.AddDays(1),
            DateTimeKind.Unspecified
        );

        var activities = await _context.study_sessions
            .AsNoTracking()
            .Where(x =>
                x.subject_id != null &&
                x.started_at >= startDate &&
                x.started_at < endDate)
            .GroupBy(x => x.subject_id)
            .Select(g => new
            {
                SubjectId = g.Key!.Value,
                Minutes = g.Sum(x => x.duration_minutes)
            })
            .OrderByDescending(x => x.Minutes)
            .Take(5)
            .ToListAsync();

        if (!activities.Any())
        {
            return new List<AdminPopularSubjectDto>();
        }

        var subjectIds = activities
            .Select(x => x.SubjectId)
            .ToList();

        var subjects = await _context.subjects
            .AsNoTracking()
            .Where(s => subjectIds.Contains(s.id))
            .Select(s => new
            {
                s.id,
                s.name
            })
            .ToListAsync();

        var totalMinutes = activities.Sum(x => x.Minutes);

        return activities
            .Select((item, index) =>
            {
                var subject = subjects.FirstOrDefault(s => s.id == item.SubjectId);

                return new AdminPopularSubjectDto
                {
                    Name = subject?.name ?? "مادة بدون اسم",
                    ActivityCount = item.Minutes,
                    Unit = "دقيقة",
                    Value = totalMinutes == 0
                        ? 0
                        : (int)Math.Round((double)item.Minutes / totalMinutes * 100),
                    Color = _colors[index % _colors.Length]
                };
            })
            .ToList();
    }

    private async Task<List<AdminActivityTimeDto>> GetActivityTimesAsync(int days)
    {
        var today = DateTime.Now.Date;

        var startDate = DateTime.SpecifyKind(
            today.AddDays(-days + 1),
            DateTimeKind.Unspecified
        );

        var endDate = DateTime.SpecifyKind(
            today.AddDays(1),
            DateTimeKind.Unspecified
        );

        var sessions = await _context.study_sessions
            .AsNoTracking()
            .Where(x =>
                x.started_at >= startDate &&
                x.started_at < endDate)
            .Select(x => new
            {
                x.started_at,
                x.duration_minutes
            })
            .ToListAsync();

        var slots = new[]
        {
            new { Label = "08:00 ص", Hour = 8 },
            new { Label = "12:00 م", Hour = 12 },
            new { Label = "04:00 م", Hour = 16 },
            new { Label = "08:00 م", Hour = 20 },
            new { Label = "11:00 م", Hour = 23 }
        };

        var counts = slots
            .Select(slot => new
            {
                slot.Label,
                Minutes = sessions
                    .Where(x => Math.Abs(x.started_at.Hour - slot.Hour) <= 1)
                    .Sum(x => x.duration_minutes)
            })
            .ToList();

        var maxMinutes = counts.Any()
            ? counts.Max(x => x.Minutes)
            : 0;

        return counts
            .Select(x =>
            {
                var percent = maxMinutes == 0
                    ? 0
                    : (int)Math.Round((double)x.Minutes / maxMinutes * 100);

                return new AdminActivityTimeDto
                {
                    Time = x.Label,
                    Minutes = x.Minutes,
                    Percent = percent,
                    Level = GetActivityLevel(percent)
                };
            })
            .ToList();
    }

    private async Task<List<AdminSubjectLessonCompletionDto>> GetLessonCompletionAsync()
    {
        var activeStudentIds = await _context.users
            .AsNoTracking()
            .Where(u => u.role == "student" && u.is_active == true)
            .Select(u => u.id)
            .ToListAsync();

        if (!activeStudentIds.Any())
        {
            return new List<AdminSubjectLessonCompletionDto>();
        }

        var lessonTotals = await _context.lessons
            .AsNoTracking()
            .Where(l => l.subject_id != null)
            .GroupBy(l => l.subject_id!.Value)
            .Select(g => new
            {
                SubjectId = g.Key,
                TotalLessons = g.Count()
            })
            .ToListAsync();

        if (!lessonTotals.Any())
        {
            return new List<AdminSubjectLessonCompletionDto>();
        }

        var subjectIds = lessonTotals
            .Select(x => x.SubjectId)
            .ToList();

        var subjects = await _context.subjects
            .AsNoTracking()
            .Where(s => subjectIds.Contains(s.id))
            .Select(s => new
            {
                s.id,
                s.name
            })
            .ToListAsync();

        var completedRows = await _context.lesson_progresses
            .AsNoTracking()
            .Where(lp =>
                lp.completed == true &&
                lp.user_id != null &&
                activeStudentIds.Contains(lp.user_id.Value) &&
                lp.lesson_id != null &&
                lp.lesson != null &&
                lp.lesson.subject_id != null)
            .Select(lp => new
            {
                UserId = lp.user_id!.Value,
                LessonId = lp.lesson_id!.Value,
                SubjectId = lp.lesson!.subject_id!.Value
            })
            .Distinct()
            .ToListAsync();

        var completedBySubject = completedRows
            .GroupBy(x => x.SubjectId)
            .ToDictionary(g => g.Key, g => g.Count());

        return lessonTotals
            .Select(item =>
            {
                var subject = subjects.FirstOrDefault(s => s.id == item.SubjectId);

                var requiredCount = item.TotalLessons * activeStudentIds.Count;

                var completedCount = completedBySubject.TryGetValue(
                    item.SubjectId,
                    out var count
                )
                    ? count
                    : 0;

                return new AdminSubjectLessonCompletionDto
                {
                    Subject = subject?.name ?? "مادة بدون اسم",
                    RequiredCount = requiredCount,
                    CompletedCount = completedCount,
                    Percent = requiredCount == 0
                        ? 0
                        : (int)Math.Round((double)completedCount / requiredCount * 100)
                };
            })
            .OrderByDescending(x => x.Percent)
            .ThenBy(x => x.Subject)
            .Take(5)
            .ToList();
    }

    private static string GetActivityLevel(int percent)
    {
        if (percent >= 85) return "ذروة";
        if (percent >= 60) return "مرتفع";
        if (percent >= 35) return "متوسط";
        if (percent > 0) return "منخفض";

        return "لا يوجد";
    }

    private static string FormatArabicDate(DateTime date)
    {
        return $"{date.Day} {GetArabicMonthName(date.Month)}";
    }

    private static string GetArabicMonthName(int month)
    {
        return month switch
        {
            1 => "يناير",
            2 => "فبراير",
            3 => "مارس",
            4 => "أبريل",
            5 => "مايو",
            6 => "يونيو",
            7 => "يوليو",
            8 => "أغسطس",
            9 => "سبتمبر",
            10 => "أكتوبر",
            11 => "نوفمبر",
            12 => "ديسمبر",
            _ => ""
        };
    }
}