using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.DTOs.Student;
using backend.Services.Student;

namespace backend.Controllers.Student;

[ApiController]
[Route("api/student/study-plans")]
[Authorize(Roles = "student")]
public class StudentStudyPlansController : ControllerBase
{
    private readonly IStudentStudyPlanService _service;

    public StudentStudyPlansController(IStudentStudyPlanService service)
    {
        _service = service;
    }

    [HttpGet("subjects")]
    public async Task<ActionResult<List<StudyPlanSubjectOptionDto>>> GetSubjects()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new { message = "تعذر التحقق من المستخدم الحالي." });
        }

        var subjects = await _service.GetSubjectsAsync(userId.Value);

        return Ok(subjects);
    }

    [HttpGet("subjects/{subjectId:guid}/lessons")]
    public async Task<ActionResult<List<StudyPlanLessonOptionDto>>> GetLessons(Guid subjectId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new { message = "تعذر التحقق من المستخدم الحالي." });
        }

        var lessons = await _service.GetLessonsAsync(userId.Value, subjectId);

        return Ok(lessons);
    }

    [HttpGet]
    public async Task<ActionResult<List<StudyPlanDto>>> GetMyPlans()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new { message = "تعذر التحقق من المستخدم الحالي." });
        }

        var plans = await _service.GetMyPlansAsync(userId.Value);

        return Ok(plans);
    }

    [HttpGet("ai-suggestion")]
    public async Task<ActionResult<StudyPlanSuggestionDto>> GetAiSuggestion()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new { message = "تعذر التحقق من المستخدم الحالي." });
        }

        var suggestion = await _service.GetAiSuggestionAsync(userId.Value);

        return Ok(suggestion);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StudyPlanDto>> GetById(Guid id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new { message = "تعذر التحقق من المستخدم الحالي." });
        }

        var plan = await _service.GetByIdAsync(userId.Value, id);

        if (plan == null)
        {
            return NotFound(new { message = "الخطة الدراسية غير موجودة." });
        }

        return Ok(plan);
    }

    [HttpPost]
    public async Task<ActionResult<StudyPlanDto>> Create([FromBody] CreateStudyPlanDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new { message = "تعذر التحقق من المستخدم الحالي." });
        }

        try
        {
            var created = await _service.CreateAsync(userId.Value, dto);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StudyPlanDto>> Update(Guid id, [FromBody] UpdateStudyPlanDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new { message = "تعذر التحقق من المستخدم الحالي." });
        }

        try
        {
            var updated = await _service.UpdateAsync(userId.Value, id, dto);

            if (updated == null)
            {
                return NotFound(new { message = "الخطة الدراسية غير موجودة." });
            }

            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new { message = "تعذر التحقق من المستخدم الحالي." });
        }

        var deleted = await _service.DeleteAsync(userId.Value, id);

        if (!deleted)
        {
            return NotFound(new { message = "الخطة الدراسية غير موجودة." });
        }

        return NoContent();
    }

    [HttpPut("{planId:guid}/items/{itemId:guid}/completion")]
    public async Task<IActionResult> UpdateItemCompletion(
        Guid planId,
        Guid itemId,
        [FromQuery] bool isCompleted
    )
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new { message = "تعذر التحقق من المستخدم الحالي." });
        }

        var updated = await _service.UpdateItemCompletionAsync(
            userId.Value,
            planId,
            itemId,
            isCompleted
        );

        if (!updated)
        {
            return NotFound(new { message = "عنصر الخطة غير موجود." });
        }

        return NoContent();
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
