using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Student;

public class SetupBasicInfoDto
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Stream { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string CurrentGrade { get; set; } = string.Empty;

    [Required]
    [Range(2024, 2100)]
    public int ExamYear { get; set; }
}