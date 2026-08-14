using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Student;

public class UpdateProfileDto
{
    [StringLength(100)]
    public string? FullName { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }
}