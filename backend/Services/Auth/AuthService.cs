using System.Security.Cryptography;
using System.Text.RegularExpressions;
using backend.Data.Generated;
using backend.DTOs.Auth;
using backend.Models;
using backend.Models.Generated;
using backend.Services.Guest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace backend.Services.Auth;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordValidator _passwordValidator;
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IEmailSender _emailSender;
    private readonly IMemoryCache _memoryCache;

    public AuthService(
        AppDbContext db,
        IJwtTokenService jwtTokenService,
        IPasswordValidator passwordValidator,
        IGoogleTokenValidator googleTokenValidator,
        IEmailSender emailSender,
        IMemoryCache memoryCache)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _passwordValidator = passwordValidator;
        _googleTokenValidator = googleTokenValidator;
        _emailSender = emailSender;
        _memoryCache = memoryCache;
    }

    public async Task<AuthResult<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto)
    {
        if (dto == null)
        {
            return AuthResult<AuthResponseDto>.Fail(
                400,
                "الطلب غير صالح."
            );
        }

        var errors = new Dictionary<string, List<string>>();

        var fullName = dto.FullName?.Trim() ?? "";
        var email = dto.Email?.Trim().ToLower() ?? "";
        var password = dto.Password ?? "";
        var confirmPassword = dto.ConfirmPassword ?? "";

        ValidateRegisterFields(
            fullName,
            email,
            password,
            confirmPassword,
            dto.AcceptedTerms,
            errors
        );

        var passwordSuggestions = _passwordValidator.GetPasswordSuggestions(password);

        if (passwordSuggestions.Any())
        {
            foreach (var suggestion in passwordSuggestions)
                AddError(errors, "password", suggestion);
        }

        if (errors.Any())
        {
            return AuthResult<AuthResponseDto>.Fail(
                400,
                "يرجى تصحيح الأخطاء التالية.",
                errors,
                passwordSuggestions
            );
        }

        var emailExists = await _db.users
            .AsNoTracking()
            .AnyAsync(user => user.email == email);

        if (emailExists)
        {
            AddError(
                errors,
                "email",
                "هذا البريد الإلكتروني مسجل بالفعل. يرجى استخدام بريد آخر أو تسجيل الدخول."
            );

            return AuthResult<AuthResponseDto>.Fail(
                400,
                "البريد الإلكتروني مستخدم مسبقًا.",
                errors
            );
        }

        var newUser = new user
        {
            full_name = fullName,
            email = email,
            password_hash = BCrypt.Net.BCrypt.HashPassword(password),
            role = "student",
            is_active = true,
            onboarding_completed = false
        };

        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            _db.users.Add(newUser);
            await _db.SaveChangesAsync();

            var token = _jwtTokenService.GenerateToken(newUser);

            await transaction.CommitAsync();

            var response = new AuthResponseDto
            {
                Token = token,
                UserId = newUser.id,
                FullName = newUser.full_name,
                Role = newUser.role,
                IsOnboardingCompleted = false,
                Branch = null
            };

            return AuthResult<AuthResponseDto>.Ok(
                response,
                "تم إنشاء الحساب بنجاح."
            );
        }
        catch
        {
            await transaction.RollbackAsync();

            return AuthResult<AuthResponseDto>.Fail(
                500,
                "حدث خطأ غير متوقع أثناء إنشاء الحساب. يرجى المحاولة مرة أخرى."
            );
        }
    }

    public async Task<AuthResult<AuthResponseDto>> LoginAsync(LoginRequestDto dto)
    {
        if (dto == null)
        {
            return AuthResult<AuthResponseDto>.Fail(
                400,
                "الطلب غير صالح."
            );
        }

        var errors = new Dictionary<string, List<string>>();

        var email = dto.Email?.Trim().ToLower() ?? "";
        var password = dto.Password ?? "";

        if (string.IsNullOrWhiteSpace(email))
            AddError(errors, "email", "البريد الإلكتروني مطلوب.");

        if (string.IsNullOrWhiteSpace(password))
            AddError(errors, "password", "كلمة المرور مطلوبة.");

        if (errors.Any())
        {
            return AuthResult<AuthResponseDto>.Fail(
                400,
                "يرجى إدخال البريد الإلكتروني وكلمة المرور.",
                errors
            );
        }

        var user = await _db.users.FirstOrDefaultAsync(x => x.email == email);

        if (user == null)
        {
            return AuthResult<AuthResponseDto>.Fail(
                401,
                "البريد الإلكتروني أو كلمة المرور غير صحيحة."
            );
        }

        if (user.is_active == false)
        {
            return AuthResult<AuthResponseDto>.Fail(
                401,
                "هذا الحساب معطل. يرجى التواصل مع الدعم."
            );
        }

        bool isPasswordValid;

        try
        {
            isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.password_hash);
        }
        catch
        {
            return AuthResult<AuthResponseDto>.Fail(
                400,
                "حدث خطأ أثناء التحقق من كلمة المرور."
            );
        }

        if (!isPasswordValid)
        {
            return AuthResult<AuthResponseDto>.Fail(
                401,
                "البريد الإلكتروني أو كلمة المرور غير صحيحة."
            );
        }

        var response = await BuildAuthResponseAsync(user, dto.RememberMe);

        return AuthResult<AuthResponseDto>.Ok(
            response,
            "تم تسجيل الدخول بنجاح."
        );
    }

    public async Task<AuthResult<AuthResponseDto>> GoogleLoginAsync(GoogleLoginRequestDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.IdToken))
        {
            return AuthResult<AuthResponseDto>.Fail(
                400,
                "تعذر تسجيل الدخول باستخدام جوجل. يرجى المحاولة مرة أخرى."
            );
        }

        GoogleUserInfo? googleUser;

        try
        {
            googleUser = await _googleTokenValidator.ValidateAsync(dto.IdToken);
        }
        catch
        {
            return AuthResult<AuthResponseDto>.Fail(
                500,
                "حدث خطأ أثناء التحقق من حساب جوجل. تأكد من إعدادات Google Client ID."
            );
        }

        if (googleUser == null)
        {
            return AuthResult<AuthResponseDto>.Fail(
                401,
                "فشل التحقق من حساب جوجل. تأكد من اختيار الحساب الصحيح والمحاولة مرة أخرى."
            );
        }

        var user = await _db.users.FirstOrDefaultAsync(x => x.email == googleUser.Email);

        if (user != null)
        {
            if (user.is_active == false)
            {
                return AuthResult<AuthResponseDto>.Fail(
                    401,
                    "هذا الحساب معطل. يرجى التواصل مع الدعم."
                );
            }

            var existingUserResponse = await BuildAuthResponseAsync(user, rememberMe: false);

            return AuthResult<AuthResponseDto>.Ok(
                existingUserResponse,
                "تم تسجيل الدخول باستخدام جوجل بنجاح."
            );
        }

        var generatedPassword = $"{Guid.NewGuid():N}-{googleUser.GoogleUserId}";

        var newUser = new user
        {
            full_name = googleUser.FullName,
            email = googleUser.Email,
            password_hash = BCrypt.Net.BCrypt.HashPassword(generatedPassword),
            role = "student",
            is_active = true,
            onboarding_completed = false
        };

        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            _db.users.Add(newUser);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            var newUserResponse = await BuildAuthResponseAsync(newUser, rememberMe: false);

            return AuthResult<AuthResponseDto>.Ok(
                newUserResponse,
                "تم إنشاء الحساب وتسجيل الدخول باستخدام جوجل بنجاح."
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            Console.WriteLine("Google login database error: " + ex.Message);

            return AuthResult<AuthResponseDto>.Fail(
                500,
                "حدث خطأ أثناء تسجيل الدخول باستخدام جوجل. يرجى المحاولة مرة أخرى."
            );
        }
    }

    public async Task<bool> RequestPasswordResetAsync(string email)
    {
        var normalizedEmail = NormalizeEmail(email);

        if (string.IsNullOrWhiteSpace(normalizedEmail))
            return false;

        var user = await _db.users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.email == normalizedEmail && user.is_active == true);

        if (user == null)
            return false;

        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        _memoryCache.Set(GetOtpCacheKey(normalizedEmail), otp, TimeSpan.FromMinutes(3));
        _memoryCache.Remove(GetVerifiedCacheKey(normalizedEmail));

        await _emailSender.SendPasswordResetOtpAsync(
            normalizedEmail,
            user.full_name,
            otp,
            CancellationToken.None
        );

        return true;
    }

    public Task<bool> VerifyOtpAsync(string email, string otp)
    {
        var normalizedEmail = NormalizeEmail(email);

        if (
            string.IsNullOrWhiteSpace(normalizedEmail) ||
            string.IsNullOrWhiteSpace(otp) ||
            !_memoryCache.TryGetValue(GetOtpCacheKey(normalizedEmail), out string? cachedOtp) ||
            cachedOtp != otp.Trim()
        )
        {
            return Task.FromResult(false);
        }

        _memoryCache.Remove(GetOtpCacheKey(normalizedEmail));
        _memoryCache.Set(GetVerifiedCacheKey(normalizedEmail), true, TimeSpan.FromMinutes(10));

        return Task.FromResult(true);
    }

    public async Task<bool> ResetPasswordAsync(string email, string newPassword)
    {
        var normalizedEmail = NormalizeEmail(email);

        if (
            string.IsNullOrWhiteSpace(normalizedEmail) ||
            !_memoryCache.TryGetValue(GetVerifiedCacheKey(normalizedEmail), out bool isVerified) ||
            !isVerified ||
            _passwordValidator.GetPasswordSuggestions(newPassword).Any()
        )
        {
            return false;
        }

        var user = await _db.users.FirstOrDefaultAsync(user =>
            user.email == normalizedEmail &&
            user.is_active == true
        );

        if (user == null)
            return false;

        user.password_hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _db.SaveChangesAsync();

        _memoryCache.Remove(GetVerifiedCacheKey(normalizedEmail));

        return true;
    }

    private async Task<AuthResponseDto> BuildAuthResponseAsync(user user, bool rememberMe = false)
    {
        var profile = await _db.student_profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(profile => profile.user_id == user.id);

        var token = _jwtTokenService.GenerateToken(user);
        string? refreshToken = null;

        if (rememberMe)
        {
            refreshToken = await GenerateRefreshTokenAsync(user.id);
        }

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            UserId = user.id,
            FullName = user.full_name,
            Role = user.role,
            IsOnboardingCompleted = user.onboarding_completed,
            Branch = profile?.stream
        };
    }

    private async Task<string> GenerateRefreshTokenAsync(Guid userId)
    {
        var refreshTokenValue = Guid.NewGuid().ToString("N");
        var tokenHash = BCrypt.Net.BCrypt.HashPassword(refreshTokenValue);
        var expiresAt = DateTime.UtcNow.AddDays(14);// صلاحية الـ refresh token لمدة 14 يومًا

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        return refreshTokenValue;
    }

    public async Task<AuthResult<AuthResponseDto>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return AuthResult<AuthResponseDto>.Fail(
                400,
                "الرمز غير صالح."
            );
        }

        // البحث عن جميع الـ tokens غير المنتهية الصلاحية والغير مُلغاة
        var activeTokens = await _db.RefreshTokens
            .AsNoTracking()
            .Where(rt => rt.ExpiresAt > DateTime.UtcNow && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        // البحث عن الـ token الذي يطابق الـ hash
        RefreshToken? storedToken = null;
        foreach (var token in activeTokens)
        {
            try
            {
                if (BCrypt.Net.BCrypt.Verify(request.RefreshToken, token.TokenHash))
                {
                    storedToken = token;
                    break;
                }
            }
            catch
            {
                // استمر مع الـ token التالي
                continue;
            }
        }

        if (storedToken == null)
        {
            return AuthResult<AuthResponseDto>.Fail(
                401,
                "رمز التحديث غير صالح أو منتهي الصلاحية."
            );
        }

        // الحصول على المستخدم
        var user = await _db.users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.id == storedToken.UserId, cancellationToken: cancellationToken);

        if (user == null || user.is_active == false)
        {
            return AuthResult<AuthResponseDto>.Fail(
                401,
                "هذا الحساب معطل أو غير موجود."
            );
        }

        // إلغاء الـ token القديم وإنشاء واحد جديد
        storedToken.RevokedAt = DateTime.UtcNow;
        _db.RefreshTokens.Update(storedToken);
        await _db.SaveChangesAsync(cancellationToken);

        var response = await BuildAuthResponseAsync(user, rememberMe: true);

        return AuthResult<AuthResponseDto>.Ok(
            response,
            "تم تحديث الرمز بنجاح."
        );
    }

    private void ValidateRegisterFields(
        string fullName,
        string email,
        string password,
        string confirmPassword,
        bool acceptedTerms,
        Dictionary<string, List<string>> errors)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            AddError(errors, "fullName", "الاسم الكامل مطلوب.");
        }
        else
        {
            if (fullName.Length < 3)
                AddError(errors, "fullName", "الاسم الكامل يجب أن يكون 3 أحرف على الأقل.");

            if (fullName.Length > 100)
                AddError(errors, "fullName", "الاسم الكامل طويل جدًا.");

            if (!Regex.IsMatch(fullName, @"^[\p{L}\s'-]+$"))
                AddError(errors, "fullName", "الاسم الكامل يجب أن يحتوي على أحرف فقط.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            AddError(errors, "email", "البريد الإلكتروني مطلوب.");
        }
        else if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            AddError(errors, "email", "صيغة البريد الإلكتروني غير صحيحة.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            AddError(errors, "password", "كلمة المرور مطلوبة.");
        }

        if (string.IsNullOrWhiteSpace(confirmPassword))
        {
            AddError(errors, "confirmPassword", "تأكيد كلمة المرور مطلوب.");
        }
        else if (password != confirmPassword)
        {
            AddError(errors, "confirmPassword", "كلمتا المرور غير متطابقتين.");
        }

        if (!acceptedTerms)
        {
            AddError(errors, "acceptedTerms", "يجب الموافقة على الشروط والأحكام لإنشاء الحساب.");
        }
    }

    private void AddError(
        Dictionary<string, List<string>> errors,
        string field,
        string message)
    {
        if (!errors.ContainsKey(field))
            errors[field] = new List<string>();

        errors[field].Add(message);
    }

    private static string NormalizeEmail(string? email)
    {
        return email?.Trim().ToLowerInvariant() ?? "";
    }

    private static string GetOtpCacheKey(string email)
    {
        return $"password-reset:otp:{email}";
    }

    private static string GetVerifiedCacheKey(string email)
    {
        return $"password-reset:verified:{email}";
    }
}
