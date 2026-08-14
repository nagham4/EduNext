using backend.Data.Generated;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Student;

public class StudentDashboardRepository : IStudentDashboardRepository
{
    private readonly AppDbContext _db;

    public StudentDashboardRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardIdentityData?> GetIdentityAsync(Guid userId)
    {
        return await _db.users
            .AsNoTracking()
            .Where(u => u.id == userId)
            .GroupJoin(
                _db.student_profiles.AsNoTracking(),
                user => user.id,
                profile => profile.user_id,
                (user, profiles) => new
                {
                    User = user,
                    Profile = profiles.FirstOrDefault()
                }
            )
            .Select(x => new DashboardIdentityData
            {
                FullName = x.User.full_name ?? "الطالب",
                Stream = x.Profile != null ? x.Profile.stream : null,
                CurrentLevel = x.Profile != null ? x.Profile.current_grade ?? "" : "",
                Goal = x.Profile != null ? x.Profile.primary_goal ?? "" : "",
                StudyHours = x.Profile != null ? x.Profile.preferred_study_time ?? "" : "",
                ExamExperience = x.Profile != null ? x.Profile.exam_experience ?? "" : "",
                LearningMethods = x.Profile != null && x.Profile.learning_methods != null
                    ? x.Profile.learning_methods
                    : new List<string>(),
                DifficultSubjects = x.Profile != null
                    ? x.Profile.student_profile_subjects
                        .Where(sps => sps.subject != null)
                        .Select(sps => sps.subject!.name)
                        .ToList()
                    : new List<string>()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<DashboardWeeklySessionData>> GetWeeklySessionsAsync(
        Guid userId,
        DateTime startOfWeek,
        DateTime endOfWeek
    )
    {
        return await _db.study_sessions
            .AsNoTracking()
            .Where(s =>
                s.user_id == userId &&
                s.started_at >= startOfWeek &&
                s.started_at < endOfWeek)
            .GroupBy(s => s.started_at.Date)
            .Select(g => new DashboardWeeklySessionData
            {
                Date = g.Key,
                Minutes = g.Sum(x => x.duration_minutes)
            })
            .ToListAsync();
    }

    public async Task<int> GetTotalStudyMinutesAsync(Guid userId)
    {
        return await _db.study_sessions
            .AsNoTracking()
            .Where(s => s.user_id == userId)
            .SumAsync(s => (int?)s.duration_minutes) ?? 0;
    }

    public async Task<List<DashboardSubjectData>> GetSubjectsByStreamAsync(string stream)
    {
        return await _db.subjects
            .AsNoTracking()
            .Where(s => s.stream == stream)
            .OrderBy(s => s.name)
            .Select(s => new DashboardSubjectData
            {
                SubjectId = s.id,
                SubjectName = s.name
            })
            .ToListAsync();
    }

    public async Task<List<DashboardSubjectLessonTotalData>> GetSubjectLessonTotalsAsync(List<Guid> subjectIds)
    {
        if (subjectIds.Count == 0)
        {
            return new List<DashboardSubjectLessonTotalData>();
        }

        return await _db.lessons
            .AsNoTracking()
            .Where(l => l.subject_id != null && subjectIds.Contains(l.subject_id.Value))
            .GroupBy(l => l.subject_id!.Value)
            .Select(g => new DashboardSubjectLessonTotalData
            {
                SubjectId = g.Key,
                Total = g.Count()
            })
            .ToListAsync();
    }

    public async Task<List<DashboardSubjectCompletedData>> GetSubjectCompletedLessonsAsync(
        Guid userId,
        List<Guid> subjectIds
    )
    {
        if (subjectIds.Count == 0)
        {
            return new List<DashboardSubjectCompletedData>();
        }

        return await _db.lesson_progresses
            .AsNoTracking()
            .Where(lp => lp.user_id == userId && lp.completed == true && lp.lesson_id != null)
            .Join(
                _db.lessons.AsNoTracking()
                    .Where(l => l.subject_id != null && subjectIds.Contains(l.subject_id.Value)),
                lp => lp.lesson_id!.Value,
                lesson => lesson.id,
                (lp, lesson) => new
                {
                    SubjectId = lesson.subject_id!.Value
                }
            )
            .GroupBy(x => x.SubjectId)
            .Select(g => new DashboardSubjectCompletedData
            {
                SubjectId = g.Key,
                Completed = g.Count()
            })
            .ToListAsync();
    }

    public async Task<DashboardExamSummaryData> GetExamSummaryAsync(Guid userId, List<Guid> subjectIds)
    {
        if (subjectIds.Count == 0)
        {
            return new DashboardExamSummaryData();
        }

        var summary = await _db.exam_results
            .AsNoTracking()
            .Where(r => r.user_id == userId && r.exam_id != null)
            .Join(
                _db.exams.AsNoTracking(),
                result => result.exam_id!.Value,
                exam => exam.id,
                (result, exam) => new
                {
                    Result = result,
                    Exam = exam
                }
            )
            .Where(x => x.Exam.subject_id != null && subjectIds.Contains(x.Exam.subject_id.Value))
            .GroupBy(_ => 1)
            .Select(g => new DashboardExamSummaryData
            {
                Count = g.Count(),
                AverageScore = g.Average(x => (double)(x.Result.score ?? 0))
            })
            .FirstOrDefaultAsync();

        return summary ?? new DashboardExamSummaryData();
    }

    public async Task<List<DashboardSubjectExamAverageData>> GetSubjectExamAveragesAsync(Guid userId, List<Guid> subjectIds)
    {
        if (subjectIds.Count == 0)
        {
            return new List<DashboardSubjectExamAverageData>();
        }

        return await _db.exam_results
            .AsNoTracking()
            .Where(r => r.user_id == userId && r.exam_id != null)
            .Join(
                _db.exams.AsNoTracking().Where(e => e.subject_id != null && subjectIds.Contains(e.subject_id.Value)),
                result => result.exam_id!.Value,
                exam => exam.id,
                (result, exam) => new
                {
                    SubjectId = exam.subject_id!.Value,
                    Score = (double)(result.score ?? 0)
                }
            )
            .GroupBy(x => x.SubjectId)
            .Select(g => new DashboardSubjectExamAverageData
            {
                SubjectId = g.Key,
                AverageScore = Math.Round(g.Average(x => x.Score), 2)
            })
            .ToListAsync();
    }

    public async Task<string?> GetLatestAiRecommendationAsync(Guid userId)
    {
        return await _db.ai_recommendations
            .AsNoTracking()
            .Where(a =>
                a.user_id == userId &&
                a.recommendation_text != null &&
                a.recommendation_text != "")
            .OrderByDescending(a => a.created_at)
            .Select(a => a.recommendation_text)
            .FirstOrDefaultAsync();
    }

    public async Task<List<DashboardLessonCandidateData>> GetNextLessonsForSubjectsAsync(
        Guid userId,
        List<Guid> subjectIds
    )
    {
        if (subjectIds.Count == 0)
        {
            return new List<DashboardLessonCandidateData>();
        }

        return await _db.lessons
            .AsNoTracking()
            .Where(l =>
                l.subject_id != null &&
                subjectIds.Contains(l.subject_id.Value))
            .Where(l => !_db.lesson_progresses.Any(lp =>
                lp.user_id == userId &&
                lp.lesson_id == l.id &&
                lp.completed == true))
            .OrderBy(l => l.subject_id)
            .ThenBy(l => l.order_number ?? int.MaxValue)
            .ThenBy(l => l.title)
            .Select(l => new DashboardLessonCandidateData
            {
                SubjectId = l.subject_id!.Value,
                LessonId = l.id,
                LessonTitle = l.title
            })
            .ToListAsync();
    }
}
