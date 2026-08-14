using backend.DTOs.Admin;
using backend.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers.Admin;

[ApiController]
[Route("api/admin/exams")]
[Authorize(Roles = "admin")]
public class AdminExamsController : ControllerBase
{
    private readonly IAdminExamsService _examsService;

    public AdminExamsController(IAdminExamsService examsService)
    {
        _examsService = examsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetExams([FromQuery] string? search)
    {
        var result = await _examsService.GetExamsPageAsync(search);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateExam([FromBody] CreateAdminExamDto dto)
    {
        var result = await _examsService.CreateExamAsync(dto);

        if (result == null)
        {
            return BadRequest(new
            {
                message = "تأكد من تعبئة عنوان الامتحان، المادة، الأسئلة، الخيارات، والإجابة الصحيحة."
            });
        }

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateExam(Guid id, [FromBody] UpdateAdminExamDto dto)
    {
        var result = await _examsService.UpdateExamAsync(id, dto);

        if (result == null)
        {
            return BadRequest(new
            {
                message = "الامتحان غير موجود أو البيانات غير صحيحة."
            });
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteExam(Guid id)
    {
        try
        {
            var deleted = await _examsService.DeleteExamAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "الامتحان غير موجود."
                });
            }

            return Ok(new
            {
                message = "تم حذف الامتحان بنجاح."
            });
        }
        catch (DbUpdateException)
        {
            return Conflict(new
            {
                message = "لا يمكن حذف هذا الامتحان لأنه مرتبط بنتائج طلاب."
            });
        }
    }
}