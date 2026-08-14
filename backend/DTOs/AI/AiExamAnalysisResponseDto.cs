namespace backend.DTOs.AI;

public class AiExamAnalysisResponseDto
{
    public List<string> StrengthAreas { get; set; } = new();
    public List<string> WeakAreas { get; set; } = new();
    public string LevelMessage { get; set; } = "";
    public string RecommendationText { get; set; } = "";
}