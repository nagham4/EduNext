using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.DTOs.Student;
using backend.Services.Student;

namespace backend.Controllers.Student;

[ApiController]
[Route("api/student/dashboard")]
[Authorize(Roles = "student")]
public class StudentDashboardController : ControllerBase
{
    private readonly IStudentDashboardService _service;

    public StudentDashboardController(IStudentDashboardService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<StudentDashboardDto>> Get()
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

        var result = await _service.GetDashboardAsync(userId);

        return Ok(result);
    }
}