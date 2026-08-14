using backend.Data.Generated;
using backend.Models.Generated;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Student;

public class StudentExamRepository : IStudentExamRepository
{
    private readonly AppDbContext _db;

    private static readonly string[] ShortTypes =
    {
        "short",
        "quick",
        "قصير"
    };

    private static readonly string[] ComprehensiveTypes =
    {
        "comprehensive",
        "full",
        "شامل"
    };

    public StudentExamRepository(AppDbContext db)
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

    public Task<List<StudentExamHistoryData>> GetExamHistoryAsync(Guid userId)
    {
        return _db.exam_results
            .AsNoTracking()
            .Where(r => r.user_id == userId)
            .OrderByDescending(r => r.created_at)
            .Select(r => new StudentExamHistoryData
            {
                ResultId = r.id,
                ExamId = r.exam_id,
                Score = r.score ?? 0,
                CreatedAt = r.created_at,
                ExamType = r.exam != null ? r.exam.type : "",
                SubjectId = r.exam != null ? r.exam.subject_id : null,
                SubjectName = r.exam != null && r.exam.subject != null ? r.exam.subject.name : "",
                LessonId = r.exam != null ? r.exam.lesson_id : null,
                LessonTitle = r.exam != null && r.exam.lesson != null ? r.exam.lesson.title : null,
                QuestionsCount = _db.questions.Count(q => q.exam_id == r.exam_id)
            })
            .ToListAsync();
    }

    public Task<List<StudentExamSubjectOptionData>> GetExamSubjectsAsync(string stream)
    {
        return _db.subjects
            .AsNoTracking()
            .Where(s => s.stream == stream)
            .Where(s => _db.exams.Any(e =>
                e.subject_id == s.id &&
                e.is_active == true &&
                _db.questions.Any(q => q.exam_id == e.id)))
            .OrderBy(s => s.name)
            .Select(s => new StudentExamSubjectOptionData
            {
                SubjectId = s.id,
                SubjectName = s.name ?? ""
            })
            .ToListAsync();
    }

    public async Task<List<StudentExamLessonOptionData>> GetQuickExamLessonsAsync(Guid subjectId)
    {
        var rawLessons = await _db.lessons
            .AsNoTracking()
            .Where(l => l.subject_id == subjectId)
            .Where(l => _db.exams.Any(e =>
                e.subject_id == subjectId &&
                e.lesson_id == l.id &&
                e.is_active == true &&
                ShortTypes.Contains(e.type) &&
                _db.questions.Any(q => q.exam_id == e.id)))
            .OrderBy(l => l.subject_unit != null ? l.subject_unit.order_number : int.MaxValue)
            .ThenBy(l => l.order_number ?? int.MaxValue)
            .ThenBy(l => l.title)
            .Select(l => new
            {
                l.id,
                l.title,
                LessonOrder = l.order_number ?? 0,
                UnitTitle = l.subject_unit != null ? l.subject_unit.title : "بدون وحدة"
            })
            .ToListAsync();

        return rawLessons
            .Select((lesson, index) => new StudentExamLessonOptionData
            {
                LessonId = lesson.id,
                LessonTitle = lesson.title,
                OrderNumber = lesson.LessonOrder,
                DisplayOrder = index + 1,
                UnitTitle = lesson.UnitTitle
            })
            .ToList();
    }

    public Task<subject?> GetSubjectByIdAndStreamAsync(Guid subjectId, string stream)
    {
        return _db.subjects
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.id == subjectId && s.stream == stream);
    }

    public Task<exam?> GetExamForStartAsync(Guid subjectId, string normalizedType, Guid? lessonId)
    {
        if (normalizedType == "short")
        {
            return _db.exams
                .AsNoTracking()
                .Include(e => e.lesson)
                .FirstOrDefaultAsync(e =>
                    e.subject_id == subjectId &&
                    e.lesson_id == lessonId &&
                    e.is_active == true &&
                    ShortTypes.Contains(e.type));
        }

        if (normalizedType == "comprehensive")
        {
            return _db.exams
                .AsNoTracking()
                .Include(e => e.lesson)
                .FirstOrDefaultAsync(e =>
                    e.subject_id == subjectId &&
                    e.lesson_id == null &&
                    e.is_active == true &&
                    ComprehensiveTypes.Contains(e.type));
        }

        return Task.FromResult<exam?>(null);
    }

    public Task<exam?> GetExamByIdAsync(Guid examId)
    {
        return _db.exams
            .AsNoTracking()
            .Include(e => e.subject)
            .FirstOrDefaultAsync(e => e.id == examId);
    }

    public Task<List<question>> GetQuestionsByExamIdAsync(Guid examId)
    {
        return _db.questions
            .AsNoTracking()
            .Where(q => q.exam_id == examId)
            .OrderBy(q => q.id)
            .ToListAsync();
    }

    public Task<exam_result?> GetExamResultByUserAsync(Guid userId, Guid resultId)
    {
        return _db.exam_results
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.id == resultId && r.user_id == userId);
    }

    public Task<List<exam_result_answer>> GetResultAnswersAsync(Guid resultId)
    {
        return _db.exam_result_answers
            .AsNoTracking()
            .Where(a => a.exam_result_id == resultId)
            .ToListAsync();
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

    public async Task<bool> DeleteExamResultAsync(Guid userId, Guid resultId)
    {
        var deletedCount = await _db.exam_results
            .Where(r => r.id == resultId && r.user_id == userId)
            .ExecuteDeleteAsync();

        return deletedCount > 0;
    }

    public Task<int> DeleteExamResultsAsync(Guid userId)
    {
        return _db.exam_results
            .Where(r => r.user_id == userId)
            .ExecuteDeleteAsync();
    }

    public void AddExamResult(exam_result result)
    {
        _db.exam_results.Add(result);
    }

    public void AddExamResultAnswer(exam_result_answer answer)
    {
        _db.exam_result_answers.Add(answer);
    }

    public void AddAiRecommendation(ai_recommendation recommendation)
    {
        _db.ai_recommendations.Add(recommendation);
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}
