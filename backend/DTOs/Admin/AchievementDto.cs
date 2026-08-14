using System;
using System.ComponentModel.DataAnnotations;
namespace backend.DTOs.Admin{

public record AchievementDto(
    [Required] string Title,
    [Required] string Description,
    [Required] string IconUrl, // رابط الشارة (Badge)
    [Required] string RequirementType, // مثال: "CompleteSubject", "Score100"
    int RequirementValue // القيمة المطلوبة (مثلاً 5 دروس)
);}