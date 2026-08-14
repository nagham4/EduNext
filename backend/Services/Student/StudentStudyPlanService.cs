using backend.DTOs.Student;
using backend.DTOs.AI;
using backend.Models.Generated;
using backend.Repositories.Student;
using backend.Services.AI;

namespace backend.Services.Student;

public class StudentStudyPlanService : IStudentStudyPlanService
{
    private readonly IStudentStudyPlanRepository _repository;
    private readonly IAiInsightsService _ai;

    public StudentStudyPlanService(IStudentStudyPlanRepository repository, IAiInsightsService ai)
    {
        _repository = repository;
        _ai = ai;
    }

    public async Task<List<StudyPlanSubjectOptionDto>> GetSubjectsAsync(Guid userId)
    {
        var stream = await _repository.GetStudentStreamAsync(userId);

        if (string.IsNullOrWhiteSpace(stream))
        {
            return new List<StudyPlanSubjectOptionDto>();
        }

        var subjects = await _repository.GetSubjectsByStreamAsync(stream);

        return subjects.Select(s => new StudyPlanSubjectOptionDto
        {
            SubjectId = s.SubjectId,
            SubjectName = s.SubjectName
        }).ToList();
    }

    public async Task<List<StudyPlanLessonOptionDto>> GetLessonsAsync(Guid userId, Guid subjectId)
    {
        var stream = await _repository.GetStudentStreamAsync(userId);

        if (string.IsNullOrWhiteSpace(stream))
        {
            return new List<StudyPlanLessonOptionDto>();
        }

        var subject = await _repository.GetSubjectByIdAndStreamAsync(subjectId, stream);

        if (subject == null)
        {
            return new List<StudyPlanLessonOptionDto>();
        }

        var lessons = await _repository.GetLessonsBySubjectAsync(subjectId);

        return lessons.Select(l => new StudyPlanLessonOptionDto
        {
            LessonId = l.LessonId,
            LessonTitle = l.LessonTitle,
            OrderNumber = l.OrderNumber,
            DisplayOrder = l.DisplayOrder,
            UnitTitle = l.UnitTitle
        }).ToList();
    }

    public async Task<List<StudyPlanDto>> GetMyPlansAsync(Guid userId)
    {
        var plans = await _repository.GetPlansByUserAsync(userId);

        return plans.Select(MapToDto).ToList();
    }

    public async Task<StudyPlanSuggestionDto> GetAiSuggestionAsync(Guid userId)
    {
        var profile = await _repository.GetProfileAsync(userId);

        if (profile == null || string.IsNullOrWhiteSpace(profile.Stream))
        {
            return new StudyPlanSuggestionDto
            {
                RecommendationText = "أكمل التهيئة أولاً حتى نتمكن من اقتراح خطة دراسية مناسبة."
            };
        }

        var subjectProgress = await _repository.GetSubjectProgressAsync(userId, profile.Stream);
        var totalLessons = subjectProgress.Sum(s => s.TotalLessons);
        var completedLessons = subjectProgress.Sum(s => s.CompletedLessons);
        var averageScore = subjectProgress
            .Where(s => s.AverageScore > 0)
            .Select(s => s.AverageScore)
            .DefaultIfEmpty(0)
            .Average();

        var aiResponse = await _ai.GeneratePersonalizedRecommendationAsync(new AiPersonalizedRecommendationRequestDto
        {
            ContextType = "study-plan",
            Stream = profile.Stream,
            CurrentLevel = profile.CurrentLevel,
            Goal = profile.Goal,
            StudyHours = profile.StudyHours,
            ExamExperience = profile.ExamExperience,
            LearningMethods = profile.LearningMethods,
            DifficultSubjects = profile.DifficultSubjects,
            AverageScore = Math.Round(averageScore, 2),
            CompletedLessons = completedLessons,
            TotalLessons = totalLessons,
            Subjects = subjectProgress.Select(s => new AiSubjectProgressDto
            {
                SubjectId = s.SubjectId,
                SubjectName = s.SubjectName,
                ProgressPercent = s.TotalLessons == 0
                    ? 0
                    : Math.Round(s.CompletedLessons * 100.0 / s.TotalLessons, 2),
                AverageScore = s.AverageScore,
                CompletedLessons = s.CompletedLessons,
                TotalLessons = s.TotalLessons,
                RemainingLessons = Math.Max(0, s.TotalLessons - s.CompletedLessons),
                NextLessonId = s.NextLessonId,
                NextLessonTitle = s.NextLessonTitle
            }).ToList()
        });

        var orderedSubjectProgress = subjectProgress
            .OrderBy(s => s.AverageScore <= 0 ? 100 : s.AverageScore)
            .ThenBy(s => s.TotalLessons == 0 ? 100 : s.CompletedLessons * 100.0 / s.TotalLessons)
            .ToList();

        var focusSubjects = orderedSubjectProgress
            .Where(s => aiResponse.FocusSubjects.Any(name => SubjectNamesMatch(name, s.SubjectName)))
            .ToList();

        if (focusSubjects.Count == 0)
        {
            focusSubjects = orderedSubjectProgress
                .Where(s =>
                    s.AverageScore is > 0 and < 70 ||
                    s.TotalLessons == 0 ||
                    s.CompletedLessons * 100.0 / Math.Max(1, s.TotalLessons) < 60 ||
                    s.TotalLessons > s.CompletedLessons)
                .Take(3)
                .ToList();
        }

        if (focusSubjects.Count == 0)
        {
            focusSubjects = orderedSubjectProgress.Take(2).ToList();
        }

        var suggestedSubjects = new List<StudyPlanSuggestedSubjectDto>();

        foreach (var subject in focusSubjects)
        {
            var subjectLessons = await _repository.GetLessonsBySubjectAsync(subject.SubjectId);
            var lessonIds = PickSuggestedLessonIds(subject, subjectLessons, aiResponse.LessonOrder);

            if (lessonIds.Count == 0)
            {
                continue;
            }

            var lessonTitlesById = subjectLessons.ToDictionary(l => l.LessonId, l => l.LessonTitle);

            suggestedSubjects.Add(new StudyPlanSuggestedSubjectDto
            {
                SubjectId = subject.SubjectId,
                SubjectName = subject.SubjectName,
                LessonIds = lessonIds,
                LessonOrder = lessonIds
                    .Where(id => lessonTitlesById.ContainsKey(id))
                    .Select(id => lessonTitlesById[id])
                    .ToList()
            });
        }

        var primarySuggestion = suggestedSubjects.FirstOrDefault();
        var verifiedFocusSubjects = suggestedSubjects
            .Select(s => s.SubjectName)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToList();
        var verifiedLessonOrder = suggestedSubjects
            .SelectMany(s => s.LessonOrder.Select(lesson => $"{s.SubjectName}: {lesson}"))
            .ToList();

        return new StudyPlanSuggestionDto
        {
            RecommendationText = aiResponse.RecommendationText,
            FocusSubjects = verifiedFocusSubjects,
            WeeklyStudyHours = NormalizeWeeklyStudyHours(aiResponse.WeeklyStudyHours, profile.StudyHours),
            LessonOrder = verifiedLessonOrder,
            SubjectId = primarySuggestion?.SubjectId,
            LessonIds = primarySuggestion?.LessonIds ?? new List<Guid>(),
            SuggestedSubjects = suggestedSubjects
        };
    }

    public async Task<StudyPlanDto?> GetByIdAsync(Guid userId, Guid id)
    {
        var plan = await _repository.GetPlanByUserAsync(userId, id);

        return plan == null ? null : MapToDto(plan);
    }

    public async Task<StudyPlanDto> CreateAsync(Guid userId, CreateStudyPlanDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var subjectId = ValidateSubjectId(dto.SubjectId);
        var lessonIds = ValidateLessonIds(dto.LessonIds);

        var stream = await _repository.GetStudentStreamAsync(userId);

        if (string.IsNullOrWhiteSpace(stream))
        {
            throw new ArgumentException("لم يتم العثور على فرع الطالب.");
        }

        var subject = await _repository.GetSubjectByIdAndStreamAsync(subjectId, stream);

        if (subject == null)
        {
            throw new ArgumentException("المادة غير موجودة أو لا تنتمي لفرع الطالب.");
        }

        var validLessons = await _repository.GetValidLessonsAsync(subjectId, lessonIds);

        if (validLessons.Count != lessonIds.Count)
        {
            throw new ArgumentException("بعض الدروس لا تنتمي للمادة المختارة.");
        }

        validLessons = SortLessonsByRequestedOrder(validLessons, lessonIds);

        var title = string.IsNullOrWhiteSpace(dto.Title)
            ? $"خطة - {subject.name}"
            : dto.Title.Trim();

        var now = GetUnspecifiedNow();

        var plan = new study_plan
        {
            id = Guid.NewGuid(),
            user_id = userId,
            subject_id = subjectId,
            title = title,
            description = dto.Description?.Trim(),
            is_ai_generated = dto.IsAiGenerated,
            study_days = CleanStudyDays(dto.StudyDays),
            daily_duration_minutes = dto.DailyDurationMinutes,
            created_at = now,
            updated_at = now
        };

        _repository.AddPlan(plan);

        var order = 1;

        foreach (var lesson in validLessons)
        {
            _repository.AddPlanItem(new study_plan_item
            {
                id = Guid.NewGuid(),
                study_plan_id = plan.id,
                lesson_id = lesson.id,
                order_number = order++,
                is_completed = false,
                created_at = now
            });
        }

        await _repository.SaveChangesAsync();

        var created = await _repository.GetPlanWithDetailsAsync(plan.id);

        if (created == null)
        {
            throw new InvalidOperationException("تم إنشاء الخطة ولكن تعذر تحميلها.");
        }

        return MapToDto(created);
    }

    public async Task<StudyPlanDto?> UpdateAsync(Guid userId, Guid id, UpdateStudyPlanDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var plan = await _repository.GetPlanForUpdateAsync(userId, id);

        if (plan == null)
        {
            return null;
        }

        var subjectId = ValidateSubjectId(dto.SubjectId);
        var lessonIds = ValidateLessonIds(dto.LessonIds);

        var stream = await _repository.GetStudentStreamAsync(userId);

        if (string.IsNullOrWhiteSpace(stream))
        {
            throw new ArgumentException("لم يتم العثور على فرع الطالب.");
        }

        var subject = await _repository.GetSubjectByIdAndStreamAsync(subjectId, stream);

        if (subject == null)
        {
            throw new ArgumentException("المادة غير موجودة أو لا تنتمي لفرع الطالب.");
        }

        var validLessons = await _repository.GetValidLessonsAsync(subjectId, lessonIds);

        if (validLessons.Count != lessonIds.Count)
        {
            throw new ArgumentException("بعض الدروس لا تنتمي للمادة المختارة.");
        }

        validLessons = SortLessonsByRequestedOrder(validLessons, lessonIds);

        var now = GetUnspecifiedNow();

        plan.subject_id = subjectId;
        plan.title = string.IsNullOrWhiteSpace(dto.Title)
            ? $"خطة - {subject.name}"
            : dto.Title.Trim();
        plan.description = dto.Description?.Trim();
        plan.is_ai_generated = dto.IsAiGenerated;
        plan.study_days = CleanStudyDays(dto.StudyDays);
        plan.daily_duration_minutes = dto.DailyDurationMinutes;
        plan.updated_at = now;

        _repository.RemovePlanItems(plan.study_plan_items);

        var order = 1;

        foreach (var lesson in validLessons)
        {
            _repository.AddPlanItem(new study_plan_item
            {
                id = Guid.NewGuid(),
                study_plan_id = plan.id,
                lesson_id = lesson.id,
                order_number = order++,
                is_completed = false,
                created_at = now
            });
        }

        await _repository.SaveChangesAsync();

        var updated = await _repository.GetPlanWithDetailsAsync(plan.id);

        return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid id)
    {
        var plan = await _repository.GetPlanForUpdateAsync(userId, id);

        if (plan == null)
        {
            return false;
        }

        _repository.RemovePlan(plan);

        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateItemCompletionAsync(Guid userId, Guid planId, Guid itemId, bool isCompleted)
    {
        var item = await _repository.GetPlanItemForUpdateAsync(userId, planId, itemId);

        if (item == null)
        {
            return false;
        }

        item.is_completed = isCompleted;

        if (item.study_plan != null)
        {
            item.study_plan.updated_at = GetUnspecifiedNow();
        }

        await _repository.SaveChangesAsync();

        return true;
    }

    private static Guid ValidateSubjectId(Guid? subjectId)
    {
        if (subjectId == null || subjectId == Guid.Empty)
        {
            throw new ArgumentException("يجب اختيار المادة.");
        }

        return subjectId.Value;
    }

    private static List<Guid> ValidateLessonIds(List<Guid>? lessonIds)
    {
        var cleanLessonIds = lessonIds?
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList() ?? new List<Guid>();

        if (cleanLessonIds.Count == 0)
        {
            throw new ArgumentException("يجب اختيار درس واحد على الأقل.");
        }

        return cleanLessonIds;
    }

    private static List<string> CleanStudyDays(List<string>? days)
    {
        return days?
            .Where(day => !string.IsNullOrWhiteSpace(day))
            .Select(day => day.Trim())
            .Distinct()
            .ToList() ?? new List<string>();
    }

    private static bool SubjectNamesMatch(string? aiName, string? subjectName)
    {
        var left = NormalizeSubjectName(aiName);
        var right = NormalizeSubjectName(subjectName);

        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return false;
        }

        return left == right || left.Contains(right) || right.Contains(left);
    }

    private static List<Guid> PickSuggestedLessonIds(
        StudentStudyPlanSubjectProgressData subject,
        List<StudyPlanLessonOptionData> subjectLessons,
        List<string> aiLessonOrder
    )
    {
        var selected = new List<Guid>();

        foreach (var aiLesson in aiLessonOrder)
        {
            var matchedLesson = subjectLessons.FirstOrDefault(lesson =>
                LessonNamesMatch(aiLesson, lesson.LessonTitle) ||
                (SubjectNamesMatch(aiLesson, subject.SubjectName) && LessonNamesMatch(aiLesson, lesson.LessonTitle))
            );

            if (matchedLesson != null && !selected.Contains(matchedLesson.LessonId))
            {
                selected.Add(matchedLesson.LessonId);
            }
        }

        if (subject.NextLessonId != null && !selected.Contains(subject.NextLessonId.Value))
        {
            selected.Add(subject.NextLessonId.Value);
        }

        foreach (var lesson in subjectLessons)
        {
            if (selected.Count >= 3)
            {
                break;
            }

            if (!selected.Contains(lesson.LessonId))
            {
                selected.Add(lesson.LessonId);
            }
        }

        return selected;
    }

    private static bool LessonNamesMatch(string? aiText, string? lessonTitle)
    {
        var left = NormalizeSubjectName(aiText);
        var right = NormalizeSubjectName(lessonTitle);

        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return false;
        }

        return left == right || left.Contains(right) || right.Contains(left);
    }

    private static int NormalizeWeeklyStudyHours(int aiHours, string? availableStudyHours)
    {
        var maxHours = ExtractAvailableWeeklyHours(availableStudyHours);
        var normalized = aiHours <= 0 ? Math.Min(8, maxHours) : aiHours;

        return Math.Clamp(normalized, 1, maxHours);
    }

    private static int ExtractAvailableWeeklyHours(string? value)
    {
        var digits = new string((value ?? "").Where(char.IsDigit).ToArray());

        if (int.TryParse(digits, out var number) && number > 0)
        {
            return number <= 6 ? number * 5 : number;
        }

        return 12;
    }

    private static List<lesson> SortLessonsByRequestedOrder(List<lesson> lessons, List<Guid> requestedIds)
    {
        var order = requestedIds
            .Select((id, index) => new { id, index })
            .ToDictionary(item => item.id, item => item.index);

        return lessons
            .OrderBy(lesson => order.TryGetValue(lesson.id, out var index) ? index : int.MaxValue)
            .ToList();
    }

    private static string NormalizeSubjectName(string? value)
    {
        return (value ?? "")
            .Trim()
            .Replace("أ", "ا")
            .Replace("إ", "ا")
            .Replace("آ", "ا")
            .Replace("ة", "ه")
            .Replace("ى", "ي")
            .ToLowerInvariant();
    }

    private static StudyPlanDto MapToDto(study_plan plan)
    {
        var items = plan.study_plan_items
            .OrderBy(i => i.order_number ?? int.MaxValue)
            .Select(i => new StudyPlanItemDto
            {
                Id = i.id,
                LessonId = i.lesson_id,
                LessonTitle = i.lesson?.title ?? string.Empty,
                OrderNumber = i.order_number ?? 0,
                IsCompleted = i.is_completed ?? false
            })
            .ToList();

        var totalItems = items.Count;
        var completedItems = items.Count(i => i.IsCompleted);

        var progressPercent = totalItems == 0
            ? 0
            : Math.Round(completedItems * 100.0 / totalItems, 2);

        return new StudyPlanDto
        {
            Id = plan.id,
            SubjectId = plan.subject_id,
            SubjectName = plan.subject?.name ?? "",
            Title = plan.title,
            Description = plan.description,
            IsAiGenerated = plan.is_ai_generated ?? false,
            StudyDays = plan.study_days ?? new List<string>(),
            DailyDurationMinutes = plan.daily_duration_minutes,
            CreatedAt = plan.created_at,
            UpdatedAt = plan.updated_at,
            TotalItems = totalItems,
            CompletedItems = completedItems,
            ProgressPercent = progressPercent,
            Items = items
        };
    }

    private static DateTime GetUnspecifiedNow()
    {
        return DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
    }
}
