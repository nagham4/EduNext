using backend.DTOs.Student;
using backend.Models.Generated;
using backend.Repositories.Student;

namespace backend.Services.Student;

public class StudentSubjectService : IStudentSubjectService
{
    private readonly IStudentSubjectRepository _repository;

    public StudentSubjectService(IStudentSubjectRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<SubjectDto>> GetAllAsync(Guid userId)
    {
        var studentStream = await _repository.GetStudentStreamAsync(userId);

        if (string.IsNullOrWhiteSpace(studentStream))
        {
            return new List<SubjectDto>();
        }

        var subjects = await _repository.GetSubjectsByStreamAsync(studentStream);
        var subjectIds = subjects.Select(s => s.SubjectId).ToList();

        var completedLessons = await _repository.GetCompletedLessonsForSubjectsAsync(userId, subjectIds);

        var completedBySubject = completedLessons
            .GroupBy(x => x.SubjectId)
            .ToDictionary(g => g.Key, g => g.Count());

        return subjects
            .Select(subject =>
            {
                completedBySubject.TryGetValue(subject.SubjectId, out var completedCount);

                var progress = subject.LessonsCount == 0
                    ? 0
                    : (int)Math.Round(completedCount * 100.0 / subject.LessonsCount);

                return new SubjectDto
                {
                    Id = subject.SubjectId,
                    Title = subject.SubjectName,
                    Desc = subject.Description,
                    Lessons = subject.LessonsCount,
                    Completed = completedCount,
                    Progress = progress,
                    Color = MapSubjectColor(subject.SubjectName),
                    IconKey = MapSubjectIcon(subject.SubjectName)
                };
            })
            .ToList();
    }

    public async Task<SubjectDetailsDto?> GetByIdAsync(Guid userId, Guid id)
    {
        var studentStream = await _repository.GetStudentStreamAsync(userId);

        if (string.IsNullOrWhiteSpace(studentStream))
        {
            return null;
        }

        var subject = await _repository.GetSubjectByIdAndStreamAsync(id, studentStream);

        if (subject == null)
        {
            return null;
        }

        var units = await _repository.GetUnitsBySubjectIdAsync(subject.id);
        var lessons = await _repository.GetLessonsBySubjectIdAsync(subject.id);

        var completedLessons = await _repository.GetCompletedLessonsForSubjectsAsync(
            userId,
            new List<Guid> { subject.id }
        );

        var completedLessonIds = completedLessons
            .Select(x => x.LessonId)
            .ToHashSet();

        var mappedUnits = units
            .Select(unit =>
            {
                var unitLessons = lessons
                    .Where(l => l.subject_unit_id == unit.id)
                    .OrderBy(l => l.order_number ?? int.MaxValue)
                    .ThenBy(l => l.title)
                    .Select((lesson, index) => new SubjectLessonDto
                    {
                        Id = lesson.id,
                        LessonId = lesson.id,
                        Title = lesson.title,
                        Duration = completedLessonIds.Contains(lesson.id)
                            ? "تم إنهاء الدرس"
                            : "غير مكتمل",
                        Completed = completedLessonIds.Contains(lesson.id)
                    })
                    .ToList();

                return new SubjectUnitDto
                {
                    Id = unit.id,
                    Title = unit.title,
                    OrderNumber = unit.order_number,
                    Lessons = unitLessons
                };
            })
            .ToList();

        var unitIds = units.Select(u => u.id).ToHashSet();

        var lessonsWithoutUnit = lessons
            .Where(l => l.subject_unit_id == null || !unitIds.Contains(l.subject_unit_id.Value))
            .OrderBy(l => l.order_number ?? int.MaxValue)
            .ThenBy(l => l.title)
            .Select((lesson, index) => new SubjectLessonDto
            {
                Id = lesson.id,
                LessonId = lesson.id,
                Title = lesson.title,
                Duration = completedLessonIds.Contains(lesson.id)
                    ? "تم إنهاء الدرس"
                    : "غير مكتمل",
                Completed = completedLessonIds.Contains(lesson.id)
            })
            .ToList();

        if (lessonsWithoutUnit.Count > 0)
        {
            mappedUnits.Add(new SubjectUnitDto
            {
                Id = Guid.Empty,
                Title = "دروس عامة",
                OrderNumber = int.MaxValue,
                Lessons = lessonsWithoutUnit
            });
        }

        var totalLessons = lessons.Count;
        var completedCount = lessons.Count(l => completedLessonIds.Contains(l.id));

        var progress = totalLessons == 0
            ? 0
            : (int)Math.Round(completedCount * 100.0 / totalLessons);

        return new SubjectDetailsDto
        {
            Id = subject.id,
            Title = subject.name,
            Desc = subject.description ?? string.Empty,
            Lessons = totalLessons,
            Completed = completedCount,
            Progress = progress,
            Color = MapSubjectColor(subject.name),
            IconKey = MapSubjectIcon(subject.name),
            Units = mappedUnits
        };
    }

    public async Task<LessonDetailsDto?> GetLessonDetailsAsync(Guid userId, Guid subjectId, Guid lessonId)
    {
        var studentStream = await _repository.GetStudentStreamAsync(userId);

        if (string.IsNullOrWhiteSpace(studentStream))
        {
            return null;
        }

        var subject = await _repository.GetSubjectByIdAndStreamAsync(subjectId, studentStream);

        if (subject == null)
        {
            return null;
        }

        var lessons = await _repository.GetLessonsBySubjectIdAsync(subject.id);
        var lesson = lessons.FirstOrDefault(l => l.id == lessonId);

        if (lesson == null)
        {
            return null;
        }

        var isCompleted = await _repository.IsLessonCompletedAsync(userId, lessonId);
        var lessonIndex = lessons.FindIndex(l => l.id == lessonId);

        return BuildLessonDetailsDto(
            subject,
            lesson,
            lessonIndex,
            lessons.Count,
            isCompleted
        );
    }

    public async Task<LessonCompletionResponseDto?> SetLessonCompletedAsync(
        Guid userId,
        Guid subjectId,
        Guid lessonId,
        bool completed,
        int? durationSeconds = null
    )
    {
        var studentStream = await _repository.GetStudentStreamAsync(userId);

        if (string.IsNullOrWhiteSpace(studentStream))
        {
            return null;
        }

        var subject = await _repository.GetSubjectByIdAndStreamAsync(subjectId, studentStream);

        if (subject == null)
        {
            return null;
        }

        var lessons = await _repository.GetLessonsBySubjectIdAsync(subject.id);
        var lesson = lessons.FirstOrDefault(l => l.id == lessonId);

        if (lesson == null)
        {
            return null;
        }

        var now = GetUnspecifiedNow();

        var progress = await _repository.GetLessonProgressAsync(userId, lessonId);
        var wasCompletedBefore = progress?.completed == true;

        if (progress == null)
        {
            progress = new lesson_progress
            {
                id = Guid.NewGuid(),
                user_id = userId,
                lesson_id = lessonId,
                completed = completed,
                completed_at = completed ? now : null
            };

            _repository.AddLessonProgress(progress);
        }
        else
        {
            progress.completed = completed;
            progress.completed_at = completed ? now : null;
        }

        if (completed && !wasCompletedBefore)
        {
            var hasStudySession = await _repository.HasStudySessionForLessonAsync(userId, lessonId);

            if (!hasStudySession)
            {
                var durationMinutes = GetLessonStudyDurationMinutes(lesson, durationSeconds);
                var endedAt = now;
                var startedAt = endedAt.AddMinutes(-durationMinutes);

                _repository.AddStudySession(new study_session
                {
                    id = Guid.NewGuid(),
                    user_id = userId,
                    subject_id = subject.id,
                    lesson_id = lesson.id,
                    started_at = startedAt,
                    ended_at = endedAt,
                    duration_minutes = durationMinutes,
                    session_type = "study",
                    created_at = now
                });
            }
        }

        await _repository.SaveChangesAsync();

        var shouldCheckAchievements = completed && !wasCompletedBefore;

        var newAchievements = shouldCheckAchievements
            ? await UnlockLessonAchievementsAsync(userId, now)
            : new List<UnlockedAchievementDto>();

        if (newAchievements.Count > 0)
        {
            await _repository.SaveChangesAsync();
        }

        var lessonIndex = lessons.FindIndex(l => l.id == lessonId);

        var lessonDto = BuildLessonDetailsDto(
            subject,
            lesson,
            lessonIndex,
            lessons.Count,
            completed
        );

        return new LessonCompletionResponseDto
        {
            Message = completed
                ? "تم تسجيل الدرس كمكتمل."
                : "تم إلغاء اكتمال الدرس.",
            Lesson = lessonDto,
            NewAchievements = newAchievements
        };
    }

    public async Task<List<OnboardingSubjectDto>> GetSubjectsByBranchAsync(string branch)
    {
        if (string.IsNullOrWhiteSpace(branch))
        {
            return new List<OnboardingSubjectDto>();
        }

        var cleanBranch = branch.Trim();

        var subjects = await _repository.GetSubjectsByBranchAsync(cleanBranch);

        return subjects
            .Select(s => new OnboardingSubjectDto
            {
                SubjectId = s.id,
                SubjectName = s.name
            })
            .ToList();
    }

    private async Task<List<UnlockedAchievementDto>> UnlockLessonAchievementsAsync(Guid userId, DateTime now)
    {
        var completedLessonsCount = await _repository.CountCompletedLessonsAsync(userId);

        var lessonAchievements = await _repository.GetAchievementsByConditionTypeAsync("lessons");
        var earnedAchievementIds = await _repository.GetEarnedAchievementIdsAsync(userId);

        var unlocked = new List<UnlockedAchievementDto>();

        foreach (var achievement in lessonAchievements)
        {
            if (earnedAchievementIds.Contains(achievement.id))
            {
                continue;
            }

            if (completedLessonsCount < achievement.condition_value)
            {
                continue;
            }

            _repository.AddUserAchievement(new user_achievement
            {
                id = Guid.NewGuid(),
                user_id = userId,
                achievement_id = achievement.id,
                earned_at = now
            });

            unlocked.Add(new UnlockedAchievementDto
            {
                AchievementId = achievement.id,
                Title = achievement.title,
                Description = achievement.description,
                Type = achievement.condition_type,
                Reward = "+50 نقطة"
            });
        }

        return unlocked;
    }

    private static LessonDetailsDto BuildLessonDetailsDto(
        subject subject,
        lesson lesson,
        int lessonIndex,
        int totalLessons,
        bool isCompleted
    )
    {
        return new LessonDetailsDto
        {
            LessonId = lesson.id,
            SubjectId = subject.id,
            SubjectTitle = subject.name,
            LessonNumber = lessonIndex + 1,
            TotalLessons = totalLessons,
            Title = lesson.title,
            Duration = isCompleted ? "مكتمل" : "غير مكتمل",
            Completed = isCompleted,
            VideoUrl = lesson.video_url,
            Explanation = lesson.content ?? string.Empty,
            Summary = SplitSummary(lesson.summary),
            PdfUrl = lesson.pdf_url,
            ResourcesUrl = lesson.resources_url
        };
    }

    private static List<string> SplitSummary(string? summary)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            return new List<string>();
        }

        return summary
            .Split(new[] { '\n', '\r', '-', '•' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
    }

    private static int GetLessonStudyDurationMinutes(lesson lesson, int? durationSeconds = null)
    {
        if (durationSeconds.HasValue && durationSeconds.Value > 0)
        {
            var minutes = (int)Math.Ceiling(durationSeconds.Value / 60.0);
            return Math.Clamp(minutes, 1, 240);
        }

        if (lesson.video_duration_seconds.HasValue && lesson.video_duration_seconds.Value > 0)
        {
            return Math.Max(1, (int)Math.Ceiling(lesson.video_duration_seconds.Value / 60.0));
        }

        return 30;
    }

    private static string MapSubjectColor(string name) => name switch
    {
        "الرياضيات" => "blue",
        "الفيزياء" => "green",
        "اللغة العربية" => "amber",
        "الكيمياء" => "purple",
        "اللغة الإنجليزية" => "blue",
        "الأحياء" => "green",
        "العلوم الحياتية" => "green",
        "التكنولوجيا" => "blue",
        "تكنولوجيا المعلومات" => "blue",
        "التربية الإسلامية" => "green",
        _ => "blue"
    };

    private static string MapSubjectIcon(string name) => name switch
    {
        "الرياضيات" => "BookMarked",
        "الفيزياء" => "Atom",
        "اللغة العربية" => "Languages",
        "الكيمياء" => "FlaskConical",
        "اللغة الإنجليزية" => "Globe",
        "الأحياء" => "BookOpen",
        "العلوم الحياتية" => "BookOpen",
        "التكنولوجيا" => "BookMarked",
        "تكنولوجيا المعلومات" => "BookMarked",
        "التربية الإسلامية" => "BookOpen",
        _ => "BookOpen"
    };

    private static DateTime GetUnspecifiedNow()
    {
        return DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
    }
}
