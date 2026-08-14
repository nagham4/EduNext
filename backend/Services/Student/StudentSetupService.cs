using backend.DTOs.Student;
using backend.DTOs.AI;
using backend.Models.Generated;
using backend.Repositories.Student;
using backend.Services.AI;

namespace backend.Services.Student;

public class StudentSetupService : IStudentSetupService
{
    private readonly IStudentSetupRepository _repository;
    private readonly IAiInsightsService _ai;

    private static readonly HashSet<string> AllowedStudyHours = new()
    {
        "أقل من ساعة",
        "١-٢ ساعة",
        "٣-٤ ساعات",
        "أكثر من ٤ ساعات"
    };

    private static readonly HashSet<string> AllowedGoals = new()
    {
        "أعلى من ٩٠٪",
        "٨٠٪ - ٩٠٪",
        "٧٠٪ - ٨٠٪",
        "النجاح فقط"
    };

    private static readonly HashSet<string> AllowedLevels = new()
    {
        "مبتدئ",
        "متوسط",
        "متقدم"
    };

    private static readonly HashSet<string> AllowedExamExperiences = new()
    {
        "نعم، عدة مرات",
        "مرة واحدة",
        "لا، لم أجرب بعد"
    };

    public StudentSetupService(IStudentSetupRepository repository, IAiInsightsService ai)
    {
        _repository = repository;
        _ai = ai;
    }

    public async Task<OnboardingOptionsDto> GetOnboardingOptionsAsync()
    {
        var subjects = await _repository.GetAllSubjectsForOnboardingAsync();

        var branches = subjects
            .GroupBy(s => s.stream)
            .Select(group => new OnboardingBranchDto
            {
                Name = group.Key,
                Subjects = group
                    .Select(s => new OnboardingSubjectDto
                    {
                        SubjectId = s.id,
                        SubjectName = s.name
                    })
                    .ToList()
            })
            .ToList();

        return new OnboardingOptionsDto
        {
            Branches = branches,
            StudyHours = AllowedStudyHours.ToList(),
            Goals = AllowedGoals.ToList(),
            Levels = AllowedLevels.ToList(),
            ExamExperiences = AllowedExamExperiences.ToList()
        };
    }

    public async Task<StudentSetupDto> GetAsync(Guid userId)
    {
        var user = await _repository.GetUserByIdAsync(userId);

        if (user == null)
        {
            throw new InvalidOperationException("المستخدم غير موجود.");
        }

        var profile = await _repository.GetProfileForReadAsync(userId);

        return BuildSetupDto(user, profile);
    }

    public async Task<StudentSetupDto> SaveBasicInfoAsync(Guid userId, SetupBasicInfoDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var user = await _repository.GetUserByIdAsync(userId);

        if (user == null)
        {
            throw new InvalidOperationException("المستخدم غير موجود.");
        }

        var profile = await GetOrCreateProfileAsync(userId);

        EnsureLearningMethods(profile);

        user.full_name = CleanRequired(dto.FullName, "الاسم الكامل مطلوب.");

        profile.stream = CleanRequired(dto.Stream, "الفرع الدراسي مطلوب.");
        profile.current_grade = CleanRequired(dto.CurrentGrade, "الصف الحالي مطلوب.");
        profile.exam_year = dto.ExamYear;
        profile.updated_at = GetUtcNow();

        await _repository.SaveChangesAsync();

        return await GetAsync(userId);
    }

    public async Task<StudentSetupDto> SaveSubjectsAsync(Guid userId, SetupSubjectsDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var profile = await GetOrCreateProfileWithSubjectsAsync(userId);

        EnsureLearningMethods(profile);

        var subjectIds = dto.SubjectIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (subjectIds.Count == 0)
        {
            throw new ArgumentException("يجب اختيار مادة واحدة على الأقل.");
        }

        var validSubjects = await _repository.GetSubjectsByIdsAsync(subjectIds);
        var validIds = validSubjects.Select(s => s.id).ToHashSet();

        if (validIds.Count != subjectIds.Count)
        {
            throw new ArgumentException("بعض المواد المختارة غير صالحة.");
        }

        _repository.RemoveProfileSubjects(profile.student_profile_subjects);

        foreach (var subjectId in subjectIds)
        {
            _repository.AddProfileSubject(new student_profile_subject
            {
                id = Guid.NewGuid(),
                student_profile_id = profile.id,
                subject_id = subjectId,
                created_at = GetUnspecifiedNow()
            });
        }

        profile.updated_at = GetUtcNow();

        await _repository.SaveChangesAsync();

        return await GetAsync(userId);
    }

    public async Task<StudentSetupDto> SaveStudyPreferencesAsync(Guid userId, SetupStudyPreferencesDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var profile = await GetOrCreateProfileAsync(userId);

        EnsureLearningMethods(profile);

        profile.preferred_study_time = CleanRequired(dto.PreferredStudyTime, "وقت الدراسة المفضل مطلوب.");
        profile.preferred_study_place = CleanRequired(dto.PreferredStudyPlace, "مكان الدراسة المفضل مطلوب.");
        profile.primary_goal = CleanRequired(dto.PrimaryGoal, "الهدف الدراسي مطلوب.");
        profile.updated_at = GetUtcNow();

        await _repository.SaveChangesAsync();

        return await GetAsync(userId);
    }

    public async Task<StudentSetupDto> CompleteAsync(Guid userId)
    {
        var profile = await _repository.GetProfileWithSubjectsForUpdateAsync(userId);

        if (profile == null)
        {
            throw new InvalidOperationException("الملف الشخصي للطالب غير موجود.");
        }

        EnsureLearningMethods(profile);

        if (string.IsNullOrWhiteSpace(profile.stream) ||
            string.IsNullOrWhiteSpace(profile.current_grade) ||
            profile.exam_year == null)
        {
            throw new ArgumentException("البيانات الأساسية غير مكتملة.");
        }

        if (!profile.student_profile_subjects.Any())
        {
            throw new ArgumentException("يجب اختيار مادة واحدة على الأقل.");
        }

        if (string.IsNullOrWhiteSpace(profile.preferred_study_time) ||
            string.IsNullOrWhiteSpace(profile.preferred_study_place) ||
            string.IsNullOrWhiteSpace(profile.primary_goal))
        {
            throw new ArgumentException("تفضيلات الدراسة غير مكتملة.");
        }

        profile.is_onboarding_completed = true;
        profile.updated_at = GetUtcNow();

        await _repository.SaveChangesAsync();

        return await GetAsync(userId);
    }

    public async Task<StudentSetupDto> CompleteOnboardingAsync(Guid userId, CompleteOnboardingDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var user = await _repository.GetUserByIdAsync(userId);

        if (user == null)
        {
            throw new InvalidOperationException("المستخدم غير موجود.");
        }

        var branch = CleanRequired(dto.Branch, "الفرع الدراسي مطلوب.");
        var hours = CleanRequired(dto.Hours, "عدد ساعات الدراسة مطلوب.");
        var goal = CleanRequired(dto.Goal, "هدف التوجيهي مطلوب.");
        var level = CleanRequired(dto.Level, "المستوى الحالي مطلوب.");
        var examExp = CleanRequired(dto.ExamExp, "خبرة الاختبارات مطلوبة.");

        var cleanMethods = CleanStringList(dto.Methods);
        var difficultNames = CleanStringList(dto.Difficult);

        if (cleanMethods.Count == 0)
        {
            throw new ArgumentException("يجب اختيار طريقة تعلم واحدة على الأقل.");
        }

        if (difficultNames.Count == 0)
        {
            throw new ArgumentException("يجب اختيار مادة صعبة واحدة على الأقل.");
        }

        var branchExists = await _repository.BranchExistsAsync(branch);

        if (!branchExists)
        {
            throw new ArgumentException("الفرع الدراسي غير موجود في قاعدة البيانات.");
        }

        if (!AllowedStudyHours.Contains(hours))
        {
            throw new ArgumentException("قيمة ساعات الدراسة غير صحيحة.");
        }

        if (!AllowedGoals.Contains(goal))
        {
            throw new ArgumentException("قيمة الهدف غير صحيحة.");
        }

        if (!AllowedLevels.Contains(level))
        {
            throw new ArgumentException("قيمة المستوى الحالي غير صحيحة.");
        }

        if (!AllowedExamExperiences.Contains(examExp))
        {
            throw new ArgumentException("قيمة الخبرة في الاختبارات غير صحيحة.");
        }

        var matchedSubjects = await _repository.GetSubjectsByBranchAndNamesAsync(branch, difficultNames);

        if (matchedSubjects.Count != difficultNames.Count)
        {
            var matchedNames = matchedSubjects
                .Select(s => s.name)
                .ToHashSet();

            var missing = difficultNames
                .Where(x => !matchedNames.Contains(x))
                .ToList();

            throw new ArgumentException($"بعض المواد غير موجودة لهذا الفرع: {string.Join("، ", missing)}");
        }

        var profile = await GetOrCreateProfileWithSubjectsAsync(userId);

        EnsureLearningMethods(profile);

        profile.stream = branch;
        profile.preferred_study_time = hours;
        profile.primary_goal = goal;
        profile.current_grade = level;
        profile.exam_experience = examExp;
        profile.learning_methods = cleanMethods;
        profile.is_onboarding_completed = true;
        profile.updated_at = GetUtcNow();

        _repository.RemoveProfileSubjects(profile.student_profile_subjects);

        foreach (var subject in matchedSubjects)
        {
            _repository.AddProfileSubject(new student_profile_subject
            {
                id = Guid.NewGuid(),
                student_profile_id = profile.id,
                subject_id = subject.id,
                created_at = GetUnspecifiedNow()
            });
        }

        user.onboarding_completed = true;
        user.onboarding_completed_at = GetUtcNow();

        var aiRecommendation = await _ai.GeneratePersonalizedRecommendationAsync(new AiPersonalizedRecommendationRequestDto
        {
            ContextType = "onboarding",
            Stream = branch,
            CurrentLevel = level,
            Goal = goal,
            StudyHours = hours,
            ExamExperience = examExp,
            LearningMethods = cleanMethods,
            DifficultSubjects = difficultNames,
            Subjects = matchedSubjects.Select(subject => new AiSubjectProgressDto
            {
                SubjectId = subject.id,
                SubjectName = subject.name ?? "",
                ProgressPercent = 0,
                CompletedLessons = 0,
                RemainingLessons = 0
            }).ToList()
        });

        if (!string.IsNullOrWhiteSpace(aiRecommendation.RecommendationText))
        {
            _repository.AddAiRecommendation(new ai_recommendation
            {
                id = Guid.NewGuid(),
                user_id = userId,
                recommendation_text = aiRecommendation.RecommendationText,
                created_at = GetUnspecifiedNow()
            });
        }

        await _repository.SaveChangesAsync();

        return BuildCompletedSetupDto(user, profile, matchedSubjects);
    }

    private async Task<student_profile> GetOrCreateProfileAsync(Guid userId)
    {
        var profile = await _repository.GetProfileForUpdateAsync(userId);

        if (profile != null)
        {
            EnsureLearningMethods(profile);
            return profile;
        }

        return _repository.CreateProfile(userId, GetUtcNow());
    }

    private async Task<student_profile> GetOrCreateProfileWithSubjectsAsync(Guid userId)
    {
        var profile = await _repository.GetProfileWithSubjectsForUpdateAsync(userId);

        if (profile != null)
        {
            EnsureLearningMethods(profile);
            return profile;
        }

        profile = _repository.CreateProfile(userId, GetUtcNow());

        EnsureLearningMethods(profile);

        return profile;
    }

    private static StudentSetupDto BuildSetupDto(user user, student_profile? profile)
    {
        return new StudentSetupDto
        {
            UserId = user.id,
            FullName = user.full_name ?? string.Empty,
            Stream = profile?.stream,
            CurrentGrade = profile?.current_grade,
            ExamYear = profile?.exam_year,
            PreferredStudyTime = profile?.preferred_study_time,
            PreferredStudyPlace = profile?.preferred_study_place,
            PrimaryGoal = profile?.primary_goal,
            IsOnboardingCompleted = user.onboarding_completed,

            SelectedSubjectIds = profile?.student_profile_subjects
                .Select(x => x.subject_id)
                .ToList() ?? new List<Guid>(),

            DifficultSubjectNames = profile?.student_profile_subjects
                .Where(x => x.subject != null)
                .Select(x => x.subject.name)
                .ToList() ?? new List<string>(),

            LearningMethods = profile?.learning_methods?.ToList() ?? new List<string>(),
            ExamExperience = profile?.exam_experience
        };
    }

    private static StudentSetupDto BuildCompletedSetupDto(
        user user,
        student_profile profile,
        List<subject> matchedSubjects
    )
    {
        return new StudentSetupDto
        {
            UserId = user.id,
            FullName = user.full_name ?? string.Empty,
            Stream = profile.stream,
            CurrentGrade = profile.current_grade,
            ExamYear = profile.exam_year,
            PreferredStudyTime = profile.preferred_study_time,
            PreferredStudyPlace = profile.preferred_study_place,
            PrimaryGoal = profile.primary_goal,
            IsOnboardingCompleted = user.onboarding_completed,

            SelectedSubjectIds = matchedSubjects
                .Select(s => s.id)
                .ToList(),

            DifficultSubjectNames = matchedSubjects
                .Select(s => s.name)
                .ToList(),

            LearningMethods = profile.learning_methods?.ToList() ?? new List<string>(),
            ExamExperience = profile.exam_experience
        };
    }

    private static void EnsureLearningMethods(student_profile profile)
    {
        profile.learning_methods ??= new List<string>();
    }

    private static string CleanRequired(string? value, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(errorMessage);
        }

        return value.Trim();
    }

    private static List<string> CleanStringList(IEnumerable<string>? values)
    {
        return values?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct()
            .ToList() ?? new List<string>();
    }

    private static DateTime GetUtcNow()
    {
        return DateTime.UtcNow;
    }

    private static DateTime GetUnspecifiedNow()
    {
        return DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
    }
}
