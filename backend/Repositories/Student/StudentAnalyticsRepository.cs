using backend.Data.Generated;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Student;

public class StudentAnalyticsRepository : IStudentAnalyticsRepository
{
    private readonly AppDbContext _db;

    public StudentAnalyticsRepository(AppDbContext db)
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

    public async Task<StudentAnalyticsExamSummaryData> GetExamSummaryAsync(Guid userId)
    {
        var summary = await _db.exam_results
            .AsNoTracking()
            .Where(r => r.user_id == userId)
            .GroupBy(_ => 1)
            .Select(g => new StudentAnalyticsExamSummaryData
            {
                TotalExams = g.Count(),
                PassedExams = g.Count(x => (x.score ?? 0) >= 50),
                AverageScore = g.Average(x => (double)(x.score ?? 0))
            })
            .FirstOrDefaultAsync();

        if (summary == null)
        {
            return new StudentAnalyticsExamSummaryData
            {
                TotalExams = 0,
                PassedExams = 0,
                AverageScore = 0
            };
        }

        summary.AverageScore = Math.Round(summary.AverageScore, 2);

        return summary;
    }

    public Task<StudentAnalyticsLatestExamData?> GetLatestExamAnalysisAsync(Guid userId)
    {
        return _db.exam_results
            .AsNoTracking()
            .Where(r => r.user_id == userId)
            .OrderByDescending(r => r.created_at)
            .Select(r => new StudentAnalyticsLatestExamData
            {
                StrengthPoints = r.strength_points,
                WeaknessPoints = r.weakness_points,
                LevelMessage = r.level_message
            })
            .FirstOrDefaultAsync();
    }

    public Task<string?> GetLatestAiRecommendationAsync(Guid userId)
    {
        return _db.ai_recommendations
            .AsNoTracking()
            .Where(a =>
                a.user_id == userId &&
                a.recommendation_text != null &&
                a.recommendation_text != "")
            .OrderByDescending(a => a.created_at)
            .Select(a => a.recommendation_text)
            .FirstOrDefaultAsync();
    }

    public Task<int> GetTotalLessonsByStreamAsync(string stream)
    {
        return _db.lessons
            .AsNoTracking()
            .Where(l =>
                l.subject != null &&
                l.subject.stream == stream)
            .CountAsync();
    }

    public Task<int> GetCompletedLessonsByStreamAsync(Guid userId, string stream)
    {
        return _db.lesson_progresses
            .AsNoTracking()
            .Where(lp =>
                lp.user_id == userId &&
                lp.completed == true &&
                lp.lesson_id != null)
            .Join(
                _db.lessons.AsNoTracking()
                    .Where(l =>
                        l.subject != null &&
                        l.subject.stream == stream),
                lp => lp.lesson_id!.Value,
                lesson => lesson.id,
                (lp, lesson) => lesson.id
            )
            .CountAsync();
    }

    public async Task<List<StudentAnalyticsSubjectScoreData>> GetSubjectScoresByStreamAsync(
        Guid userId,
        string stream
    )
    {
        var rawScores = await _db.exam_results
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
            .Where(x => x.Exam.subject_id != null)
            .Join(
                _db.subjects.AsNoTracking()
                    .Where(s => s.stream == stream),
                x => x.Exam.subject_id!.Value,
                subject => subject.id,
                (x, subject) => new
                {
                    SubjectId = subject.id,
                    SubjectName = subject.name,
                    Score = (double)(x.Result.score ?? 0)
                }
            )
            .GroupBy(x => new
            {
                x.SubjectId,
                x.SubjectName
            })
            .Select(g => new
            {
                g.Key.SubjectId,
                SubjectName = g.Key.SubjectName ?? "",
                AverageScore = g.Average(x => x.Score)
            })
            .ToListAsync();

        return rawScores
            .Select(x => new StudentAnalyticsSubjectScoreData
            {
                SubjectId = x.SubjectId,
                SubjectName = x.SubjectName,
                Score = Math.Round(x.AverageScore, 2)
            })
            .OrderByDescending(x => x.Score)
            .ToList();
    }

    public async Task<List<StudentAnalyticsExamScoreData>> GetSubjectExamAveragesAsync(Guid userId, string stream)
    {
        var rawSubjectScores = await _db.exam_results
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
            .Where(x => x.Exam.subject_id != null)
            .Join(
                _db.subjects.AsNoTracking()
                    .Where(s => s.stream == stream),
                x => x.Exam.subject_id!.Value,
                subject => subject.id,
                (x, subject) => new
                {
                    SubjectId = subject.id,
                    SubjectName = subject.name,
                    Score = (double)(x.Result.score ?? 0),
                    x.Result.created_at
                }
            )
            .GroupBy(x => new
            {
                x.SubjectId,
                x.SubjectName
            })
            .Select(g => new
            {
                SubjectName = g.Key.SubjectName ?? "",
                AverageScore = g.Average(x => x.Score),
                ExamsCount = g.Count(),
                LatestDate = g.Max(x => x.created_at)
            })
            .ToListAsync();

        return rawSubjectScores
            .Select(x => new StudentAnalyticsExamScoreData
            {
                Name = string.IsNullOrWhiteSpace(x.SubjectName) ? "المادة" : x.SubjectName,
                Date = x.LatestDate,
                Average = Math.Round(x.AverageScore, 2),
                SubjectsCount = x.ExamsCount,
                Level = GetPerformanceLevel(x.AverageScore)
            })
            .OrderByDescending(x => x.Average)
            .ToList();
    }

    public async Task<List<StudentAnalyticsSubjectDetailData>> GetSubjectDetailsAsync(Guid userId, string stream)
    {
        var rawDetails = await _db.exam_results
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
            .Where(x => x.Exam.subject_id != null)
            .Join(
                _db.subjects.AsNoTracking()
                    .Where(s => s.stream == stream),
                x => x.Exam.subject_id!.Value,
                subject => subject.id,
                (x, subject) => new
                {
                    SubjectId = subject.id,
                    SubjectName = subject.name,
                    Score = (double)(x.Result.score ?? 0)
                }
            )
            .GroupBy(x => new { x.SubjectId, x.SubjectName })
            .Select(g => new StudentAnalyticsSubjectDetailData
            {
                SubjectId = g.Key.SubjectId,
                SubjectName = g.Key.SubjectName ?? "",
                AverageScore = Math.Round(g.Average(x => x.Score), 2),
                ExamCount = g.Count()
            })
            .ToListAsync();

        return rawDetails
            .OrderByDescending(x => x.AverageScore)
            .ToList();
    }

    private static string GetPerformanceLevel(double score)
    {
        if (score >= 90) return "ممتاز";
        if (score >= 80) return "جيد جداً";
        if (score >= 70) return "جيد";
        if (score >= 60) return "مقبول";
        return "بحاجة لتحسين";
    }

    public async Task<List<StudentAnalyticsMonthlyProgressData>> GetMonthlyProgressAsync(Guid userId)
    {
        var rawMonthlyProgress = await _db.exam_results
            .AsNoTracking()
            .Where(r =>
                r.user_id == userId &&
                r.created_at != null)
            .GroupBy(r => new
            {
                r.created_at!.Value.Year,
                r.created_at!.Value.Month
            })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                AverageScore = g.Average(x => (double)(x.score ?? 0))
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .Take(12)
            .ToListAsync();

        return rawMonthlyProgress
            .Select(x => new StudentAnalyticsMonthlyProgressData
            {
                Year = x.Year,
                Month = x.Month,
                Average = Math.Round(x.AverageScore, 2)
            })
            .ToList();
    }
}
