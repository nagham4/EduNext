namespace backend.DTOs.AI;

public class AiQuestionExplanationRequestDto
{
    public string SubjectName { get; set; } = "";
    public string QuestionText { get; set; } = "";
    public string? SelectedAnswer { get; set; }
    public string? SelectedAnswerText { get; set; }
    public string CorrectAnswer { get; set; } = "";
    public string CorrectAnswerText { get; set; } = "";
    public bool IsCorrect { get; set; }
}

public class AiQuestionExplanationResponseDto
{
    public string SolutionText { get; set; } = "";
}
