using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Student;

public class SetupStudyPreferencesDto
{
    [Required]
    [StringLength(50)]
    public string PreferredStudyTime { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string PreferredStudyPlace { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string PrimaryGoal { get; set; } = string.Empty;
}