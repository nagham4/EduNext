using backend.Data.Generated;
using backend.DTOs.Admin;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Admin;

public class AdminUsersRepository : IAdminUsersRepository
{
    private readonly AppDbContext _context;

    public AdminUsersRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminUsersPageDto> GetUsersPageAsync(
        string? search,
        string? role,
        string? status,
        int page,
        int pageSize
    )
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 4 : pageSize;

        var query = _context.users
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();

            query = query.Where(u =>
                (u.full_name != null && u.full_name.ToLower().Contains(normalizedSearch)) ||
                u.email.ToLower().Contains(normalizedSearch)
            );
        }

        var normalizedRole = NormalizeRole(role);

        if (!string.IsNullOrWhiteSpace(normalizedRole) && normalizedRole != "all")
        {
            query = query.Where(u => u.role == normalizedRole);
        }

        if (!string.IsNullOrWhiteSpace(status) && status != "all")
        {
            if (status == "active" || status == "نشط")
            {
                query = query.Where(u => u.is_active == true);
            }
            else if (status == "disabled" || status == "معطل")
            {
                query = query.Where(u => u.is_active != true);
            }
        }

        var totalItems = await query.CountAsync();

        var totalPages = totalItems == 0
            ? 1
            : (int)Math.Ceiling((double)totalItems / pageSize);

        if (page > totalPages)
        {
            page = totalPages;
        }

        var rawUsers = await query
            .OrderByDescending(u => u.created_at)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.id,
                u.full_name,
                u.email,
                u.role,
                u.is_active,
                u.created_at
            })
            .ToListAsync();

        var users = rawUsers
            .Select(u => new AdminUserDto
            {
                Id = u.id,
                DisplayId = BuildDisplayId(u.id),
                FullName = u.full_name ?? "مستخدم بدون اسم",
                Email = u.email,
                Role = u.role,
                RoleLabel = GetRoleLabel(u.role),
                Status = u.is_active == true ? "نشط" : "معطل",
                IsActive = u.is_active == true,
                RegistrationDate = FormatDate(u.created_at),
           // Avatar = GetAvatarByRole(u.role),
                AvatarColor = GetAvatarColorByRole(u.role)
            })
            .ToList();

        return new AdminUsersPageDto
        {
            Users = users,
            Stats = await GetStatsAsync(),
            CurrentPage = page,
            TotalPages = totalPages,
            TotalItems = totalItems,
            PageSize = pageSize
        };
    }

    public async Task<AdminUserDto?> GetUserByIdAsync(Guid id)
    {
        var rawUser = await _context.users
            .AsNoTracking()
            .Where(u => u.id == id)
            .Select(u => new
            {
                u.id,
                u.full_name,
                u.email,
                u.role,
                u.is_active,
                u.created_at
            })
            .FirstOrDefaultAsync();

        if (rawUser == null)
        {
            return null;
        }

        return new AdminUserDto
        {
            Id = rawUser.id,
            DisplayId = BuildDisplayId(rawUser.id),
            FullName = rawUser.full_name ?? "مستخدم بدون اسم",
            Email = rawUser.email,
            Role = rawUser.role,
            RoleLabel = GetRoleLabel(rawUser.role),
            Status = rawUser.is_active == true ? "نشط" : "معطل",
            IsActive = rawUser.is_active == true,
            RegistrationDate = FormatDate(rawUser.created_at),
           // Avatar = GetAvatarByRole(rawUser.role),
            AvatarColor = GetAvatarColorByRole(rawUser.role)
        };
    }

    public async Task<List<AdminRoleOptionDto>> GetRolesAsync()
    {
        var existingRoles = await _context.users
            .AsNoTracking()
            .Where(u => u.role == "student" || u.role == "admin")
            .Select(u => u.role)
            .Distinct()
            .ToListAsync();

        var roles = new List<AdminRoleOptionDto>();

        if (existingRoles.Contains("student"))
        {
            roles.Add(new AdminRoleOptionDto
            {
                Label = "طالب",
                Value = "student"
            });
        }

        if (existingRoles.Contains("admin"))
        {
            roles.Add(new AdminRoleOptionDto
            {
                Label = "مسؤول",
                Value = "admin"
            });
        }

        if (roles.Count == 0)
        {
            roles.Add(new AdminRoleOptionDto
            {
                Label = "طالب",
                Value = "student"
            });

            roles.Add(new AdminRoleOptionDto
            {
                Label = "مسؤول",
                Value = "admin"
            });
        }

        return roles;
    }

    public async Task<AdminUserDto?> UpdateRoleAsync(Guid id, string role)
    {
        var normalizedRole = NormalizeRole(role);

        if (string.IsNullOrWhiteSpace(normalizedRole) || normalizedRole == "all")
        {
            return null;
        }

        if (!IsAllowedRole(normalizedRole))
        {
            return null;
        }

        var user = await _context.users
            .FirstOrDefaultAsync(u => u.id == id);

        if (user == null)
        {
            return null;
        }

        user.role = normalizedRole;

        await _context.SaveChangesAsync();

        return await GetUserByIdAsync(id);
    }

    public async Task<AdminUserDto?> UpdateStatusAsync(Guid id, bool isActive)
    {
        var user = await _context.users
            .FirstOrDefaultAsync(u => u.id == id);

        if (user == null)
        {
            return null;
        }

        user.is_active = isActive;

        await _context.SaveChangesAsync();

        return await GetUserByIdAsync(id);
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _context.users
            .FirstOrDefaultAsync(u => u.id == id);

        if (user == null)
        {
            return false;
        }

        user.is_active = false;

        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<AdminUsersStatsDto> GetStatsAsync()
    {
        var studentCount = await _context.users
            .AsNoTracking()
            .CountAsync(u => u.role == "student");

        var adminCount = await _context.users
            .AsNoTracking()
            .CountAsync(u => u.role == "admin");

        var activeUsersCount = await _context.users
            .AsNoTracking()
            .CountAsync(u => u.is_active == true);

        return new AdminUsersStatsDto
        {
            StudentCount = studentCount,
            AdminCount = adminCount,
            ActiveUsersCount = activeUsersCount
        };
    }

    private static string NormalizeRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return "all";
        }

        return role.Trim() switch
        {
            "الكل" => "all",
            "جميع الأدوار" => "all",
            "طالب" => "student",
            "مسؤول" => "admin",
            "student" => "student",
            "admin" => "admin",
            _ => role.Trim()
        };
    }

    private static bool IsAllowedRole(string role)
    {
        return role is "student" or "admin";
    }

    private static string GetRoleLabel(string role)
    {
        return role switch
        {
            "student" => "طالب",
            "admin" => "مسؤول",
            _ => role
        };
    }

    // private static string GetAvatarByRole(string role)
    // {
    //     return role switch
    //     {
    //         "student" => "🧑",
    //         "admin" => "👩",
    //         _ => "👤"
    //     };
    // }

    private static string GetAvatarColorByRole(string role)
    {
        return role switch
        {
            "student" => "#e3f2fd",
            "admin" => "#fff3e0",
            _ => "#e8f0fe"
        };
    }

    private static string BuildDisplayId(Guid id)
    {
        var shortId = id.ToString("N")[..4].ToUpper();
        return $"ED-{shortId}";
    }

    private static string FormatDate(DateTime? date)
    {
        return date == null
            ? ""
            : date.Value.ToString("yyyy-MM-dd");
    }
}