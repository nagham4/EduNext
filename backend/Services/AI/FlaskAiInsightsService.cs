using System.Net.Http.Json;
using System.Text.Json.Serialization;
using backend.DTOs.AI;

namespace backend.Services.AI;

public class FlaskAiInsightsService : IAiInsightsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly MockAiInsightsService _fallback = new();

    public FlaskAiInsightsService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<AiExamAnalysisResponseDto> AnalyzeExamAsync(AiExamAnalysisRequestDto request, CancellationToken ct = default)
    {
        var baseUrl = _configuration["AiChatbot:BaseUrl"] ?? "http://localhost:5001";
        var analysisUrl = $"{baseUrl.TrimEnd('/')}/exam-analysis";

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                analysisUrl,
                new
                {
                    userId = request.UserId,
                    examId = request.ExamId,
                    examType = request.ExamType,
                    subjectName = request.SubjectName,
                    score = request.Score,
                    questions = request.Questions.Select(q => new
                    {
                        questionText = q.QuestionText,
                        selectedAnswer = q.SelectedAnswer,
                        correctAnswer = q.CorrectAnswer,
                        isCorrect = q.IsCorrect
                    }).ToList()
                },
                ct
            );

            response.EnsureSuccessStatusCode();

            var aiResponse = await response.Content.ReadFromJsonAsync<AiExamAnalysisApiResponse>(cancellationToken: ct);

            if (aiResponse == null || string.IsNullOrWhiteSpace(aiResponse.RecommendationText))
            {
                return await _fallback.AnalyzeExamAsync(request, ct);
            }

            return new AiExamAnalysisResponseDto
            {
                StrengthAreas = CleanList(aiResponse.StrengthAreas),
                WeakAreas = CleanList(aiResponse.WeakAreas),
                LevelMessage = aiResponse.LevelMessage?.Trim() ?? "",
                RecommendationText = aiResponse.RecommendationText.Trim()
            };
        }
        catch
        {
            return await _fallback.AnalyzeExamAsync(request, ct);
        }
    }

    public async Task<AiQuestionExplanationResponseDto> ExplainQuestionAsync(
        AiQuestionExplanationRequestDto request,
        CancellationToken ct = default
    )
    {
        var baseUrl = _configuration["AiChatbot:BaseUrl"] ?? "http://localhost:5001";
        var explanationUrl = $"{baseUrl.TrimEnd('/')}/question-explanation";

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                explanationUrl,
                new
                {
                    subjectName = request.SubjectName,
                    questionText = request.QuestionText,
                    selectedAnswer = request.SelectedAnswer,
                    selectedAnswerText = request.SelectedAnswerText,
                    correctAnswer = request.CorrectAnswer,
                    correctAnswerText = request.CorrectAnswerText,
                    isCorrect = request.IsCorrect
                },
                ct
            );

            response.EnsureSuccessStatusCode();

            var aiResponse = await response.Content.ReadFromJsonAsync<AiQuestionExplanationApiResponse>(cancellationToken: ct);

            if (aiResponse == null || string.IsNullOrWhiteSpace(aiResponse.SolutionText))
            {
                return await _fallback.ExplainQuestionAsync(request, ct);
            }

            return new AiQuestionExplanationResponseDto
            {
                SolutionText = aiResponse.SolutionText.Trim()
            };
        }
        catch
        {
            return await _fallback.ExplainQuestionAsync(request, ct);
        }
    }

    public async Task<AiPersonalizedRecommendationResponseDto> GeneratePersonalizedRecommendationAsync(
        AiPersonalizedRecommendationRequestDto request,
        CancellationToken ct = default
    )
    {
        var baseUrl = _configuration["AiChatbot:BaseUrl"] ?? "http://localhost:5001";
        var recommendationUrl = $"{baseUrl.TrimEnd('/')}/personalized-recommendation";

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(recommendationUrl, request, ct);

            response.EnsureSuccessStatusCode();

            var aiResponse = await response.Content.ReadFromJsonAsync<AiPersonalizedRecommendationApiResponse>(cancellationToken: ct);

            if (aiResponse == null || string.IsNullOrWhiteSpace(aiResponse.RecommendationText))
            {
                return await _fallback.GeneratePersonalizedRecommendationAsync(request, ct);
            }

            return new AiPersonalizedRecommendationResponseDto
            {
                RecommendationText = aiResponse.RecommendationText.Trim(),
                FocusSubjects = CleanList(aiResponse.FocusSubjects),
                WeeklyStudyHours = aiResponse.WeeklyStudyHours,
                LessonOrder = CleanList(aiResponse.LessonOrder),
                StrengthAreas = CleanList(aiResponse.StrengthAreas),
                WeakAreas = CleanList(aiResponse.WeakAreas),
                SubjectAnalyses = CleanSubjectAnalyses(aiResponse.SubjectAnalyses, request.Subjects)
            };
        }
        catch
        {
            return await _fallback.GeneratePersonalizedRecommendationAsync(request, ct);
        }
    }

    private static List<string> CleanList(IEnumerable<string>? values)
    {
        return values?
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim())
            .ToList() ?? new List<string>();
    }

    private static List<AiSubjectAnalysisDto> CleanSubjectAnalyses(
        IEnumerable<AiSubjectAnalysisApiResponse>? values,
        List<AiSubjectProgressDto> requestSubjects
    )
    {
        var subjectIdByName = requestSubjects
            .Where(s => !string.IsNullOrWhiteSpace(s.SubjectName))
            .GroupBy(s => s.SubjectName.Trim())
            .ToDictionary(g => g.Key, g => g.First().SubjectId);

        return values?
            .Where(v => !string.IsNullOrWhiteSpace(v.SubjectName))
            .Select(v =>
            {
                var name = v.SubjectName.Trim();
                subjectIdByName.TryGetValue(name, out var subjectId);

                return new AiSubjectAnalysisDto
                {
                    SubjectId = subjectId,
                    SubjectName = name,
                    Strengths = CleanList(v.Strengths),
                    Weaknesses = CleanList(v.Weaknesses)
                };
            })
            .Where(v => v.Strengths.Count > 0 || v.Weaknesses.Count > 0)
            .ToList() ?? new List<AiSubjectAnalysisDto>();
    }

    private sealed class AiExamAnalysisApiResponse
    {
        [JsonPropertyName("strengthAreas")]
        public List<string> StrengthAreas { get; set; } = new();

        [JsonPropertyName("weakAreas")]
        public List<string> WeakAreas { get; set; } = new();

        [JsonPropertyName("levelMessage")]
        public string? LevelMessage { get; set; }

        [JsonPropertyName("recommendationText")]
        public string? RecommendationText { get; set; }
    }

    private sealed class AiQuestionExplanationApiResponse
    {
        [JsonPropertyName("solutionText")]
        public string? SolutionText { get; set; }
    }

    private sealed class AiPersonalizedRecommendationApiResponse
    {
        [JsonPropertyName("recommendationText")]
        public string? RecommendationText { get; set; }

        [JsonPropertyName("focusSubjects")]
        public List<string> FocusSubjects { get; set; } = new();

        [JsonPropertyName("weeklyStudyHours")]
        public int WeeklyStudyHours { get; set; }

        [JsonPropertyName("lessonOrder")]
        public List<string> LessonOrder { get; set; } = new();

        [JsonPropertyName("strengthAreas")]
        public List<string> StrengthAreas { get; set; } = new();

        [JsonPropertyName("weakAreas")]
        public List<string> WeakAreas { get; set; } = new();

        [JsonPropertyName("subjectAnalyses")]
        public List<AiSubjectAnalysisApiResponse> SubjectAnalyses { get; set; } = new();
    }

    private sealed class AiSubjectAnalysisApiResponse
    {
        [JsonPropertyName("subjectName")]
        public string SubjectName { get; set; } = "";

        [JsonPropertyName("strengths")]
        public List<string> Strengths { get; set; } = new();

        [JsonPropertyName("weaknesses")]
        public List<string> Weaknesses { get; set; } = new();
    }
}
