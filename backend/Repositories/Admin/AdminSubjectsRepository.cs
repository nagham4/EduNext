using backend.Data.Generated;
using backend.DTOs.Admin;
using backend.Models.Generated;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Admin;

public class AdminSubjectsRepository : IAdminSubjectsRepository
{
    private readonly AppDbContext _context;

    public AdminSubjectsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminSubjectsPageDto> GetSubjectsPageAsync(
        string? search,
        string? department,
        string? sortBy,
        int page,
        int pageSize
    )
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 4 : pageSize;

        var subjectsQuery = _context.subjects
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();

            subjectsQuery = subjectsQuery.Where(s =>
                s.name.ToLower().Contains(normalizedSearch) ||
                (s.stream != null && s.stream.ToLower().Contains(normalizedSearch))
            );
        }

        if (!string.IsNullOrWhiteSpace(department) && department != "الكل")
        {
            subjectsQuery = subjectsQuery.Where(s => s.stream == department);
        }

        var subjectRows = await subjectsQuery
            .Select(s => new
            {
                s.id,
                s.name,
                s.stream
            })
            .ToListAsync();

        var subjectIds = subjectRows
            .Select(s => s.id)
            .ToList();

        var lessonsCounts = await _context.lessons
            .AsNoTracking()
            .Where(l => l.subject_id != null && subjectIds.Contains(l.subject_id.Value))
            .GroupBy(l => l.subject_id)
            .Select(g => new
            {
                SubjectId = g.Key!.Value,
                Count = g.Count()
            })
            .ToListAsync();

        var examsCounts = await _context.exams
            .AsNoTracking()
            .Where(e => e.subject_id != null && subjectIds.Contains(e.subject_id.Value))
            .GroupBy(e => e.subject_id)
            .Select(g => new
            {
                SubjectId = g.Key!.Value,
                Count = g.Count()
            })
            .ToListAsync();

        var allSubjects = subjectRows
            .Select(s => new AdminSubjectDto
            {
                Id = s.id,
                Name = s.name ?? "",
                Abbr = GenerateSubjectAbbr(s.name ?? ""),
                Department = s.stream ?? "",
                LessonsCount = lessonsCounts.FirstOrDefault(x => x.SubjectId == s.id)?.Count ?? 0,
                ExamsCount = examsCounts.FirstOrDefault(x => x.SubjectId == s.id)?.Count ?? 0
            })
            .ToList();

        allSubjects = sortBy switch
        {
            "name" => allSubjects.OrderBy(s => s.Name).ToList(),
            "lessons" => allSubjects.OrderByDescending(s => s.LessonsCount).ToList(),
            "exams" => allSubjects.OrderByDescending(s => s.ExamsCount).ToList(),
            _ => allSubjects.OrderBy(s => s.Name).ToList()
        };

        var totalItems = allSubjects.Count;

        var totalPages = totalItems == 0
            ? 1
            : (int)Math.Ceiling((double)totalItems / pageSize);

        if (page > totalPages)
        {
            page = totalPages;
        }

        var pagedSubjects = allSubjects
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var stats = await GetStatsAsync();
        var departments = await GetDepartmentsAsync();

        return new AdminSubjectsPageDto
        {
            Stats = stats,
            Subjects = pagedSubjects,
            Departments = departments,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalItems = totalItems,
            PageSize = pageSize
        };
    }

    public async Task<List<string>> GetDepartmentsAsync()
    {
        return await _context.subjects
            .AsNoTracking()
            .Where(s => s.stream != null && s.stream != "")
            .Select(s => s.stream!)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
    }

    public async Task<List<AdminSubjectLessonDto>> GetSubjectLessonsAsync(Guid subjectId)
    {
        var lessons = await _context.lessons
            .AsNoTracking()
            .Where(l => l.subject_id == subjectId)
            .Select(l => new
            {
                LessonId = l.id,
                LessonTitle = l.title,
                LessonOrder = l.order_number,
                SubjectUnitId = l.subject_unit_id
            })
            .ToListAsync();

        if (lessons.Count == 0)
        {
            return new List<AdminSubjectLessonDto>();
        }

        var unitIds = lessons
            .Where(l => l.SubjectUnitId != null)
            .Select(l => l.SubjectUnitId!.Value)
            .Distinct()
            .ToList();

        var units = await _context.subject_units
            .AsNoTracking()
            .Where(u => unitIds.Contains(u.id))
            .Select(u => new
            {
                UnitId = u.id,
                UnitTitle = u.title,
                UnitOrder = u.order_number
            })
            .ToListAsync();

        var unitById = units.ToDictionary(u => u.UnitId);

        return lessons
            .Select(lesson =>
            {
                var unit = lesson.SubjectUnitId != null &&
                           unitById.ContainsKey(lesson.SubjectUnitId.Value)
                    ? unitById[lesson.SubjectUnitId.Value]
                    : null;

                return new AdminSubjectLessonDto
                {
                    Id = lesson.LessonId,
                    Title = lesson.LessonTitle ?? "",
                    Order = lesson.LessonOrder ?? 0,
                    UnitOrder = unit?.UnitOrder,
                    UnitTitle = unit?.UnitTitle
                };
            })
            .OrderBy(x => x.UnitOrder ?? int.MaxValue)
            .ThenBy(x => x.Order)
            .ThenBy(x => x.Title)
            .ToList();
    }

    public async Task<AdminSubjectDto?> CreateSubjectAsync(CreateAdminSubjectDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(dto.Department))
        {
            return null;
        }

        var subjectName = dto.Name.Trim();
        var department = dto.Department.Trim();

        var exists = await _context.subjects
            .AsNoTracking()
            .AnyAsync(s => s.name.ToLower() == subjectName.ToLower());

        if (exists)
        {
            return null;
        }

        var newSubject = new subject
        {
            id = Guid.NewGuid(),
            name = subjectName,
            stream = department,
            created_at = DateTime.Now
        };

        _context.subjects.Add(newSubject);
        await _context.SaveChangesAsync();

        return new AdminSubjectDto
        {
            Id = newSubject.id,
            Name = newSubject.name,
            Abbr = GenerateSubjectAbbr(newSubject.name),
            Department = newSubject.stream ?? "",
            LessonsCount = 0,
            ExamsCount = 0
        };
    }

    public async Task<AdminSubjectDto?> UpdateSubjectAsync(Guid id, UpdateAdminSubjectDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(dto.Department))
        {
            return null;
        }

        var subject = await _context.subjects
            .FirstOrDefaultAsync(s => s.id == id);

        if (subject == null)
        {
            return null;
        }

        var subjectName = dto.Name.Trim();
        var department = dto.Department.Trim();

        var duplicateExists = await _context.subjects
            .AsNoTracking()
            .AnyAsync(s =>
                s.id != id &&
                s.name.ToLower() == subjectName.ToLower()
            );

        if (duplicateExists)
        {
            return null;
        }

        subject.name = subjectName;
        subject.stream = department;

        await _context.SaveChangesAsync();

        var lessonsCount = await _context.lessons
            .AsNoTracking()
            .CountAsync(l => l.subject_id == id);

        var examsCount = await _context.exams
            .AsNoTracking()
            .CountAsync(e => e.subject_id == id);

        return new AdminSubjectDto
        {
            Id = subject.id,
            Name = subject.name,
            Abbr = GenerateSubjectAbbr(subject.name),
            Department = subject.stream ?? "",
            LessonsCount = lessonsCount,
            ExamsCount = examsCount
        };
    }

    public async Task<bool> DeleteSubjectAsync(Guid id)
    {
        var subject = await _context.subjects
            .FirstOrDefaultAsync(s => s.id == id);

        if (subject == null)
        {
            return false;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var lessonIds = await _context.lessons
            .Where(l => l.subject_id == id)
            .Select(l => l.id)
            .ToListAsync();

        var examIds = await _context.exams
            .Where(e =>
                e.subject_id == id ||
                (e.lesson_id != null && lessonIds.Contains(e.lesson_id.Value)))
            .Select(e => e.id)
            .ToListAsync();

        var questionIds = await _context.questions
            .Where(q => q.exam_id != null && examIds.Contains(q.exam_id.Value))
            .Select(q => q.id)
            .ToListAsync();

        var resultIds = await _context.exam_results
            .Where(r => r.exam_id != null && examIds.Contains(r.exam_id.Value))
            .Select(r => r.id)
            .ToListAsync();

        if (resultIds.Count > 0 || questionIds.Count > 0)
        {
            await _context.exam_result_answers
                .Where(a =>
                    (a.exam_result_id != null && resultIds.Contains(a.exam_result_id.Value)) ||
                    (a.question_id != null && questionIds.Contains(a.question_id.Value)))
                .ExecuteDeleteAsync();
        }

        if (resultIds.Count > 0)
        {
            await _context.exam_results
                .Where(r => resultIds.Contains(r.id))
                .ExecuteDeleteAsync();
        }

        if (questionIds.Count > 0)
        {
            await _context.questions
                .Where(q => questionIds.Contains(q.id))
                .ExecuteDeleteAsync();
        }

        if (examIds.Count > 0)
        {
            await _context.exams
                .Where(e => examIds.Contains(e.id))
                .ExecuteDeleteAsync();
        }

        if (lessonIds.Count > 0)
        {
            await _context.lesson_progresses
                .Where(lp => lp.lesson_id != null && lessonIds.Contains(lp.lesson_id.Value))
                .ExecuteDeleteAsync();

            await _context.study_plan_items
                .Where(i => lessonIds.Contains(i.lesson_id))
                .ExecuteDeleteAsync();

            await _context.study_sessions
                .Where(s => s.lesson_id != null && lessonIds.Contains(s.lesson_id.Value))
                .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.lesson_id, (Guid?)null));

            await _context.lessons
                .Where(l => lessonIds.Contains(l.id))
                .ExecuteDeleteAsync();
        }

        await _context.study_plans
            .Where(p => p.subject_id == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.subject_id, (Guid?)null));

        await _context.study_sessions
            .Where(s => s.subject_id == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.subject_id, (Guid?)null));

        await _context.subject_analyses
            .Where(a => a.subject_id == id)
            .ExecuteDeleteAsync();

        await _context.student_profile_subjects
            .Where(s => s.subject_id == id)
            .ExecuteDeleteAsync();

        await _context.student_preference_difficult_subjects
            .Where(s => s.subject_id == id)
            .ExecuteDeleteAsync();

        await _context.subject_units
            .Where(u => u.subject_id == id)
            .ExecuteDeleteAsync();

        _context.subjects.Remove(subject);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return true;
    }

    private async Task<AdminSubjectsStatsDto> GetStatsAsync()
    {
        var startOfMonth = GetStartOfCurrentMonth();

        var totalSubjects = await _context.subjects
            .AsNoTracking()
            .CountAsync();

        var totalLessons = await _context.lessons
            .AsNoTracking()
            .CountAsync();

        var totalExams = await _context.exams
            .AsNoTracking()
            .CountAsync();

        var newSubjectsThisMonth = await _context.subjects
            .AsNoTracking()
            .CountAsync(s => s.created_at != null && s.created_at >= startOfMonth);

        return new AdminSubjectsStatsDto
        {
            TotalSubjects = totalSubjects,
            TotalLessons = totalLessons,
            TotalExams = totalExams,
            NewSubjectsThisMonth = newSubjectsThisMonth
        };
    }

    private static DateTime GetStartOfCurrentMonth()
    {
        var now = DateTime.Now;
        return new DateTime(now.Year, now.Month, 1);
    }

    private static string GenerateSubjectAbbr(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "--";
        }

        var words = name
            .Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word =>
            {
                if (word.StartsWith("ال") && word.Length > 2)
                {
                    return word[2..];
                }

                return word;
            })
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .ToList();

        if (words.Count == 0)
        {
            return "--";
        }

        if (words.Count == 1)
        {
            return words[0][0].ToString();
        }

        return string.Join(" ", words.Take(2).Select(word => word[0]));
    }
}
