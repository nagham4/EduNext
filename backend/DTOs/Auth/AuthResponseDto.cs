namespace backend.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = "";
    public string? RefreshToken { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "";
    public bool IsOnboardingCompleted { get; set; }
    public string? Branch { get; set; }
}