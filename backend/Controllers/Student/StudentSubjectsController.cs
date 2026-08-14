using System.Security.Claims;
using backend.DTOs.Student;
using backend.Services.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Student;

[ApiController]
[Route("api/student/subjects")]
[Authorize(Roles = "student")]
public class StudentSubjectsController : ControllerBase
{
    private readonly IStudentSubjectService _service;

    public StudentSubjectsController(IStudentSubjectService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<SubjectDto>>> GetAll()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "تعذر التحقق من المستخدم الحالي."
            });
        }

        var subjects = await _service.GetAllAsync(userId.Value);

        return Ok(subjects);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubjectDetailsDto>> GetById(Guid id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "تعذر التحقق من المستخدم الحالي."
            });
        }

        var subject = await _service.GetByIdAsync(userId.Value, id);

        if (subject == null)
        {
            return NotFound(new
            {
                message = "المادة غير موجودة."
            });
        }

        return Ok(subject);
    }

    [HttpGet("{subjectId:guid}/lessons/{lessonId:guid}")]
    public async Task<ActionResult<LessonDetailsDto>> GetLessonDetails(Guid subjectId, Guid lessonId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "تعذر التحقق من المستخدم الحالي."
            });
        }

        var lesson = await _service.GetLessonDetailsAsync(userId.Value, subjectId, lessonId);

        if (lesson == null)
        {
            return NotFound(new
            {
                message = "الدرس غير موجود."
            });
        }

        return Ok(lesson);
    }

    [HttpPatch("{subjectId:guid}/lessons/{lessonId:guid}/completion")]
    public async Task<ActionResult<LessonCompletionResponseDto>> SetLessonCompleted(
        Guid subjectId,
        Guid lessonId,
        [FromBody] MarkLessonCompletedDto dto
    )
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "تعذر التحقق من المستخدم الحالي."
            });
        }

        var result = await _service.SetLessonCompletedAsync(
            userId.Value,
            subjectId,
            lessonId,
            dto.Completed,
            dto.DurationSeconds
        );

        if (result == null)
        {
            return NotFound(new
            {
                message = "الدرس غير موجود."
            });
        }

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("by-branch")]
    public async Task<ActionResult<List<OnboardingSubjectDto>>> GetByBranch([FromQuery] string branch)
    {
        if (string.IsNullOrWhiteSpace(branch))
        {
            return BadRequest(new
            {
                message = "الفرع مطلوب."
            });
        }

        var subjects = await _service.GetSubjectsByBranchAsync(branch);

        return Ok(subjects);
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
