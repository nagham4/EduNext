namespace backend.DTOs.AI;

public class SubjectChatbotRequestDto
{
    public string Message { get; set; } = string.Empty;
    public string SubjectTitle { get; set; } = string.Empty;
    public string LessonTitle { get; set; } = string.Empty;
    public string SubjectKey { get; set; } = string.Empty;
    public string? ImageData { get; set; }
    public string? ImageMimeType { get; set; }
}
