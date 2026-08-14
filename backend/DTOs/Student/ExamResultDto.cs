namespace backend.DTOs.Student;

public class ExamResultDto
{
    public Guid ExamResultId { get; set; }
    public Guid ExamId { get; set; }

    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int WrongAnswers { get; set; }
    public int Percentage { get; set; }

    public List<string> StrengthAreas { get; set; } = new();
    public List<string> WeakAreas { get; set; } = new();

    public string? LevelMessage { get; set; }
    public string? RecommendationText { get; set; }

    public DateTime? CreatedAt { get; set; }

    public List<ExamReviewQuestionDto> Review { get; set; } = new();
}