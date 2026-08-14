using System.Security.Cryptography;
using System.Text;
using backend.DTOs.Student;
using backend.Repositories.Student;
using Microsoft.AspNetCore.Identity;

namespace backend.Services.Student;

public class StudentProfileService : IStudentProfileService
{
    private readonly IStudentProfileRepository _repository;

    public StudentProfileService(IStudentProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _repository.GetUserWithProfileAsync(userId);

        if (user == null)
        {
            throw new InvalidOperationException("المستخدم غير موجود.");
        }

        var lessonActivities = await _repository.GetLessonActivitiesAsync(userId, 10);
        var examActivities = await _repository.GetExamActivitiesAsync(userId, 10);
        var achievementActivities = await _repository.GetAchievementActivitiesAsync(userId, 10);

        var allActivities = lessonActivities
            .Concat(examActivities)
            .Concat(achievementActivities)
            .OrderByDescending(x => x.Date)
            .Take(12)
            .ToList();

        var activityGroups = allActivities
            .GroupBy(x => GetArabicDateLabel(x.Date))
            .Select(g => new ActivityGroupDto
            {
                DateLabel = g.Key,
                Items = g.Select(x => new ActivityItemDto
                {
                    Type = x.Type,
                    Text = x.Text,
                    Time = x.Date.ToString("hh:mm tt"),
                    Color = x.Color
                }).ToList()
            })
            .ToList();

        return new ProfileDto
        {
            Id = user.id,
            FullName = user.full_name ?? "",
            Email = user.email ?? "",
            Phone = user.phone,
            Role = user.role ?? "",
            Branch = MapBranchToArabic(user.student_profile?.stream),
            AcademicYear = user.student_profile?.exam_year != null
                ? $"توجيهي {ToArabicNumber(user.student_profile.exam_year.Value)}"
                : null,
            ActivityHistory = activityGroups
        };
    }

    public async Task UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var user = await _repository.GetUserForUpdateAsync(userId);

        if (user == null)
        {
            throw new InvalidOperationException("المستخدم غير موجود.");
        }

        if (!string.IsNullOrWhiteSpace(dto.FullName))
        {
            var cleanName = dto.FullName.Trim();

            if (cleanName.Length < 3)
            {
                throw new ArgumentException("الاسم يجب أن يكون 3 أحرف على الأقل.");
            }

            user.full_name = cleanName;
        }

        /*
         * البريد الإلكتروني لا يتم تعديله من صفحة الملف الشخصي.
         * الفرع الدراسي أيضاً لا يتم تعديله من هنا.
         */

        user.phone = string.IsNullOrWhiteSpace(dto.Phone)
            ? null
            : dto.Phone.Trim();

        await _repository.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var user = await _repository.GetUserForUpdateAsync(userId);

        if (user == null)
        {
            throw new InvalidOperationException("المستخدم غير موجود.");
        }

        if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
        {
            throw new ArgumentException("كلمة المرور الحالية مطلوبة.");
        }

        if (string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            throw new ArgumentException("كلمة المرور الجديدة مطلوبة.");
        }

        if (dto.NewPassword.Length < 8)
        {
            throw new ArgumentException("كلمة المرور الجديدة يجب أن تكون 8 أحرف على الأقل.");
        }

        if (dto.NewPassword != dto.ConfirmNewPassword)
        {
            throw new ArgumentException("كلمة المرور الجديدة وتأكيدها غير متطابقتين.");
        }

        if (VerifyStoredPassword(dto.CurrentPassword, user.password_hash))
        {
            user.password_hash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _repository.SaveChangesAsync();
            return;
        }

        bool isCurrentPasswordCorrect;
        try
        {
            isCurrentPasswordCorrect = BCrypt.Net.BCrypt.Verify(
                dto.CurrentPassword,
                user.password_hash
            );
        }
        catch
        {
            isCurrentPasswordCorrect = false;
        }

        if (!isCurrentPasswordCorrect)
        {
            throw new ArgumentException("كلمة المرور الحالية غير صحيحة.");
        }

        bool isSamePassword;
        try
        {
            isSamePassword = BCrypt.Net.BCrypt.Verify(
                dto.NewPassword,
                user.password_hash
            );
        }
        catch
        {
            isSamePassword = false;
        }

        if (isSamePassword)
        {
            throw new ArgumentException("كلمة المرور الجديدة يجب أن تكون مختلفة عن الحالية.");
        }

        user.password_hash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        await _repository.SaveChangesAsync();
    }

    public async Task DeleteAccountAsync(Guid userId)
    {
        var user = await _repository.GetUserForUpdateAsync(userId);

        if (user == null)
        {
            throw new InvalidOperationException("المستخدم غير موجود.");
        }

        /*
         * حذف آمن:
         * لا نحذف السجل فعلياً من الداتا بيس حتى لا نخرب العلاقات
         * مثل exam_results و lesson_progress و study_plans.
         */
        user.is_active = false;

        await _repository.SaveChangesAsync();
    }

    private static string GetArabicDateLabel(DateTime date)
    {
        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);
        var twoDaysAgo = today.AddDays(-2);

        if (date.Date == today)
        {
            return "اليوم";
        }

        if (date.Date == yesterday)
        {
            return "أمس";
        }

        if (date.Date == twoDaysAgo)
        {
            return "منذ يومين";
        }

        return date.ToString("yyyy-MM-dd");
    }

    private static string MapBranchToArabic(string? branch)
    {
        return branch switch
        {
            "scientific" => "العلمي",
            "literary" => "الأدبي",
            "industrial" => "الصناعي",
            "commercial" => "التجاري",
            "sharia" => "الشرعي",

            "العلمي" => "العلمي",
            "الأدبي" => "الأدبي",
            "الصناعي" => "الصناعي",
            "التجاري" => "التجاري",
            "الشرعي" => "الشرعي",

            _ => branch ?? ""
        };
    }

    private static string ToArabicNumber(int number)
    {
        return number.ToString()
            .Replace('0', '٠')
            .Replace('1', '١')
            .Replace('2', '٢')
            .Replace('3', '٣')
            .Replace('4', '٤')
            .Replace('5', '٥')
            .Replace('6', '٦')
            .Replace('7', '٧')
            .Replace('8', '٨')
            .Replace('9', '٩');
    }

    private static bool VerifyStoredPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        try
        {
            if (BCrypt.Net.BCrypt.Verify(password, passwordHash))
            {
                return true;
            }
        }
        catch
        {
            // Try legacy hash formats below.
        }

        try
        {
            var identityHasher = new PasswordHasher<object>();
            var result = identityHasher.VerifyHashedPassword(
                new object(),
                passwordHash,
                password
            );

            if (result != PasswordVerificationResult.Failed)
            {
                return true;
            }
        }
        catch
        {
            // Try legacy PBKDF2 format below.
        }

        if (VerifyPbkdf2Password(password, passwordHash))
        {
            return true;
        }

        return VerifyLegacyPlainTextPassword(password, passwordHash);
    }

    private static bool VerifyPbkdf2Password(string password, string passwordHash)
    {
        try
        {
            var parts = passwordHash.Split('$');
            if (parts.Length != 4 || parts[0] != "PBKDF2" || !int.TryParse(parts[1], out var iterations))
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[2]);
            var storedHash = Convert.FromBase64String(parts[3]);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                storedHash.Length
            );

            return CryptographicOperations.FixedTimeEquals(hash, storedHash);
        }
        catch
        {
            return false;
        }
    }

    private static bool VerifyLegacyPlainTextPassword(string password, string passwordHash)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var passwordHashBytes = Encoding.UTF8.GetBytes(passwordHash);

        return passwordBytes.Length == passwordHashBytes.Length &&
            CryptographicOperations.FixedTimeEquals(passwordBytes, passwordHashBytes);
    }
}
