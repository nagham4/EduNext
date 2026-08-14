using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.DTOs.Student;
using backend.Services.Student;

namespace backend.Controllers.Student;

[ApiController]
[Route("api/student/analytics")]
[Authorize(Roles = "student")]
public class StudentAnalyticsController : ControllerBase
{
    private readonly IStudentAnalyticsService _service;

    public StudentAnalyticsController(IStudentAnalyticsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<StudentAnalyticsDto>> Get()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "تعذر التحقق من المستخدم الحالي."
            });
        }

        var result = await _service.GetAnalyticsAsync(userId.Value);

        return Ok(result);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdValue))
        {
            return null;
        }

        return Guid.TryParse(userIdValue, out var userId)
            ? userId
            : null;
    }
}