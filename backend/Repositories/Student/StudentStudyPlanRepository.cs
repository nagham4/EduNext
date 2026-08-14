using backend.Data.Generated;
using backend.Models.Generated;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Student;

public class StudentStudyPlanRepository : IStudentStudyPlanRepository
{
    private readonly AppDbContext _db;

    public StudentStudyPlanRepository(AppDbContext db)
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

    public Task<List<study_plan>> GetPlansByUserAsync(Guid userId)
    {
        return _db.study_plans
            .AsNoTracking()
            .Include(sp => sp.subject)
            .Include(sp => sp.study_plan_items)
                .ThenInclude(i => i.lesson)
            .Where(sp => sp.user_id == userId)
            .OrderByDescending(sp => sp.created_at)
            .ToListAsync();
    }

    public Task<study_plan?> GetPlanByUserAsync(Guid userId, Guid planId)
    {
        return _db.study_plans
            .AsNoTracking()
            .Include(sp => sp.subject)
            .Include(sp => sp.study_plan_items)
                .ThenInclude(i => i.lesson)
            .FirstOrDefaultAsync(sp => sp.id == planId && sp.user_id == userId);
    }

    public Task<study_plan?> GetPlanForUpdateAsync(Guid userId, Guid planId)
    {
        return _db.study_plans
            .Include(sp => sp.study_plan_items)
            .FirstOrDefaultAsync(sp => sp.id == planId && sp.user_id == userId);
    }

    public Task<subject?> GetSubjectByIdAndStreamAsync(Guid subjectId, string stream)
    {
        return _db.subjects
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.id == subjectId && s.stream == stream);
    }

    public Task<List<StudyPlanSubjectOptionData>> GetSubjectsByStreamAsync(string stream)
    {
        return _db.subjects
            .AsNoTracking()
            .Where(s => s.stream == stream)
            .Where(s => _db.lessons.Any(l => l.subject_id == s.id))
            .OrderBy(s => s.name)
            .Select(s => new StudyPlanSubjectOptionData
            {
                SubjectId = s.id,
                SubjectName = s.name
            })
            .ToListAsync();
    }

    public Task<StudentStudyPlanProfileData?> GetProfileAsync(Guid userId)
    {
        return _db.student_profiles
            .AsNoTracking()
            .Where(p => p.user_id == userId)
            .Select(p => new StudentStudyPlanProfileData
            {
                Stream = p.stream ?? "",
                CurrentLevel = p.current_grade ?? "",
                Goal = p.primary_goal ?? "",
                StudyHours = p.preferred_study_time ?? "",
                ExamExperience = p.exam_experience ?? "",
                LearningMethods = p.learning_methods,
                DifficultSubjects = p.student_profile_subjects
                    .Where(x => x.subject != null)
                    .Select(x => x.subject!.name)
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<StudentStudyPlanSubjectProgressData>> GetSubjectProgressAsync(Guid userId, string stream)
    {
        var subjects = await _db.subjects
            .AsNoTracking()
            .Where(s => s.stream == stream)
            .Select(s => new
            {
                s.id,
                SubjectName = s.name ?? ""
            })
            .ToListAsync();

        var subjectIds = subjects.Select(s => s.id).ToList();

        var lessons = await _db.lessons
            .AsNoTracking()
            .Where(l => l.subject_id != null && subjectIds.Contains(l.subject_id.Value))
            .OrderBy(l => l.subject_id)
            .ThenBy(l => l.subject_unit != null ? l.subject_unit.order_number : int.MaxValue)
            .ThenBy(l => l.order_number ?? int.MaxValue)
            .ThenBy(l => l.title)
            .Select(l => new
            StudentStudyPlanLessonProgressData
            {
                LessonId = l.id,
                SubjectId = l.subject_id!.Value,
                LessonTitle = l.title
            })
            .ToListAsync();

        var completedLessonIds = await _db.lesson_progresses
            .AsNoTracking()
            .Where(lp =>
                lp.user_id == userId &&
                lp.lesson_id != null &&
                lp.completed == true)
            .Select(lp => lp.lesson_id!.Value)
            .ToListAsync();

        var completedSet = completedLessonIds.ToHashSet();

        var scores = await _db.exam_results
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
            .Select(g => new
            {
                SubjectId = g.Key,
                AverageScore = g.Average(x => x.Score)
            })
            .ToDictionaryAsync(x => x.SubjectId, x => Math.Round(x.AverageScore, 2));

        var lessonsBySubject = lessons
            .GroupBy(l => l.SubjectId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return subjects.Select(subject =>
        {
            var subjectLessons = lessonsBySubject.TryGetValue(subject.id, out var foundLessons)
                ? foundLessons
                : new List<StudentStudyPlanLessonProgressData>();

            var nextLesson = subjectLessons.FirstOrDefault(l => !completedSet.Contains(l.LessonId));
            var completedLessons = subjectLessons.Count(l => completedSet.Contains(l.LessonId));
            scores.TryGetValue(subject.id, out var averageScore);

            return new StudentStudyPlanSubjectProgressData
            {
                SubjectId = subject.id,
                SubjectName = subject.SubjectName,
                TotalLessons = subjectLessons.Count,
                CompletedLessons = completedLessons,
                AverageScore = averageScore,
                NextLessonId = nextLesson?.LessonId,
                NextLessonTitle = nextLesson?.LessonTitle
            };
        }).ToList();
    }

    public async Task<List<StudyPlanLessonOptionData>> GetLessonsBySubjectAsync(Guid subjectId)
    {
        var rawLessons = await _db.lessons
            .AsNoTracking()
            .Where(l => l.subject_id == subjectId)
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
            .Select((lesson, index) => new StudyPlanLessonOptionData
            {
                LessonId = lesson.id,
                LessonTitle = lesson.title,
                OrderNumber = lesson.LessonOrder,
                DisplayOrder = index + 1,
                UnitTitle = lesson.UnitTitle
            })
            .ToList();
    }

    public Task<List<lesson>> GetValidLessonsAsync(Guid subjectId, List<Guid> lessonIds)
    {
        return _db.lessons
            .AsNoTracking()
            .Where(l => lessonIds.Contains(l.id) && l.subject_id == subjectId)
            .OrderBy(l => l.subject_unit_id)
            .ThenBy(l => l.order_number ?? int.MaxValue)
            .ThenBy(l => l.title)
            .ToListAsync();
    }

    public void AddPlan(study_plan plan)
    {
        _db.study_plans.Add(plan);
    }

    public void AddPlanItem(study_plan_item item)
    {
        _db.study_plan_items.Add(item);
    }

    public void RemovePlanItems(IEnumerable<study_plan_item> items)
    {
        _db.study_plan_items.RemoveRange(items);
    }

    public void RemovePlan(study_plan plan)
    {
        _db.study_plans.Remove(plan);
    }

    public Task<study_plan?> GetPlanWithDetailsAsync(Guid planId)
    {
        return _db.study_plans
            .AsNoTracking()
            .Include(sp => sp.subject)
            .Include(sp => sp.study_plan_items)
                .ThenInclude(i => i.lesson)
            .FirstOrDefaultAsync(sp => sp.id == planId);
    }

    public Task<study_plan_item?> GetPlanItemForUpdateAsync(Guid userId, Guid planId, Guid itemId)
    {
        return _db.study_plan_items
            .Include(i => i.study_plan)
            .FirstOrDefaultAsync(i =>
                i.id == itemId &&
                i.study_plan_id == planId &&
                i.study_plan.user_id == userId);
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}
