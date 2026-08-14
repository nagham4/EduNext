using backend.DTOs.AI;

namespace backend.Services.AI;

public class MockAiInsightsService : IAiInsightsService
{
    public Task<AiExamAnalysisResponseDto> AnalyzeExamAsync(AiExamAnalysisRequestDto request, CancellationToken ct = default)
    {
        if (request.Score >= 85)
        {
            return Task.FromResult(new AiExamAnalysisResponseDto
            {
                StrengthAreas = new List<string> { "فهم المفاهيم", "حل المسائل" },
                WeakAreas = new List<string> { "بعض التفاصيل الدقيقة" },
                LevelMessage = "أداء ممتاز. مستواك جيد جدًا في هذا الاختبار.",
                RecommendationText = "أحسنت! استمر على هذا المستوى، وراجع النقاط البسيطة التي أخطأت فيها ثم انتقل إلى أسئلة أكثر تقدمًا."
            });
        }

        if (request.Score >= 60)
        {
            return Task.FromResult(new AiExamAnalysisResponseDto
            {
                StrengthAreas = new List<string> { "الأساسيات", "تذكر القوانين" },
                WeakAreas = new List<string> { "الأسئلة التطبيقية", "التركيز أثناء الحل" },
                LevelMessage = "أداء جيد. لديك أساس مناسب لكنك تحتاج إلى مزيد من التدريب.",
                RecommendationText = "راجع الأسئلة التي أخطأت فيها، وركّز على التمارين التطبيقية، ثم أعد المحاولة بعد مراجعة القوانين الأساسية."
            });
        }

        return Task.FromResult(new AiExamAnalysisResponseDto
        {
            StrengthAreas = new List<string> { "المحاولة", "الاستمرار في التعلم" },
            WeakAreas = new List<string> { "المفاهيم الأساسية", "الدقة في الإجابة" },
            LevelMessage = "يحتاج مستواك إلى تحسين. ابدأ من الأساسيات أولًا.",
            RecommendationText = "ارجع إلى شرح الدرس وملخصه، ثم ابدأ بحل الأسئلة السهلة تدريجيًا قبل الانتقال إلى الأسئلة الأصعب."
        });
    }

    public Task<AiQuestionExplanationResponseDto> ExplainQuestionAsync(
        AiQuestionExplanationRequestDto request,
        CancellationToken ct = default
    )
    {
        var selected = string.IsNullOrWhiteSpace(request.SelectedAnswerText)
            ? "لم يتم اختيار إجابة"
            : request.SelectedAnswerText;

        return Task.FromResult(new AiQuestionExplanationResponseDto
        {
            SolutionText =
                $"السؤال: {request.QuestionText}\n" +
                $"إجابتك: {selected}\n" +
                $"الإجابة الصحيحة: {request.CorrectAnswerText}\n" +
                "راجع فكرة السؤال، ثم قارن بين اختيارك والإجابة الصحيحة وحدد سبب الخطأ قبل إعادة حل سؤال مشابه."
        });
    }

    public Task<AiPersonalizedRecommendationResponseDto> GeneratePersonalizedRecommendationAsync(
        AiPersonalizedRecommendationRequestDto request,
        CancellationToken ct = default
    )
    {
        var weakestSubjects = request.Subjects
            .OrderBy(s => s.AverageScore <= 0 ? 100 : s.AverageScore)
            .ThenBy(s => s.ProgressPercent)
            .Take(2)
            .ToList();

        if (weakestSubjects.Count == 0 && request.DifficultSubjects.Count > 0)
        {
            weakestSubjects = request.DifficultSubjects
                .Take(2)
                .Select(name => new AiSubjectProgressDto { SubjectName = name })
                .ToList();
        }

        var focusSubjects = weakestSubjects
            .Select(s => s.SubjectName)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToList();

        var lessonOrder = weakestSubjects
            .Select(s => s.NextLessonTitle)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!)
            .Take(3)
            .ToList();

        return Task.FromResult(new AiPersonalizedRecommendationResponseDto
        {
            FocusSubjects = focusSubjects,
            WeeklyStudyHours = request.StudyHours.Contains("٤") || request.StudyHours.Contains("4") ? 18 : 12,
            LessonOrder = lessonOrder,
            RecommendationText = focusSubjects.Count == 0
                ? "ابدأ بحل اختبار قصير أو مشاهدة درس واحد حتى تظهر توصيات أدق بناءً على أدائك."
                : $"ركّز هذا الأسبوع على {string.Join(" و", focusSubjects)}، وابدأ بالدروس الأقل تقدماً ثم حل اختباراً قصيراً لقياس التحسن."
        });
    }
}
