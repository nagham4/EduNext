using System.ComponentModel.DataAnnotations;
using backend.Models.Enums;

namespace backend.DTOs.Admin
{

// لعرض قائمة المستخدمين ونشاطهم (مطلوب في الإدارة)
public record UserSummaryDto(
    string Id,
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAt,
    int ExamsTakenCount // عرض نشاط المستخدم
);}