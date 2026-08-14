using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.DTOs.Student;
using backend.Services.Student;

namespace backend.Controllers.Student;

[ApiController]
[Route("api/student/setup")]
[Authorize(Roles = "student")]
public class StudentSetupController : ControllerBase
{
    private readonly IStudentSetupService _service;

    public StudentSetupController(IStudentSetupService service)
    {
        _service = service;
    }

    [HttpGet("onboarding-options")]
    public async Task<IActionResult> GetOnboardingOptions()
    {
        var result = await _service.GetOnboardingOptionsAsync();

        return Ok(result);
    }

    [HttpPost("complete-onboarding")]
    public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboardingDto dto)
    {
        try
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdValue))
            {
                return Unauthorized(new
                {
                    message = "تعذر التحقق من المستخدم الحالي."
                });
            }

            var userId = Guid.Parse(userIdValue);

            var result = await _service.CompleteOnboardingAsync(userId, dto);

            return Ok(new
            {
                message = "تم حفظ بيانات التهيئة بنجاح.",
                isOnboardingCompleted = result.IsOnboardingCompleted,
                branch = result.Stream
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("=== CompleteOnboarding Exception ===");
            Console.WriteLine(ex.ToString());

            return StatusCode(500, new
            {
                message = "حدث خطأ أثناء حفظ بيانات الطالب."
            });
        }
    }
}