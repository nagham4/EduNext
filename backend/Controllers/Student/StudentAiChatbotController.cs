using backend.DTOs.AI;
using backend.Services.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Student;

[ApiController]
[Route("api/student/ai-chatbot")]
[Authorize(Roles = "student")]
public class StudentAiChatbotController : ControllerBase
{
    private readonly ISubjectChatbotService _chatbotService;

    public StudentAiChatbotController(ISubjectChatbotService chatbotService)
    {
        _chatbotService = chatbotService;
    }

    [HttpPost("chat")]
    public async Task<ActionResult<SubjectChatbotResponseDto>> Chat(
        [FromBody] SubjectChatbotRequestDto request,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(request.Message) && string.IsNullOrWhiteSpace(request.ImageData))
        {
            return BadRequest(new
            {
                message = "نص السؤال مطلوب."
            });
        }

        var subjectKey = request.SubjectKey.Trim().ToLowerInvariant();
        var subjectTitle = request.SubjectTitle.Trim().ToLowerInvariant();
        var supportedSubject =
            subjectKey is "math" or "english" ||
            subjectTitle.Contains("رياض") ||
            subjectTitle.Contains("math") ||
            subjectTitle.Contains("انج") ||
            subjectTitle.Contains("إنج") ||
            subjectTitle.Contains("english");

        if (!supportedSubject)
        {
            return BadRequest(new
            {
                message = "الشات بوت مفعّل حالياً للرياضيات واللغة الإنجليزية فقط."
            });
        }

        try
        {
            var response = await _chatbotService.SendAsync(request, ct);
            return Ok(response);
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, new
            {
                message = "تعذر الاتصال بخدمة الشات بوت Python. تأكد أنها تعمل على المنفذ 5001."
            });
        }
    }
}
