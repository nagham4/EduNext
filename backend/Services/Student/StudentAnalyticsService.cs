using backend.DTOs.Student;
using backend.DTOs.AI;
using backend.Repositories.Student;
using backend.Services.AI;

namespace backend.Services.Student;

public class StudentAnalyticsService : IStudentAnalyticsService
{
    private readonly IStudentAnalyticsRepository _repository;
    private readonly IAiInsightsService _ai;

    public StudentAnalyticsService(IStudentAnalyticsRepository repository, IAiInsightsService ai)
    {
        _repository = repository;
        _ai = ai;
    }

    public async Task<StudentAnalyticsDto> GetAnalyticsAsync(Guid userId)
    {
        var studentStream = await _repository.GetStudentStreamAsync(userId);

        if (string.IsNullOrWhiteSpace(studentStream))
        {
            return BuildEmptyAnalytics();
        }

        var examSummary = await _repository.GetExamSummaryAsync(userId);

        var totalLessons = await _repository.GetTotalLessonsByStreamAsync(studentStream);
        var completedLessons = await _repository.GetCompletedLessonsByStreamAsync(userId, studentStream);

        var subjectScores = await _repository.GetSubjectScoresByStreamAsync(userId, studentStream);
        var monthlyProgressRaw = await _repository.GetMonthlyProgressAsync(userId);
        var subjectExamAverages = await _repository.GetSubjectExamAveragesAsync(userId, studentStream);
        var subjectDetailData = await _repository.GetSubjectDetailsAsync(userId, studentStream);

        if (subjectDetailData.Count == 0 && subjectScores.Count > 0)
        {
            subjectDetailData = subjectScores.Select(x => new StudentAnalyticsSubjectDetailData
            {
                SubjectId = x.SubjectId,
                SubjectName = x.SubjectName,
                AverageScore = x.Score,
                ExamCount = 1
            }).ToList();
        }

        var monthlyProgress = monthlyProgressRaw
            .Select(x => new MonthlyProgressPointDto
            {
                Month = GetArabicMonthName(x.Month),
                Value = x.Average
            })
            .ToList();

        var localStrengthAreas = BuildOverallStrengths(
            examSummary,
            subjectScores,
            completedLessons,
            totalLessons
        );
        var localWeakAreas = BuildOverallWeaknesses(examSummary, subjectScores);
        var aiInsights = await GenerateAnalyticsInsightsAsync(
            studentStream,
            examSummary,
            completedLessons,
            totalLessons,
            subjectScores,
            subjectDetailData
        );
        var subjectAnalysesById = aiInsights.SubjectAnalyses
            .Where(x => x.SubjectId != null)
            .GroupBy(x => x.SubjectId!.Value)
            .ToDictionary(g => g.Key, g => g.First());
        var subjectAnalysesByName = aiInsights.SubjectAnalyses
            .Where(x => !string.IsNullOrWhiteSpace(x.SubjectName))
            .GroupBy(x => x.SubjectName.Trim())
            .ToDictionary(g => g.Key, g => g.First());

        return new StudentAnalyticsDto
        {
            Overview = new AnalyticsOverviewDto
            {
                OverallLevel = GetOverallLevel(examSummary.AverageScore),
                AverageScore = Math.Round(examSummary.AverageScore, 2),
                CompletedLessons = completedLessons,
                TotalLessons = totalLessons,
                PassedExams = examSummary.PassedExams,
                TotalExams = examSummary.TotalExams
            },
            SubjectScores = subjectScores
                .Select(x => new AnalyticsSubjectScoreDto
                {
                    SubjectId = x.SubjectId,
                    SubjectName = x.SubjectName,
                    Score = x.Score
                })
                .ToList(),
            ExamScores = subjectExamAverages
                .Select(x => new StudentAnalyticsExamScoreDto
                {
                    Name = x.Name,
                    Date = x.Date?.ToString("yyyy-MM-dd") ?? "غير محدد",
                    Average = x.Average,
                    SubjectsCount = x.SubjectsCount,
                    Level = x.Level
                })
                .ToList(),
            SubjectDetails = subjectDetailData
                .Select(x => new AnalyticsSubjectDetailDto
                {
                    SubjectId = x.SubjectId,
                    Name = x.SubjectName,
                    AverageScore = x.AverageScore,
                    ExamCount = x.ExamCount,
                    Level = GetOverallLevel(x.AverageScore),
                    Strengths = GetSubjectAiAnalysis(x, subjectAnalysesById, subjectAnalysesByName)?.Strengths
                        ?? BuildSubjectStrengths(x),
                    Weaknesses = GetSubjectAiAnalysis(x, subjectAnalysesById, subjectAnalysesByName)?.Weaknesses
                        ?? BuildSubjectWeaknesses(x)
                })
                .ToList(),
            StrengthAreas = aiInsights.StrengthAreas.Count > 0 ? aiInsights.StrengthAreas : localStrengthAreas,
            WeakAreas = aiInsights.WeakAreas.Count > 0 ? aiInsights.WeakAreas : localWeakAreas,
            RecommendationText = aiInsights.RecommendationText,
            MonthlyProgress = monthlyProgress
        };
    }

    private async Task<AiPersonalizedRecommendationResponseDto> GenerateAnalyticsInsightsAsync(
        string studentStream,
        StudentAnalyticsExamSummaryData examSummary,
        int completedLessons,
        int totalLessons,
        List<StudentAnalyticsSubjectScoreData> subjectScores,
        List<StudentAnalyticsSubjectDetailData> subjectDetailData
    )
    {
        try
        {
            var detailBySubject = subjectDetailData
                .GroupBy(x => x.SubjectId)
                .ToDictionary(g => g.Key, g => g.First());

            var subjects = subjectScores
                .OrderBy(x => x.Score)
                .Take(8)
                .Select(score =>
                {
                    detailBySubject.TryGetValue(score.SubjectId, out var detail);

                    return new AiSubjectProgressDto
                    {
                        SubjectId = score.SubjectId,
                        SubjectName = score.SubjectName,
                        ProgressPercent = score.Score,
                        AverageScore = score.Score,
                        CompletedLessons = detail?.ExamCount ?? 0,
                        TotalLessons = Math.Max(detail?.ExamCount ?? 0, 1),
                        RemainingLessons = 0
                    };
                })
                .ToList();

            var aiResponse = await _ai.GeneratePersonalizedRecommendationAsync(
                new AiPersonalizedRecommendationRequestDto
                {
                    ContextType = "analytics-performance",
                    Stream = studentStream,
                    CurrentLevel = GetOverallLevel(examSummary.AverageScore),
                    AverageScore = Math.Round(examSummary.AverageScore, 2),
                    CompletedLessons = completedLessons,
                    TotalLessons = totalLessons,
                    Subjects = subjects
                }
            );

            return aiResponse;
        }
        catch
        {
        }

        return BuildLocalAiInsightsFallback(
            examSummary,
            completedLessons,
            totalLessons,
            subjectScores,
            subjectDetailData
        );
    }

    private static AiSubjectAnalysisDto? GetSubjectAiAnalysis(
        StudentAnalyticsSubjectDetailData subject,
        Dictionary<Guid, AiSubjectAnalysisDto> analysesById,
        Dictionary<string, AiSubjectAnalysisDto> analysesByName
    )
    {
        if (analysesById.TryGetValue(subject.SubjectId, out var byId))
        {
            return byId.Strengths.Count > 0 || byId.Weaknesses.Count > 0 ? byId : null;
        }

        if (analysesByName.TryGetValue(subject.SubjectName.Trim(), out var byName))
        {
            return byName.Strengths.Count > 0 || byName.Weaknesses.Count > 0 ? byName : null;
        }

        return null;
    }

    private static AiPersonalizedRecommendationResponseDto BuildLocalAiInsightsFallback(
        StudentAnalyticsExamSummaryData examSummary,
        int completedLessons,
        int totalLessons,
        List<StudentAnalyticsSubjectScoreData> subjectScores,
        List<StudentAnalyticsSubjectDetailData> subjectDetailData
    )
    {
        var weakSubjects = subjectScores
            .Where(x => x.Score < 65)
            .OrderBy(x => x.Score)
            .Take(2)
            .Select(x => x.SubjectName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToList();

        var recommendation = weakSubjects.Count > 0
            ? $"ركز هذا الأسبوع على {string.Join(" و", weakSubjects)} حسب نتائجك الحالية، وابدأ بمراجعة الأساسيات ثم حل اختبار قصير للتأكد من التحسن."
            : "استمر على خطة المراجعة الحالية، وحافظ على حل اختبارات قصيرة لقياس التحسن بشكل منتظم.";

        return new AiPersonalizedRecommendationResponseDto
        {
            RecommendationText = recommendation,
            FocusSubjects = weakSubjects,
            WeeklyStudyHours = 12,
            StrengthAreas = BuildOverallStrengths(examSummary, subjectScores, completedLessons, totalLessons),
            WeakAreas = BuildOverallWeaknesses(examSummary, subjectScores),
            SubjectAnalyses = subjectDetailData
                .Select(x => new AiSubjectAnalysisDto
                {
                    SubjectId = x.SubjectId,
                    SubjectName = x.SubjectName,
                    Strengths = BuildSubjectStrengths(x),
                    Weaknesses = BuildSubjectWeaknesses(x)
                })
                .ToList()
        };
    }

    private static StudentAnalyticsDto BuildEmptyAnalytics()
    {
        return new StudentAnalyticsDto
        {
            Overview = new AnalyticsOverviewDto
            {
                OverallLevel = "بحاجة لتحسين",
                AverageScore = 0,
                CompletedLessons = 0,
                TotalLessons = 0,
                PassedExams = 0,
                TotalExams = 0
            },
            SubjectScores = new List<AnalyticsSubjectScoreDto>(),
            ExamScores = new List<StudentAnalyticsExamScoreDto>(),
            SubjectDetails = new List<AnalyticsSubjectDetailDto>(),
            StrengthAreas = new List<string>(),
            WeakAreas = new List<string>(),
            RecommendationText = "أكمل إعداد بياناتك الدراسية وابدأ بحل الاختبارات حتى تظهر توصيات الأداء.",
            MonthlyProgress = new List<MonthlyProgressPointDto>()
        };
    }

    private static string GetOverallLevel(double averageScore)
    {
        if (averageScore >= 85)
        {
            return "ممتاز";
        }

        if (averageScore >= 75)
        {
            return "جيد جداً";
        }

        if (averageScore >= 65)
        {
            return "جيد";
        }

        if (averageScore >= 50)
        {
            return "مقبول";
        }

        return "بحاجة لتحسين";
    }

    private static string GetArabicMonthName(int month)
    {
        return month switch
        {
            1 => "يناير",
            2 => "فبراير",
            3 => "مارس",
            4 => "أبريل",
            5 => "مايو",
            6 => "يونيو",
            7 => "يوليو",
            8 => "أغسطس",
            9 => "سبتمبر",
            10 => "أكتوبر",
            11 => "نوفمبر",
            12 => "ديسمبر",
            _ => ""
        };
    }

    private static List<string> SplitCsv(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new List<string>();
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .ToList();
    }

    private static List<string> BuildOverallStrengths(
        StudentAnalyticsExamSummaryData summary,
        List<StudentAnalyticsSubjectScoreData> subjectScores,
        int completedLessons,
        int totalLessons
    )
    {
        var strengths = new List<string>();

        if (summary.TotalExams > 0)
        {
            strengths.Add("المحاولة والاستمرار في حل الاختبارات.");
        }

        var strongSubjects = subjectScores
            .Where(x => x.Score >= 70)
            .OrderByDescending(x => x.Score)
            .Take(2)
            .Select(x => x.SubjectName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToList();

        if (strongSubjects.Count > 0)
        {
            strengths.Add($"أفضل أداء حتى الآن في: {string.Join("، ", strongSubjects)}.");
        }

        if (totalLessons > 0 && completedLessons > 0)
        {
            strengths.Add($"أنجزت {completedLessons} من أصل {totalLessons} درس.");
        }

        return strengths;
    }

    private static List<string> BuildOverallWeaknesses(
        StudentAnalyticsExamSummaryData summary,
        List<StudentAnalyticsSubjectScoreData> subjectScores
    )
    {
        var weaknesses = new List<string>();

        var weakSubjects = subjectScores
            .Where(x => x.Score < 65)
            .OrderBy(x => x.Score)
            .Take(2)
            .Select(x => x.SubjectName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToList();

        if (weakSubjects.Count > 0)
        {
            weaknesses.Add($"تحتاج لمراجعة إضافية في: {string.Join("، ", weakSubjects)}.");
        }

        if (summary.AverageScore < 50 && summary.TotalExams > 0)
        {
            weaknesses.Add("رفع متوسط الدرجات العام من خلال مراجعة الأساسيات.");
        }

        if (summary.TotalExams < 3)
        {
            weaknesses.Add("حل اختبارات إضافية للحصول على صورة أدق عن الأداء.");
        }

        return weaknesses;
    }

    private static List<string> BuildSubjectStrengths(StudentAnalyticsSubjectDetailData detail)
    {
        var strengths = new List<string>();

        if (detail.AverageScore >= 85)
        {
            strengths.Add($"أداء قوي ومستقر في مادة {detail.SubjectName}.");
        }
        else if (detail.AverageScore >= 70)
        {
            strengths.Add($"فهم جيد للمفاهيم الأساسية في {detail.SubjectName}.");
        }
        else
        {
            strengths.Add($"بدء بناء الثقة في {detail.SubjectName} من خلال المراجعة المتكررة.");
        }

        if (detail.ExamCount >= 3)
        {
            strengths.Add($"أجريت {detail.ExamCount} اختبارات في هذه المادة مما يعطي صورة أوضح عن الأداء.");
        }

        return strengths;
    }

    private static List<string> BuildSubjectWeaknesses(StudentAnalyticsSubjectDetailData detail)
    {
        var weaknesses = new List<string>();

        if (detail.AverageScore < 80)
        {
            weaknesses.Add($"تعزيز حل التمارين في {detail.SubjectName} لتحسين المعدل.");
        }

        if (detail.AverageScore < 65)
        {
            weaknesses.Add($"ركز على المفاهيم الصعبة والتمارين المتقدمة في {detail.SubjectName}.");
        }

        if (detail.ExamCount < 3)
        {
            weaknesses.Add("عدد الاختبارات قليل حاول زيادة التمارين العملية.");
        }

        return weaknesses;
    }
}
