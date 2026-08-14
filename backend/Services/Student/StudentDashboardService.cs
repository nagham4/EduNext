using backend.DTOs.Student;
using backend.Repositories.Student;
using backend.DTOs.AI;
using backend.Services.AI;

namespace backend.Services.Student;

public class StudentDashboardService : IStudentDashboardService
{
    private readonly IStudentDashboardRepository _repository;
    private readonly IAiInsightsService _ai;

    public StudentDashboardService(IStudentDashboardRepository repository, IAiInsightsService ai)
    {
        _repository = repository;
        _ai = ai;
    }

    public async Task<StudentDashboardDto> GetDashboardAsync(Guid userId)
    {
        var identity = await _repository.GetIdentityAsync(userId);

        var fullName = string.IsNullOrWhiteSpace(identity?.FullName)
            ? "الطالب"
            : identity.FullName;

        var studentStream = identity?.Stream;

        var today = DateTime.Today;
        var startOfWeek = GetStartOfWeek(today);
        var endOfWeek = startOfWeek.AddDays(7);

        var orderedWeekDays = GetOrderedWeekDays();

        var weeklySessions = await _repository.GetWeeklySessionsAsync(userId, startOfWeek, endOfWeek);

        var weeklyProgress = orderedWeekDays
            .Select((dayName, index) =>
            {
                var date = startOfWeek.AddDays(index);
                var minutes = weeklySessions.FirstOrDefault(x => x.Date.Date == date.Date)?.Minutes ?? 0;

                var value = minutes == 0
                    ? 0
                    : Math.Min(100, (int)Math.Round(minutes * 100.0 / 180.0));

                return new WeeklyProgressItemDto
                {
                    Day = dayName,
                    Value = value
                };
            })
            .ToList();

        var totalStudyMinutes = await _repository.GetTotalStudyMinutesAsync(userId);
        var totalStudyTimeText = FormatStudyDuration(totalStudyMinutes);

        if (string.IsNullOrWhiteSpace(studentStream))
        {
            return BuildEmptyDashboard(fullName, totalStudyTimeText, weeklyProgress);
        }

        var subjects = await _repository.GetSubjectsByStreamAsync(studentStream);
        var subjectIds = subjects.Select(s => s.SubjectId).ToList();

        var subjectTotals = await _repository.GetSubjectLessonTotalsAsync(subjectIds);
        var subjectCompleted = await _repository.GetSubjectCompletedLessonsAsync(userId, subjectIds);
        var examSummary = await _repository.GetExamSummaryAsync(userId, subjectIds);
        var subjectExamAverages = await _repository.GetSubjectExamAveragesAsync(userId, subjectIds);
        var totalDict = subjectTotals.ToDictionary(x => x.SubjectId, x => x.Total);
        var completedDict = subjectCompleted.ToDictionary(x => x.SubjectId, x => x.Completed);
        var subjectAverageDict = subjectExamAverages.ToDictionary(x => x.SubjectId, x => x.AverageScore);

        var totalLessons = subjectTotals.Sum(x => x.Total);
        var completedLessonsCount = subjectCompleted.Sum(x => x.Completed);

        var overallProgressPercent = totalLessons == 0
            ? 0
            : Math.Round(completedLessonsCount * 100.0 / totalLessons, 2);

        var averageScorePercent = Math.Round(examSummary.AverageScore, 2);
        var examsCount = examSummary.Count;

        var subjectProgress = subjects
            .Select(subject =>
            {
                totalDict.TryGetValue(subject.SubjectId, out var total);
                completedDict.TryGetValue(subject.SubjectId, out var completed);

                var progress = total == 0
                    ? 0
                    : Math.Round(completed * 100.0 / total, 2);

                var remaining = Math.Max(0, total - completed);

                return new SubjectProgressDto
                {
                    SubjectId = subject.SubjectId,
                    SubjectName = subject.SubjectName,
                    CompletedLessons = completed,
                    RemainingLessons = remaining,
                    ProgressPercent = progress
                };
            })
            .Where(x => x.CompletedLessons > 0 || x.RemainingLessons > 0)
            .OrderBy(x => x.ProgressPercent)
            .ThenByDescending(x => x.RemainingLessons)
            .ToList();

        var hasAnyProgress =
            totalStudyMinutes > 0 ||
            completedLessonsCount > 0 ||
            examsCount > 0;

        var weeklyStudyMinutes = weeklySessions.Sum(x => x.Minutes);
        var weeklyActiveDays = weeklySessions.Count(x => x.Minutes > 0);

        var recommendationResult = await BuildRecommendationsAsync(
            userId,
            fullName,
            studentStream,
            averageScorePercent,
            completedLessonsCount,
            totalLessons,
            hasAnyProgress,
            subjectProgress,
            identity ?? new DashboardIdentityData(),
            subjectAverageDict
        );

        var stats = BuildStats(
            overallProgressPercent,
            averageScorePercent,
            totalStudyTimeText,
            completedLessonsCount
        );

        return new StudentDashboardDto
        {
            Header = new DashboardHeaderDto
            {
                Title = $"أهلاً بك  {fullName} ",
            },
            Stats = stats,
            SubjectProgress = subjectProgress,
            WeeklyProgress = weeklyProgress,
            Recommendations = recommendationResult.Recommendations,
            MotivationalMessage = BuildMotivationalMessage(
                hasAnyProgress,
                weeklyStudyMinutes,
                weeklyActiveDays,
                completedLessonsCount,
                examsCount
            ),
            RecommendationsTitle = recommendationResult.Title,
            RecommendationsEmptyMessage = recommendationResult.EmptyMessage,
            HasAnyProgress = hasAnyProgress,
            IsAiRecommendations = recommendationResult.IsAi
        };
    }

    private async Task<DashboardRecommendationResult> BuildRecommendationsAsync(
        Guid userId,
        string fullName,
        string studentStream,
        double averageScorePercent,
        int completedLessonsCount,
        int totalLessons,
        bool hasAnyProgress,
        List<SubjectProgressDto> subjectProgress,
        DashboardIdentityData profile,
        Dictionary<Guid, double> subjectAverageScores
    )
    {
        var recommendations = new List<DashboardRecommendationDto>();

        if (hasAnyProgress && subjectProgress.Any())
        {
            var weakestSubjects = subjectProgress
                .OrderBy(x => x.ProgressPercent)
                .ThenByDescending(x => x.RemainingLessons)
                .Take(3)
                .ToList();

            var weakestSubjectIds = weakestSubjects
                .Select(x => x.SubjectId)
                .ToList();

            var lessonCandidates = await _repository.GetNextLessonsForSubjectsAsync(userId, weakestSubjectIds);

            var nextLessonBySubject = lessonCandidates
                .GroupBy(x => x.SubjectId)
                .ToDictionary(g => g.Key, g => g.First());

            var aiResponse = await _ai.GeneratePersonalizedRecommendationAsync(new AiPersonalizedRecommendationRequestDto
            {
                ContextType = "dashboard",
                StudentName = fullName,
                Stream = studentStream,
                CurrentLevel = profile.CurrentLevel,
                Goal = profile.Goal,
                StudyHours = profile.StudyHours,
                ExamExperience = profile.ExamExperience,
                LearningMethods = profile.LearningMethods,
                DifficultSubjects = profile.DifficultSubjects,
                AverageScore = averageScorePercent,
                CompletedLessons = completedLessonsCount,
                TotalLessons = totalLessons,
                Subjects = weakestSubjects.Select(subject =>
                {
                    nextLessonBySubject.TryGetValue(subject.SubjectId, out var nextLesson);

                    return new AiSubjectProgressDto
                    {
                        SubjectId = subject.SubjectId,
                        SubjectName = subject.SubjectName,
                        ProgressPercent = subject.ProgressPercent,
                        AverageScore = subjectAverageScores.TryGetValue(subject.SubjectId, out var averageScore)
                            ? averageScore
                            : 0,
                        CompletedLessons = subject.CompletedLessons,
                        TotalLessons = subject.CompletedLessons + subject.RemainingLessons,
                        RemainingLessons = subject.RemainingLessons,
                        NextLessonId = nextLesson?.LessonId,
                        NextLessonTitle = nextLesson?.LessonTitle
                    };
                }).ToList()
            });

            var recommendationText = aiResponse.RecommendationText;

            if (!string.IsNullOrWhiteSpace(recommendationText))
            {
                recommendations.Add(new DashboardRecommendationDto
                {
                    Title = "توصية مخصصة لك",
                    Tag = "الذكاء الاصطناعي",
                    TagColor = "purple",
                    Description = recommendationText
                });
            }

            foreach (var subject in weakestSubjects)
            {
                nextLessonBySubject.TryGetValue(subject.SubjectId, out var nextLesson);

                recommendations.Add(new DashboardRecommendationDto
                {
                    SubjectId = subject.SubjectId,
                    LessonId = nextLesson?.LessonId,
                    Title = nextLesson?.LessonTitle ?? $"تابع مادة {subject.SubjectName}",
                    Tag = subject.SubjectName,
                    TagColor = MapTagColor(subject.SubjectName),
                    Description = BuildRecommendationDescription(subject.SubjectName, subject.ProgressPercent)
                });
            }

            return new DashboardRecommendationResult
            {
                Recommendations = recommendations,
                Title = "توصيات دراسية مدعومة بالذكاء الاصطناعي",
                EmptyMessage = "",
                IsAi = true
            };
        }

        return new DashboardRecommendationResult
        {
            Recommendations = recommendations,
            Title = "توصيات دراسية مخصصة لك",
            EmptyMessage = "ابدأ بدراسة أول درس أو بحل أول اختبار لنتمكن من اقتراح خطوات مناسبة لك.",
            IsAi = false
        };
    }

    private static StudentDashboardDto BuildEmptyDashboard(
        string fullName,
        string totalStudyTimeText,
        List<WeeklyProgressItemDto> weeklyProgress
    )
    {
        return new StudentDashboardDto
        {
            Header = new DashboardHeaderDto
            {
                Title = $"أهلاً بك  {fullName} ",
                Subtitle = "أكمل إعداد بياناتك الدراسية للبدء."
            },
            Stats = new List<DashboardStatsDto>
            {
                new()
                {
                    Label = "التقدم الحالي",
                    Value = "0٪",
                    Color = "blue",
                    Icon = "TrendingUp"
                },
                new()
                {
                    Label = "متوسط الدرجات",
                    Value = "0٪",
                    Color = "amber",
                    Icon = "Star"
                },
                new()
                {
                    Label = "وقت الدراسة",
                    Value = totalStudyTimeText,
                    Color = "green",
                    Icon = "Clock"
                },
                new()
                {
                    Label = "الدروس المكتملة",
                    Value = "0 درس",
                    Color = "purple",
                    Icon = "GraduationCap"
                }
            },
            SubjectProgress = new List<SubjectProgressDto>(),
            WeeklyProgress = weeklyProgress,
            Recommendations = new List<DashboardRecommendationDto>(),
            MotivationalMessage = "أكمل التهيئة الدراسية أولًا حتى نبدأ بعرض تقدمك وتوصياتك.",
            RecommendationsTitle = "توصيات دراسية مخصصة لك",
            RecommendationsEmptyMessage = "بعد إكمال التهيئة وبدء الدراسة، ستظهر لك هنا توصيات مناسبة.",
            HasAnyProgress = false,
            IsAiRecommendations = false
        };
    }

    private static List<DashboardStatsDto> BuildStats(
        double overallProgressPercent,
        double averageScorePercent,
        string totalStudyTimeText,
        int completedLessonsCount
    )
    {
        return new List<DashboardStatsDto>
        {
            new()
            {
                Label = "التقدم الحالي",
                Value = $"{(int)Math.Round(overallProgressPercent)}٪",
                Color = "blue",
                Icon = "TrendingUp"
            },
            new()
            {
                Label = "متوسط الدرجات",
                Value = $"{(int)Math.Round(averageScorePercent)}٪",
                Color = "amber",
                Icon = "Star"
            },
            new()
            {
                Label = "وقت الدراسة",
                Value = totalStudyTimeText,
                Color = "green",
                Icon = "Clock"
            },
            new()
            {
                Label = "الدروس المكتملة",
                Value = $"{completedLessonsCount} درس",
                Color = "purple",
                Icon = "GraduationCap"
            }
        };
    }

    private static DateTime GetStartOfWeek(DateTime today)
    {
        var daysFromSaturday = ((int)today.DayOfWeek + 1) % 7;
        return today.Date.AddDays(-daysFromSaturday);
    }

    private static List<string> GetOrderedWeekDays()
    {
        return new List<string>
        {
            "السبت",
            "الأحد",
            "الإثنين",
            "الثلاثاء",
            "الأربعاء",
            "الخميس",
            "الجمعة"
        };
    }

    private static string MapTagColor(string subjectName) => subjectName switch
    {
        "الرياضيات" => "blue",
        "الفيزياء" => "green",
        "اللغة العربية" => "amber",
        "اللغة الإنجليزية" => "purple",
        "الكيمياء" => "green",
        "الأحياء" => "green",
        _ => "blue"
    };

    private static string BuildRecommendationDescription(string subjectName, double progressPercent)
    {
        if (progressPercent < 30)
        {
            return $"ابدأ بمراجعة أساسيات {subjectName} ثم انتقل إلى حل أسئلة سهلة ومتوسطة.";
        }

        if (progressPercent < 70)
        {
            return $"أنت في مستوى جيد في {subjectName}، ركز الآن على التمارين التطبيقية والأسئلة المتقدمة.";
        }

        return $"مستواك جيد في {subjectName}، حافظ على الاستمرارية وراجع النقاط الصعبة فقط.";
    }

    private static string BuildMotivationalMessage(
        bool hasAnyProgress,
        int weeklyStudyMinutes,
        int weeklyActiveDays,
        int completedLessonsCount,
        int examsCount
    )
    {
        if (!hasAnyProgress)
        {
            return "ابدأ أول جلسة دراسة هذا الأسبوع حتى نعرض لك تقدمك هنا بشكل أدق.";
        }

        if (weeklyStudyMinutes >= 300 || weeklyActiveDays >= 5)
        {
            return "لقد أحرزت تقدمًا ممتازًا هذا الأسبوع! استمر بهذا الأداء الرائع.";
        }

        if (weeklyStudyMinutes >= 120 || weeklyActiveDays >= 3)
        {
            return "أنت تسير بشكل جيد هذا الأسبوع، حاول الحفاظ على الاستمرارية للوصول إلى نتائج أفضل.";
        }

        if (completedLessonsCount > 0 || examsCount > 0)
        {
            return "بداية جيدة، واصل الدراسة بانتظام لتلاحظ تحسنًا أوضح في تقدمك.";
        }

        return "كل خطوة صغيرة تصنع فرقًا، ابدأ اليوم ولو بوقت بسيط.";
    }

    private static string FormatStudyDuration(int totalMinutes)
    {
        if (totalMinutes <= 0)
        {
            return "0 ساعة";
        }

        if (totalMinutes < 60)
        {
            return $"{totalMinutes} دقيقة";
        }

        var hours = totalMinutes / 60;
        var minutes = totalMinutes % 60;

        if (minutes == 0)
        {
            return $"{hours} ساعة";
        }

        return $"{hours} ساعة و {minutes} دقيقة";
    }

    private class DashboardRecommendationResult
    {
        public List<DashboardRecommendationDto> Recommendations { get; set; } = new();
        public string Title { get; set; } = "";
        public string EmptyMessage { get; set; } = "";
        public bool IsAi { get; set; }
    }
}
