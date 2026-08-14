using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.DTOs.Student;
using backend.Services.Student;

namespace backend.Controllers.Student;

[ApiController]
[Route("api/student/achievements")]
[Authorize(Roles = "student")]
public class StudentAchievementsController : ControllerBase
{
    private readonly IStudentAchievementService _service;

    public StudentAchievementsController(IStudentAchievementService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<AchievementsHubDto>> Get([FromQuery] int leaderboardSize = 10)
    {
        if (leaderboardSize < 1 || leaderboardSize > 50)
        {
            return BadRequest(new
            {
                message = "leaderboardSize must be between 1 and 50."
            });
        }

        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "تعذر التحقق من المستخدم الحالي."
            });
        }

        var result = await _service.GetHubAsync(userId.Value, leaderboardSize);

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