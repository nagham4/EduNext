using backend.DTOs.Admin;
using backend.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers.Admin;

[ApiController]
[Route("api/admin/achievements")]
[Authorize(Roles = "admin")]
public class AdminAchievementsController : ControllerBase
{
    private readonly IAdminAchievementsService _achievementsService;

    public AdminAchievementsController(IAdminAchievementsService achievementsService)
    {
        _achievementsService = achievementsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAchievements(
        [FromQuery] string? search,
        [FromQuery] string? type = "all",
        [FromQuery] string? status = "all",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5
    )
    {
        var result = await _achievementsService.GetAchievementsPageAsync(
            search,
            type,
            status,
            page,
            pageSize
        );

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAchievement([FromBody] CreateAdminAchievementDto dto)
    {
        try
        {
            var result = await _achievementsService.CreateAchievementAsync(dto);

            if (result == null)
            {
                return BadRequest(new
                {
                    message = "تأكد من تعبئة جميع بيانات الإنجاز بشكل صحيح."
                });
            }

            return Ok(result);
        }
        catch (DbUpdateException)
        {
            return Conflict(new
            {
                message = "يوجد إنجاز بنفس العنوان أو حدث تعارض أثناء الحفظ."
            });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAchievement(Guid id, [FromBody] UpdateAdminAchievementDto dto)
    {
        try
        {
            var result = await _achievementsService.UpdateAchievementAsync(id, dto);

            if (result == null)
            {
                return BadRequest(new
                {
                    message = "الإنجاز غير موجود أو البيانات غير صحيحة."
                });
            }

            return Ok(result);
        }
        catch (DbUpdateException)
        {
            return Conflict(new
            {
                message = "يوجد إنجاز بنفس العنوان أو حدث تعارض أثناء التعديل."
            });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAchievement(Guid id)
    {
        var disabled = await _achievementsService.DeleteAchievementAsync(id);

        if (!disabled)
        {
            return NotFound(new
            {
                message = "الإنجاز غير موجود."
            });
        }

        return Ok(new
        {
            message = "تم تعطيل الإنجاز بنجاح."
        });
    }
}