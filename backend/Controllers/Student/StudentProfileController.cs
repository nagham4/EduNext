using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.DTOs.Student;
using backend.Services.Student;

namespace backend.Controllers.Student;

[ApiController]
[Route("api/student/profile")]
[Authorize(Roles = "student")]
public class StudentProfileController : ControllerBase
{
    private readonly IStudentProfileService _service;

    public StudentProfileController(IStudentProfileService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ProfileDto>> Get()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "تعذر التحقق من المستخدم الحالي."
            });
        }

        try
        {
            var profile = await _service.GetProfileAsync(userId.Value);

            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateProfileDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "تعذر التحقق من المستخدم الحالي."
            });
        }

        try
        {
            await _service.UpdateProfileAsync(userId.Value, dto);

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "تعذر التحقق من المستخدم الحالي."
            });
        }

        try
        {
            await _service.ChangePasswordAsync(userId.Value, dto);

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "تعذر التحقق من المستخدم الحالي."
            });
        }

        try
        {
            await _service.DeleteAccountAsync(userId.Value);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
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