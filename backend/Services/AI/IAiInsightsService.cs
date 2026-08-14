using backend.DTOs.AI;

namespace backend.Services.AI;

public interface IAiInsightsService
{
    Task<AiExamAnalysisResponseDto> AnalyzeExamAsync(AiExamAnalysisRequestDto request, CancellationToken ct = default);

    Task<AiQuestionExplanationResponseDto> ExplainQuestionAsync(
        AiQuestionExplanationRequestDto request,
        CancellationToken ct = default
    );

    Task<AiPersonalizedRecommendationResponseDto> GeneratePersonalizedRecommendationAsync(
        AiPersonalizedRecommendationRequestDto request,
        CancellationToken ct = default
    );
}
