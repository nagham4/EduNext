namespace backend.DTOs.Admin;

public class AdminUsersPageDto
{
    public List<AdminUserDto> Users { get; set; } = new();

    public AdminUsersStatsDto Stats { get; set; } = new();

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int TotalItems { get; set; }

    public int PageSize { get; set; }
}

public class AdminUserDto
{
    public Guid Id { get; set; }

    public string DisplayId { get; set; } = "";

    public string FullName { get; set; } = "";

    public string Email { get; set; } = "";

    public string Role { get; set; } = "";

    public string RoleLabel { get; set; } = "";

    public string Status { get; set; } = "";

    public bool IsActive { get; set; }

    public string RegistrationDate { get; set; } = "";

    public string Avatar { get; set; } = "";

    public string AvatarColor { get; set; } = "";
}

public class AdminUsersStatsDto
{
    public int StudentCount { get; set; }

    public int AdminCount { get; set; }

    public int ActiveUsersCount { get; set; }
}

public class AdminRoleOptionDto
{
    public string Label { get; set; } = "";

    public string Value { get; set; } = "";
}

public class UpdateAdminUserRoleDto
{
    public string Role { get; set; } = "";
}

public class UpdateAdminUserStatusDto
{
    public bool IsActive { get; set; }
}