using backend.Data.Generated;
using backend.DTOs.Admin;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Admin;

public class AdminDashboardRepository : IAdminDashboardRepository
{
    private readonly AppDbContext _context;

    public AdminDashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> GetAdminNameAsync(Guid adminId)
    {
        var adminName = await _context.users
            .AsNoTracking()
            .Where(u => u.id == adminId && u.role == "admin")
            .Select(u => u.full_name)
            .FirstOrDefaultAsync();

        return string.IsNullOrWhiteSpace(adminName)
            ? "Admin"
            : adminName;
    }

    public async Task<int> GetTotalStudentsAsync()
    {
        return await _context.users
            .AsNoTracking()
            .CountAsync(u => u.role == "student");
    }

    public async Task<int> GetTotalSubjectsAsync()
    {
        return await _context.subjects
            .AsNoTracking()
            .CountAsync();
    }

    public async Task<int> GetTotalLessonsAsync()
    {
        return await _context.lessons
            .AsNoTracking()
            .CountAsync();
    }

    public async Task<int> GetTotalExamsAsync()
    {
        return await _context.exams
            .AsNoTracking()
            .CountAsync();
    }

    public async Task<int> GetCompletedExamsAsync()
    {
        return await _context.exam_results
            .AsNoTracking()
            .CountAsync();
    }

    public async Task<int> GetNewStudentsThisMonthAsync()
    {
        var startOfMonth = GetStartOfCurrentMonthUtc();

        return await _context.users
            .AsNoTracking()
            .CountAsync(u =>
                u.role == "student" &&
                u.created_at != null &&
                u.created_at >= startOfMonth);
    }

    public async Task<int> GetNewSubjectsThisMonthAsync()
    {
        var startOfMonth = GetStartOfCurrentMonthUnspecified();

        return await _context.subjects
            .AsNoTracking()
            .CountAsync(s =>
                s.created_at != null &&
                s.created_at >= startOfMonth);
    }

    public Task<int> GetNewLessonsThisMonthAsync()
    {
        /*
         * جدول lessons عندك لا يحتوي created_at.
         * لذلك حالياً لا يمكن حساب الدروس الجديدة هذا الشهر بدقة.
         */
        return Task.FromResult(0);
    }

    public async Task<int> GetCompletedExamsThisMonthAsync()
    {
        var startOfMonth = GetStartOfCurrentMonthUnspecified();

        return await _context.exam_results
            .AsNoTracking()
            .CountAsync(e =>
                e.created_at != null &&
                e.created_at >= startOfMonth);
    }

    public async Task<AdminLessonCompletionDto> GetLessonCompletionAsync()
    {
        /*
         * بقيت موجودة للتوافق فقط.
         * الواجهة الرئيسية لم تعد تستخدم كرت معدل الإكمال.
         */

        var students = await _context.student_profiles
            .AsNoTracking()
            .Where(p =>
                p.stream != null &&
                p.stream != "")
            .Select(p => new
            {
                UserId = p.user_id,
                Stream = p.stream!
            })
            .ToListAsync();

        if (students.Count == 0)
        {
            return new AdminLessonCompletionDto
            {
                Percentage = 0,
                CompletedCount = 0,
                InProgressCount = 0
            };
        }

        var streams = students
            .Select(s => s.Stream)
            .Distinct()
            .ToList();

        var lessonsByStream = await _context.lessons
            .AsNoTracking()
            .Where(l => l.subject_id != null)
            .Join(
                _context.subjects.AsNoTracking(),
                lesson => lesson.subject_id!.Value,
                subject => subject.id,
                (lesson, subject) => new
                {
                    LessonId = lesson.id,
                    Stream = subject.stream
                }
            )
            .Where(x =>
                x.Stream != null &&
                streams.Contains(x.Stream))
            .Select(x => new
            {
                x.LessonId,
                Stream = x.Stream!
            })
            .ToListAsync();

        if (lessonsByStream.Count == 0)
        {
            return new AdminLessonCompletionDto
            {
                Percentage = 0,
                CompletedCount = 0,
                InProgressCount = 0
            };
        }

        var lessonsCountByStream = lessonsByStream
            .GroupBy(l => l.Stream)
            .ToDictionary(g => g.Key, g => g.Count());

        var totalRequiredLessons = students.Sum(student =>
            lessonsCountByStream.TryGetValue(student.Stream, out var count)
                ? count
                : 0
        );

        if (totalRequiredLessons == 0)
        {
            return new AdminLessonCompletionDto
            {
                Percentage = 0,
                CompletedCount = 0,
                InProgressCount = 0
            };
        }

        var completedProgress = await _context.lesson_progresses
            .AsNoTracking()
            .Where(lp =>
                lp.completed == true &&
                lp.user_id != null &&
                lp.lesson_id != null)
            .Select(lp => new
            {
                UserId = lp.user_id,
                LessonId = lp.lesson_id!.Value
            })
            .Distinct()
            .ToListAsync();

        var studentStreamByUserId = students
            .ToDictionary(x => x.UserId, x => x.Stream);

        var lessonStreamByLessonId = lessonsByStream
            .ToDictionary(x => x.LessonId, x => x.Stream);

        var completedCount = completedProgress.Count(progress =>
            progress.UserId.HasValue &&
            studentStreamByUserId.TryGetValue(progress.UserId.Value, out var studentStream) &&
            lessonStreamByLessonId.TryGetValue(progress.LessonId, out var lessonStream) &&
            studentStream == lessonStream
        );

        completedCount = Math.Min(completedCount, totalRequiredLessons);

        var remainingCount = Math.Max(0, totalRequiredLessons - completedCount);

        var percentage = (int)Math.Round(completedCount * 100.0 / totalRequiredLessons);

        return new AdminLessonCompletionDto
        {
            Percentage = percentage,
            CompletedCount = completedCount,
            InProgressCount = remainingCount
        };
    }

    public async Task<List<AdminMostActiveSubjectDto>> GetMostActiveSubjectsAsync(int limit)
    {
        var subjectActivities = await _context.study_sessions
            .AsNoTracking()
            .Where(ss => ss.subject_id != null)
            .GroupBy(ss => ss.subject_id)
            .Select(g => new
            {
                SubjectId = g.Key!.Value,
                ActivityValue = g.Count(),
                TotalMinutes = g.Sum(x => x.duration_minutes)
            })
            .OrderByDescending(x => x.ActivityValue)
            .ThenByDescending(x => x.TotalMinutes)
            .Take(limit)
            .ToListAsync();

        if (!subjectActivities.Any())
        {
            return new List<AdminMostActiveSubjectDto>();
        }

        var subjectIds = subjectActivities
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

        var maxActivityValue = subjectActivities.Max(x => x.ActivityValue);

        return subjectActivities
            .Select(activity =>
            {
                var subject = subjects.FirstOrDefault(s => s.id == activity.SubjectId);

                return new AdminMostActiveSubjectDto
                {
                    SubjectId = activity.SubjectId,
                    SubjectName = subject?.name ?? "مادة بدون اسم",
                    ActivityValue = activity.ActivityValue,
                    Percentage = maxActivityValue == 0
                        ? 0
                        : (int)Math.Round(activity.ActivityValue * 100.0 / maxActivityValue)
                };
            })
            .ToList();
    }

    public async Task<List<AdminRecentActivityDto>> GetRecentActivitiesAsync(int limit)
    {
        var recentStudents = await GetRecentStudentActivitiesAsync(limit);
        var recentExamResults = await GetRecentExamActivitiesAsync(limit);
        var recentLessonCompletions = await GetRecentLessonCompletionActivitiesAsync(limit);
        var recentAdminLogs = await GetRecentAdminLogActivitiesAsync(limit);

        return recentStudents
            .Concat(recentExamResults)
            .Concat(recentLessonCompletions)
            .Concat(recentAdminLogs)
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToList();
    }

    public async Task<List<AdminStudentPerformanceTrendDto>> GetStudentPerformanceTrendsAsync(int days)
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

        var examResults = await _context.exam_results
            .AsNoTracking()
            .Where(e =>
                e.created_at != null &&
                e.created_at >= startDate &&
                e.created_at < endDate)
            .Select(e => new
            {
                CreatedAt = e.created_at!.Value,
                Score = e.score ?? 0
            })
            .ToListAsync();

        var groupedResults = examResults
            .GroupBy(e => e.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                AverageScore = g.Average(x => x.Score)
            })
            .ToList();

        var trends = new List<AdminStudentPerformanceTrendDto>();

        for (var i = 0; i < days; i++)
        {
            var currentDate = startDate.AddDays(i);
            var result = groupedResults.FirstOrDefault(x => x.Date == currentDate.Date);

            trends.Add(new AdminStudentPerformanceTrendDto
            {
                Day = GetArabicDayName(currentDate.DayOfWeek),
                Value = result == null
                    ? 0
                    : (int)Math.Round(result.AverageScore)
            });
        }

        return trends;
    }

    private async Task<List<AdminRecentActivityDto>> GetRecentStudentActivitiesAsync(int limit)
    {
        var students = await _context.users
            .AsNoTracking()
            .Where(u => u.role == "student")
            .OrderByDescending(u => u.created_at)
            .Take(limit)
            .Select(u => new
            {
                u.full_name,
                u.created_at
            })
            .ToListAsync();

        return students
            .Select(u => new AdminRecentActivityDto
            {
                Type = "student_registered",
                Title = "تم تسجيل طالب جديد",
                Description = string.IsNullOrWhiteSpace(u.full_name)
                    ? "طالب جديد انضم إلى المنصة"
                    : $"{u.full_name} انضم إلى المنصة",
                CreatedAt = u.created_at ?? GetUtcNow()
            })
            .ToList();
    }

    private async Task<List<AdminRecentActivityDto>> GetRecentExamActivitiesAsync(int limit)
    {
        var examResults = await _context.exam_results
            .AsNoTracking()
            .OrderByDescending(r => r.created_at)
            .Take(limit)
            .Select(r => new
            {
                r.user_id,
                r.exam_id,
                r.score,
                r.created_at
            })
            .ToListAsync();

        if (!examResults.Any())
        {
            return new List<AdminRecentActivityDto>();
        }

        var userIds = examResults
            .Where(x => x.user_id != null)
            .Select(x => x.user_id!.Value)
            .Distinct()
            .ToList();

        var examIds = examResults
            .Where(x => x.exam_id != null)
            .Select(x => x.exam_id!.Value)
            .Distinct()
            .ToList();

        var users = await _context.users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.id))
            .Select(u => new
            {
                u.id,
                u.full_name
            })
            .ToListAsync();

        var exams = await _context.exams
            .AsNoTracking()
            .Where(e => examIds.Contains(e.id))
            .Select(e => new
            {
                e.id,
                e.title,
                e.type,
                e.subject_id
            })
            .ToListAsync();

        var subjectIds = exams
            .Where(e => e.subject_id != null)
            .Select(e => e.subject_id!.Value)
            .Distinct()
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

        return examResults
            .Select(result =>
            {
                var user = result.user_id == null
                    ? null
                    : users.FirstOrDefault(u => u.id == result.user_id.Value);

                var exam = result.exam_id == null
                    ? null
                    : exams.FirstOrDefault(e => e.id == result.exam_id.Value);

                var subject = exam?.subject_id == null
                    ? null
                    : subjects.FirstOrDefault(s => s.id == exam.subject_id.Value);

                var studentName = string.IsNullOrWhiteSpace(user?.full_name)
                    ? "طالب"
                    : user!.full_name;

                var examType = GetArabicExamType(exam?.type);
                var subjectName = string.IsNullOrWhiteSpace(subject?.name)
                    ? "مادة غير محددة"
                    : subject!.name;

                var scoreText = result.score == null
                    ? ""
                    : $" بنتيجة {(int)Math.Round(Convert.ToDouble(result.score.Value), 0)}%";

                return new AdminRecentActivityDto
                {
                    Type = "exam_completed",
                    Title = "اكتمال امتحان",
                    Description = $"{studentName} أنهى {examType} في {subjectName}{scoreText}",
                    CreatedAt = result.created_at ?? GetUtcNow()
                };
            })
            .ToList();
    }

    private async Task<List<AdminRecentActivityDto>> GetRecentLessonCompletionActivitiesAsync(int limit)
    {
        var lessonProgress = await _context.lesson_progresses
            .AsNoTracking()
            .Where(lp =>
                lp.completed == true &&
                lp.completed_at != null &&
                lp.user_id != null &&
                lp.lesson_id != null)
            .OrderByDescending(lp => lp.completed_at)
            .Take(limit)
            .Select(lp => new
            {
                UserId = lp.user_id,
                LessonId = lp.lesson_id!.Value,
                CompletedAt = lp.completed_at!.Value
            })
            .ToListAsync();

        if (!lessonProgress.Any())
        {
            return new List<AdminRecentActivityDto>();
        }

        var userIds = lessonProgress
            .Where(x => x.UserId != null)
            .Select(x => x.UserId!.Value)
            .Distinct()
            .ToList();

        var lessonIds = lessonProgress
            .Select(x => x.LessonId)
            .Distinct()
            .ToList();

        var users = await _context.users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.id))
            .Select(u => new
            {
                u.id,
                u.full_name
            })
            .ToListAsync();

        var lessons = await _context.lessons
            .AsNoTracking()
            .Where(l => lessonIds.Contains(l.id))
            .Select(l => new
            {
                l.id,
                l.title,
                l.subject_id
            })
            .ToListAsync();

        var subjectIds = lessons
            .Where(l => l.subject_id != null)
            .Select(l => l.subject_id!.Value)
            .Distinct()
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

        return lessonProgress
            .Select(progress =>
            {
                var user = progress.UserId == null
                    ? null
                    : users.FirstOrDefault(u => u.id == progress.UserId.Value);

                var lesson = lessons.FirstOrDefault(l => l.id == progress.LessonId);

                var subject = lesson?.subject_id == null
                    ? null
                    : subjects.FirstOrDefault(s => s.id == lesson.subject_id.Value);

                var studentName = string.IsNullOrWhiteSpace(user?.full_name)
                    ? "طالب"
                    : user!.full_name;

                var lessonTitle = string.IsNullOrWhiteSpace(lesson?.title)
                    ? "درس"
                    : lesson!.title;

                var subjectName = string.IsNullOrWhiteSpace(subject?.name)
                    ? "مادة غير محددة"
                    : subject!.name;

                return new AdminRecentActivityDto
                {
                    Type = "lesson_completed",
                    Title = "إكمال درس",
                    Description = $"{studentName} أكمل درس {lessonTitle} في مادة {subjectName}",
                    CreatedAt = progress.CompletedAt
                };
            })
            .ToList();
    }

    private async Task<List<AdminRecentActivityDto>> GetRecentAdminLogActivitiesAsync(int limit)
    {
        var logs = await _context.admin_logs
            .AsNoTracking()
            .OrderByDescending(a => a.created_at)
            .Take(limit)
            .Select(a => new
            {
                a.action_type,
                a.description,
                a.created_at
            })
            .ToListAsync();

        return logs
            .Select(a => new AdminRecentActivityDto
            {
                Type = "admin_action",
                Title = a.action_type ?? "نشاط إداري",
                Description = a.description ?? "",
                CreatedAt = a.created_at ?? GetUtcNow()
            })
            .ToList();
    }

    private static string GetArabicExamType(string? type)
    {
        return type switch
        {
            "short" => "اختبار قصير",
            "quick" => "اختبار قصير",
            "comprehensive" => "اختبار شامل",
            "full" => "اختبار شامل",
            _ => "امتحان"
        };
    }

    private static DateTime GetStartOfCurrentMonthUtc()
    {
        var now = DateTime.UtcNow;

        return new DateTime(
            now.Year,
            now.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc
        );
    }

    private static DateTime GetStartOfCurrentMonthUnspecified()
    {
        var now = DateTime.Now;

        return new DateTime(
            now.Year,
            now.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Unspecified
        );
    }

    private static DateTime GetUtcNow()
    {
        return DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    private static string GetArabicDayName(DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Saturday => "سبت",
            DayOfWeek.Sunday => "أحد",
            DayOfWeek.Monday => "اثنين",
            DayOfWeek.Tuesday => "ثلاثاء",
            DayOfWeek.Wednesday => "أربعاء",
            DayOfWeek.Thursday => "خميس",
            DayOfWeek.Friday => "جمعة",
            _ => ""
        };
    }
}