using backend.DTOs.Auth;
using backend.Services.Auth;
using backend.Services.Guest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly IPasswordValidator _passwordValidator;

    public AuthController(IAuthService authService, ILogger<AuthController> logger, IPasswordValidator passwordValidator)
    {
        _authService = authService;
        _logger = logger;
        _passwordValidator = passwordValidator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto dto)
    {
        var result = await _authService.RegisterAsync(dto);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new
            {
                message = result.Message,
                errors = result.Errors,
                passwordSuggestions = result.PasswordSuggestions
            });
        }

        return Ok(result.Data);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto dto)
    {
        var result = await _authService.LoginAsync(dto);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new
            {
                message = result.Message,
                errors = result.Errors
            });
        }

        return Ok(result.Data);
    }

    [HttpPost("google-login")]
    public async Task<ActionResult<AuthResponseDto>> GoogleLogin([FromBody] GoogleLoginRequestDto dto)
    {
        var result = await _authService.GoogleLoginAsync(dto);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new
            {
                message = result.Message,
                errors = result.Errors
            });
        }

        return Ok(result.Data);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var emailSent = await _authService.RequestPasswordResetAsync(dto.Email);

            if (!emailSent)
                return NotFound(new { message = "لا يوجد حساب نشط مرتبط بهذا البريد الإلكتروني." });

            return Ok(new
            {
                emailSent = true,
                message = "تم إرسال رمز التحقق إلى بريدك الإلكتروني."
            });
        }
        catch (EmailDeliveryException ex)
        {
            _logger.LogError(ex, "Password reset email could not be sent.");

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = "تعذر إرسال رمز التحقق. تأكد من إعدادات البريد ثم حاول مرة أخرى."
            });
        }
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] OtpVerificationDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var isValid = await _authService.VerifyOtpAsync(dto.Email, dto.Otp);

        if (!isValid)
            return BadRequest(new { message = "رمز التحقق غير صحيح أو انتهت صلاحيته." });

        return Ok(new { message = "تم التحقق من الرمز بنجاح." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (dto.NewPassword != dto.ConfirmPassword)
        {
            return BadRequest(new
            {
                message = "كلمتا المرور غير متطابقتين.",
                errors = new Dictionary<string, List<string>>
                {
                    ["confirmPassword"] = new() { "كلمتا المرور غير متطابقتين." }
                }
            });
        }

        var passwordSuggestions = _passwordValidator.GetPasswordSuggestions(dto.NewPassword);

        if (passwordSuggestions.Any())
        {
            return BadRequest(new
            {
                message = "كلمة المرور لا تفي بالمتطلبات. يرجى التحقق من الشروط التالية:",
                passwordSuggestions = passwordSuggestions
            });
        }

        var success = await _authService.ResetPasswordAsync(dto.Email, dto.NewPassword);

        if (!success)
            return BadRequest(new { message = "فشل تحديث كلمة المرور. أعد طلب رمز تحقق جديد وحاول مرة أخرى." });

        return Ok(new { message = "تم تحديث كلمة المرور بنجاح." });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshAsync(request, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new
            {
                message = result.Message,
                errors = result.Errors
            });
        }

        return Ok(result.Data);
    }
}
