namespace backend.DTOs.Student;

public class ExamReviewQuestionDto
{
    public Guid QuestionId { get; set; }
    public string QuestionText { get; set; } = "";
    public string? SelectedAnswer { get; set; }
    public string? SelectedAnswerText { get; set; }
    public string CorrectAnswer { get; set; } = "";
    public string CorrectAnswerText { get; set; } = "";
    public bool IsCorrect { get; set; }
    public string? Solution { get; set; }
}