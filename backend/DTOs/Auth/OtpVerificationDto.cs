using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Auth;

public class OtpVerificationDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{6}$")]
    public string Otp { get; set; } = string.Empty;
}
