namespace backend.DTOs.AI;

public class AiExamAnalysisRequestDto
{
    public Guid UserId { get; set; }
    public Guid ExamId { get; set; }
    public string ExamType { get; set; } = "";
    public string SubjectName { get; set; } = "";
    public int Score { get; set; }

    public List<AiQuestionResultDto> Questions { get; set; } = new();
}

public class AiQuestionResultDto
{
    public string QuestionText { get; set; } = "";
    public string? SelectedAnswer { get; set; }
    public string? CorrectAnswer { get; set; }
    public bool IsCorrect { get; set; }
}