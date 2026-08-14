namespace backend.DTOs.Auth;

public class RegisterRequestDto
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
    public bool AcceptedTerms { get; set; }
}