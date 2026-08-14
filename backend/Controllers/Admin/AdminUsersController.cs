using backend.DTOs.Admin;
using backend.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUsersService _usersService;

    public AdminUsersController(IAdminUsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? role = "all",
        [FromQuery] string? status = "all",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 4
    )
    {
        var result = await _usersService.GetUsersPageAsync(
            search,
            role,
            status,
            page,
            pageSize
        );

        return Ok(result);
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _usersService.GetRolesAsync();
        return Ok(roles);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _usersService.GetUserByIdAsync(id);

        if (user == null)
        {
            return NotFound(new
            {
                message = "المستخدم غير موجود."
            });
        }

        return Ok(user);
    }

    [HttpPatch("{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateAdminUserRoleDto dto)
    {
        var result = await _usersService.UpdateRoleAsync(id, dto.Role);

        if (result == null)
        {
            return BadRequest(new
            {
                message = "المستخدم غير موجود أو الدور غير صالح."
            });
        }

        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateAdminUserStatusDto dto)
    {
        var result = await _usersService.UpdateStatusAsync(id, dto.IsActive);

        if (result == null)
        {
            return NotFound(new
            {
                message = "المستخدم غير موجود."
            });
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var disabled = await _usersService.DeleteUserAsync(id);

        if (!disabled)
        {
            return NotFound(new
            {
                message = "المستخدم غير موجود."
            });
        }

        return Ok(new
        {
            message = "تم تعطيل المستخدم بنجاح."
        });
    }
}