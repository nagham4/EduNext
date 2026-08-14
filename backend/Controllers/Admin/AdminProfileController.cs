using System.Security.Claims;
using backend.DTOs.Admin;
using backend.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers.Admin;

[ApiController]
[Route("api/admin/profile")]
[Authorize(Roles = "admin")]
public class AdminProfileController : ControllerBase
{
    private readonly IAdminProfileService _profileService;

    public AdminProfileController(IAdminProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        if (!TryGetUserId(out var adminId))
        {
            return Unauthorized(new
            {
                message = "Invalid admin token."
            });
        }

        var profile = await _profileService.GetProfileAsync(adminId);

        if (profile == null)
        {
            return NotFound(new
            {
                message = "فشل تحميل الملف الشخصي"
            });
        }

        return Ok(profile);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateAdminProfileDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!TryGetUserId(out var adminId))
        {
            return Unauthorized(new
            {
                message = "Invalid admin token."
            });
        }

        try
        {
            var profile = await _profileService.UpdateProfileAsync(adminId, dto);

            if (profile == null)
            {
                return BadRequest(new
                {
                    message = "تأكد من إدخال الاسم بشكل صحيح."
                });
            }

            return Ok(profile);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (DbUpdateException)
        {
            return BadRequest(new
            {
                message = "رقم الهاتف يجب أن يكون بصيغة دولية مثل: +970599123456"
            });
        }
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangeAdminPasswordDto dto)
    {
        if (!TryGetUserId(out var adminId))
        {
            return Unauthorized(new
            {
                message = "Invalid admin token."
            });
        }

        var changed = await _profileService.ChangePasswordAsync(adminId, dto);

        if (!changed)
        {
            return BadRequest(new
            {
                message = "تأكد من كلمة المرور الحالية ومن تطابق كلمة المرور الجديدة."
            });
        }

        return Ok(new
        {
            message = "تم تغيير كلمة المرور بنجاح."
        });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAccount()
    {
        if (!TryGetUserId(out var adminId))
        {
            return Unauthorized(new
            {
                message = "Invalid admin token."
            });
        }

        var deleted = await _profileService.DeleteAccountAsync(adminId);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "المستخدم غير موجود."
            });
        }

        return Ok(new
        {
            message = "تم تعطيل الحساب بنجاح."
        });
    }

    private bool TryGetUserId(out Guid userId)
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub") ??
            User.FindFirstValue("id");

        return Guid.TryParse(userIdValue, out userId);
    }
}
