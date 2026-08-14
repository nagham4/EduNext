namespace backend.DTOs.Student;

public class SubmitExamDto
{
    public List<QuestionAnswerDto> Answers { get; set; } = new();
}

public class QuestionAnswerDto
{
    public Guid QuestionId { get; set; }
    public string SelectedAnswer { get; set; } = ""; // A/B/C/D
}