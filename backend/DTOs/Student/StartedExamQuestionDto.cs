namespace backend.DTOs.Student;

public class StartedExamQuestionDto
{
    public Guid QuestionId { get; set; }
    public string Text { get; set; } = "";
    public List<StartedExamOptionDto> Options { get; set; } = new();
}