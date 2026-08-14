using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.DTOs.Student;
using backend.Services.Student;

namespace backend.Controllers.Student;

[ApiController]
[Route("api/student/progress")]
[Authorize(Roles = "student")]
public class StudentProgressController : ControllerBase
{
    private readonly ProgressService _service;

    public StudentProgressController(ProgressService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<LessonProgressDto>> Get()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _service.GetProgressAsync(userId);
        return Ok(result);
    }

    [HttpGet("by-subject")]
public async Task<ActionResult<List<SubjectProgressDto>>> GetBySubject()
{
    var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var result = await _service.GetProgressBySubjectAsync(userId);
    return Ok(result);
}
}