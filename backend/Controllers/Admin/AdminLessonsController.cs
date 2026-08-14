using backend.DTOs.Admin;
using backend.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers.Admin;

[ApiController]
[Route("api/admin/lessons")]
[Authorize(Roles = "admin")]
public class AdminLessonsController : ControllerBase
{
    private readonly IAdminLessonsService _lessonsService;

    public AdminLessonsController(IAdminLessonsService lessonsService)
    {
        _lessonsService = lessonsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLessons(
        [FromQuery] string? search,
        [FromQuery] string? department,
        [FromQuery] Guid? subjectId,
        [FromQuery] string? sortBy = "default",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 4
    )
    {
        var result = await _lessonsService.GetLessonsPageAsync(
            search,
            department,
            subjectId,
            sortBy,
            page,
            pageSize
        );

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateLesson([FromBody] CreateAdminLessonDto dto)
    {
        var result = await _lessonsService.CreateLessonAsync(dto);

        if (result == null)
        {
            return BadRequest(new
            {
                message = "المادة وعنوان الدرس مطلوبان، أو يوجد درس بنفس الاسم ضمن نفس المادة."
            });
        }

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateLesson(Guid id, [FromBody] UpdateAdminLessonDto dto)
    {
        var result = await _lessonsService.UpdateLessonAsync(id, dto);

        if (result == null)
        {
            return BadRequest(new
            {
                message = "الدرس غير موجود، أو البيانات غير صحيحة، أو يوجد درس بنفس الاسم ضمن نفس المادة."
            });
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteLesson(Guid id)
    {
        try
        {
            var deleted = await _lessonsService.DeleteLessonAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "الدرس غير موجود."
                });
            }

            return Ok(new
            {
                message = "تم حذف الدرس بنجاح."
            });
        }
        catch (DbUpdateException)
        {
            return Conflict(new
            {
                message = "لا يمكن حذف هذا الدرس لأنه مرتبط ببيانات أخرى مثل امتحانات أو تقدم الطلاب."
            });
        }
    }
}