using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Admin;

public class AdminProfileDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = "";

    public string Email { get; set; } = "";

    public string Phone { get; set; } = "";

    public string Role { get; set; } = "admin";

    public string RoleLabel { get; set; } = "مسؤول";

    public string Branch { get; set; } = "";

    public string AcademicYear { get; set; } = "";

    public List<AdminProfileActivityGroupDto> ActivityHistory { get; set; } = new();
}

public class AdminProfileActivityGroupDto
{
    public string DateLabel { get; set; } = "";

    public List<AdminProfileActivityItemDto> Items { get; set; } = new();
}

public class AdminProfileActivityItemDto
{
    public string Type { get; set; } = "";

    public string Text { get; set; } = "";

    public string Time { get; set; } = "";

    public string Color { get; set; } = "";
}

public class UpdateAdminProfileDto
{
    [StringLength(100)]
    public string FullName { get; set; } = "";

    [StringLength(30)]
    public string? Phone { get; set; }
}

public class ChangeAdminPasswordDto
{
    public string CurrentPassword { get; set; } = "";

    public string NewPassword { get; set; } = "";

    public string ConfirmNewPassword { get; set; } = "";
}
