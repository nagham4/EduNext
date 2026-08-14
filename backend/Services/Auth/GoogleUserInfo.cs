namespace backend.Services.Auth;

public class GoogleUserInfo
{
    public string GoogleUserId { get; set; } = "";
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public bool EmailVerified { get; set; }
}