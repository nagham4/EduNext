using backend.DTOs.Admin;
using backend.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers.Admin;

[ApiController]
[Route("api/admin/subjects")]
[Authorize(Roles = "admin")]
public class AdminSubjectsController : ControllerBase
{
    private readonly IAdminSubjectsService _subjectsService;

    public AdminSubjectsController(IAdminSubjectsService subjectsService)
    {
        _subjectsService = subjectsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSubjects(
        [FromQuery] string? search,
        [FromQuery] string? department,
        [FromQuery] string? sortBy = "default",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 4
    )
    {
        var result = await _subjectsService.GetSubjectsPageAsync(
            search,
            department,
            sortBy,
            page,
            pageSize
        );

        return Ok(result);
    }

    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments()
    {
        var departments = await _subjectsService.GetDepartmentsAsync();
        return Ok(departments);
    }

    [HttpGet("{id:guid}/lessons")]
    public async Task<IActionResult> GetSubjectLessons(Guid id)
    {
        var lessons = await _subjectsService.GetSubjectLessonsAsync(id);
        return Ok(lessons);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubject([FromBody] CreateAdminSubjectDto dto)
    {
        var result = await _subjectsService.CreateSubjectAsync(dto);

        if (result == null)
        {
            return BadRequest(new
            {
                message = "اسم المادة والقسم مطلوبان، أو يوجد مادة بنفس الاسم."
            });
        }

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSubject(Guid id, [FromBody] UpdateAdminSubjectDto dto)
    {
        var result = await _subjectsService.UpdateSubjectAsync(id, dto);

        if (result == null)
        {
            return BadRequest(new
            {
                message = "المادة غير موجودة، أو البيانات غير صحيحة، أو يوجد مادة بنفس الاسم."
            });
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSubject(Guid id)
    {
        try
        {
            var deleted = await _subjectsService.DeleteSubjectAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "المادة غير موجودة."
                });
            }

            return Ok(new
            {
                message = "تم حذف المادة بنجاح."
            });
        }
        catch (DbUpdateException)
        {
            return Conflict(new
            {
                message = "لا يمكن حذف هذه المادة لأنها مرتبطة ببيانات أخرى."
            });
        }
    }
}
