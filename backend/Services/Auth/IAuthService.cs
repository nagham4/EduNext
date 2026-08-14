using backend.DTOs.Auth;

namespace backend.Services.Auth;

public interface IAuthService
{
    Task<AuthResult<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto);
    Task<AuthResult<AuthResponseDto>> LoginAsync(LoginRequestDto dto);
    Task<AuthResult<AuthResponseDto>> GoogleLoginAsync(GoogleLoginRequestDto dto);
    Task<bool> RequestPasswordResetAsync(string email);
    Task<bool> VerifyOtpAsync(string email, string otp);
    Task<bool> ResetPasswordAsync(string email, string newPassword);
    Task<AuthResult<AuthResponseDto>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
}
