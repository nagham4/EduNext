using backend.Data.Generated;
using backend.DTOs.Admin;
using backend.Models.Generated;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Admin;

public class AdminExamsRepository : IAdminExamsRepository
{
    private readonly AppDbContext _context;

    public AdminExamsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminExamsPageDto> GetExamsPageAsync(string? search)
    {
        var query = _context.exams
            .AsNoTracking()
            .Include(e => e.subject)
            .Include(e => e.lesson)
            .Include(e => e.questions)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();

            query = query.Where(e =>
                e.title.ToLower().Contains(normalizedSearch) ||
                (e.subject != null && e.subject.name.ToLower().Contains(normalizedSearch)) ||
                e.type.ToLower().Contains(normalizedSearch)
            );
        }

        var exams = await query
            .OrderByDescending(e => e.created_at)
            .Select(e => new AdminExamDto
            {
                Id = e.id,
                Title = e.title,
                SubjectId = e.subject_id,
                SubjectName = e.subject != null ? e.subject.name : "",
                LessonId = e.lesson_id,
                LessonTitle = e.lesson != null ? e.lesson.title : "",
                Type = e.type,
                TypeLabel = GetExamTypeLabel(e.type),
                TypeColor = GetExamTypeColor(e.type),
                QuestionCount = e.questions.Count,
                IsActive = e.is_active,
                Questions = e.questions.Select(q => new AdminExamQuestionDto
                {
                    Id = q.id,
                    Text = q.question_text,
                    Options = new List<string>
                    {
                        q.option_a ?? "",
                        q.option_b ?? "",
                        q.option_c ?? "",
                        q.option_d ?? ""
                    },
                    CorrectAnswer = ConvertCorrectAnswerToIndex(q.correct_answer),
                    SolutionText = q.solution_text ?? ""
                }).ToList()
            })
            .ToListAsync();

        return new AdminExamsPageDto
        {
            Stats = await GetStatsAsync(),
            Exams = exams,
            Subjects = await GetSubjectsAsync(),
            Lessons = await GetLessonsAsync()
        };
    }

    public async Task<AdminExamDto?> CreateExamAsync(CreateAdminExamDto dto)
    {
        var validationPassed = await ValidateExamAsync(
            dto.Title,
            dto.SubjectId,
            dto.LessonId,
            dto.Type,
            dto.Questions
        );

        if (!validationPassed)
            return null;

        var exam = new exam
        {
            id = Guid.NewGuid(),
            title = dto.Title.Trim(),
            subject_id = dto.SubjectId,
            lesson_id = dto.LessonId,
            type = NormalizeExamType(dto.Type),
            is_active = true,
            created_at = DateTime.Now
        };

        _context.exams.Add(exam);

        foreach (var questionDto in dto.Questions)
        {
            _context.questions.Add(MapQuestionDtoToEntity(questionDto, exam.id));
        }

        await _context.SaveChangesAsync();

        return await GetExamByIdAsync(exam.id);
    }

    public async Task<AdminExamDto?> UpdateExamAsync(Guid id, UpdateAdminExamDto dto)
    {
        var exam = await _context.exams
            .Include(e => e.questions)
            .FirstOrDefaultAsync(e => e.id == id);

        if (exam == null)
            return null;

        var validationPassed = await ValidateExamAsync(
            dto.Title,
            dto.SubjectId,
            dto.LessonId,
            dto.Type,
            dto.Questions
        );

        if (!validationPassed)
            return null;

        exam.title = dto.Title.Trim();
        exam.subject_id = dto.SubjectId;
        exam.lesson_id = dto.LessonId;
        exam.type = NormalizeExamType(dto.Type);

        _context.questions.RemoveRange(exam.questions);

        foreach (var questionDto in dto.Questions)
        {
            _context.questions.Add(MapQuestionDtoToEntity(questionDto, exam.id));
        }

        await _context.SaveChangesAsync();

        return await GetExamByIdAsync(exam.id);
    }

    public async Task<bool> DeleteExamAsync(Guid id)
    {
        var exam = await _context.exams
            .FirstOrDefaultAsync(e => e.id == id);

        if (exam == null)
            return false;

        _context.exams.Remove(exam);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<AdminExamDto?> GetExamByIdAsync(Guid id)
    {
        return await _context.exams
            .AsNoTracking()
            .Include(e => e.subject)
            .Include(e => e.lesson)
            .Include(e => e.questions)
            .Where(e => e.id == id)
            .Select(e => new AdminExamDto
            {
                Id = e.id,
                Title = e.title,
                SubjectId = e.subject_id,
                SubjectName = e.subject != null ? e.subject.name : "",
                LessonId = e.lesson_id,
                LessonTitle = e.lesson != null ? e.lesson.title : "",
                Type = e.type,
                TypeLabel = GetExamTypeLabel(e.type),
                TypeColor = GetExamTypeColor(e.type),
                QuestionCount = e.questions.Count,
                IsActive = e.is_active,
                Questions = e.questions.Select(q => new AdminExamQuestionDto
                {
                    Id = q.id,
                    Text = q.question_text,
                    Options = new List<string>
                    {
                        q.option_a ?? "",
                        q.option_b ?? "",
                        q.option_c ?? "",
                        q.option_d ?? ""
                    },
                    CorrectAnswer = ConvertCorrectAnswerToIndex(q.correct_answer),
                    SolutionText = q.solution_text ?? ""
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    private async Task<AdminExamsStatsDto> GetStatsAsync()
    {
        var totalExams = await _context.exams
            .AsNoTracking()
            .CountAsync();

        var activeExams = await _context.exams
            .AsNoTracking()
            .CountAsync(e => e.is_active);

        var scores = await _context.exam_results
            .AsNoTracking()
            .Where(r => r.score != null)
            .Select(r => r.score!.Value)
            .ToListAsync();

        var averageScore = scores.Count == 0
            ? 0
            : (int)Math.Round(scores.Average());

        var testedStudents = await _context.exam_results
            .AsNoTracking()
            .Where(r => r.user_id != null)
            .Select(r => r.user_id)
            .Distinct()
            .CountAsync();

        return new AdminExamsStatsDto
        {
            TotalExams = totalExams,
            ActiveExams = activeExams,
            AverageScore = averageScore,
            TestedStudents = testedStudents
        };
    }

    private async Task<List<AdminExamSubjectDto>> GetSubjectsAsync()
    {
        return await _context.subjects
            .AsNoTracking()
            .OrderBy(s => s.stream)
            .ThenBy(s => s.name)
            .Select(s => new AdminExamSubjectDto
            {
                Id = s.id,
                Name = s.name ?? "",
                Department = s.stream ?? ""
            })
            .ToListAsync();
    }

    private async Task<List<AdminExamLessonDto>> GetLessonsAsync()
    {
        return await _context.lessons
            .AsNoTracking()
            .OrderBy(l => l.subject != null ? l.subject.name : "")
            .ThenBy(l => l.order_number)
            .Select(l => new AdminExamLessonDto
            {
                Id = l.id,
                SubjectId = l.subject_id,
                Title = l.title ?? ""
            })
            .ToListAsync();
    }

    private async Task<bool> ValidateExamAsync(
        string title,
        Guid subjectId,
        Guid? lessonId,
        string type,
        List<AdminExamQuestionDto> questions
    )
    {
        if (string.IsNullOrWhiteSpace(title))
            return false;

        if (subjectId == Guid.Empty)
            return false;

        var normalizedType = NormalizeExamType(type);

        if (normalizedType != "short" && normalizedType != "comprehensive")
            return false;

        var subjectExists = await _context.subjects
            .AsNoTracking()
            .AnyAsync(s => s.id == subjectId);

        if (!subjectExists)
            return false;

        if (lessonId.HasValue)
        {
            var lessonBelongsToSubject = await _context.lessons
                .AsNoTracking()
                .AnyAsync(l => l.id == lessonId.Value && l.subject_id == subjectId);

            if (!lessonBelongsToSubject)
                return false;
        }

        if (questions == null || questions.Count == 0)
            return false;

        foreach (var question in questions)
        {
            if (string.IsNullOrWhiteSpace(question.Text))
                return false;

            if (question.Options == null || question.Options.Count < 4)
                return false;

            if (question.Options.Take(4).Any(string.IsNullOrWhiteSpace))
                return false;

            if (question.CorrectAnswer < 0 || question.CorrectAnswer > 3)
                return false;
        }

        return true;
    }

    private static question MapQuestionDtoToEntity(AdminExamQuestionDto dto, Guid examId)
    {
        var options = dto.Options ?? new List<string>();

        return new question
        {
            id = Guid.NewGuid(),
            exam_id = examId,
            question_text = dto.Text.Trim(),
            option_a = options.ElementAtOrDefault(0)?.Trim(),
            option_b = options.ElementAtOrDefault(1)?.Trim(),
            option_c = options.ElementAtOrDefault(2)?.Trim(),
            option_d = options.ElementAtOrDefault(3)?.Trim(),
            correct_answer = ConvertIndexToCorrectAnswer(dto.CorrectAnswer),
            solution_text = string.IsNullOrWhiteSpace(dto.SolutionText)
                ? null
                : dto.SolutionText.Trim()
        };
    }

    private static string NormalizeExamType(string type)
    {
        return type switch
        {
            "شامل" => "comprehensive",
            "اختبار قصير" => "short",
            "comprehensive" => "comprehensive",
            "short" => "short",
            _ => "short"
        };
    }

    private static string GetExamTypeLabel(string type)
    {
        return type == "comprehensive" ? "شامل" : "اختبار قصير";
    }

    private static string GetExamTypeColor(string type)
    {
        return type == "comprehensive" ? "#2563eb" : "#d97706";
    }

    private static string ConvertIndexToCorrectAnswer(int index)
    {
        return index switch
        {
            0 => "A",
            1 => "B",
            2 => "C",
            3 => "D",
            _ => "A"
        };
    }

    private static int ConvertCorrectAnswerToIndex(string? answer)
    {
        return answer switch
        {
            "A" => 0,
            "B" => 1,
            "C" => 2,
            "D" => 3,
            _ => 0
        };
    }
}