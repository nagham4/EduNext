using System.Security.Cryptography;
using System.Text;
using backend.Data.Generated;
using backend.DTOs.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Admin;

public class AdminProfileRepository : IAdminProfileRepository
{
    private readonly AppDbContext _context;

    public AdminProfileRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminProfileDto?> GetProfileAsync(Guid adminId)
    {
        var admin = await _context.users
            .AsNoTracking()
            .Where(u => u.id == adminId && u.role != null && u.role.ToLower() == "admin")
            .Select(u => new
            {
                u.id,
                u.full_name,
                u.email,
                u.phone,
                u.role
            })
            .FirstOrDefaultAsync();

        if (admin == null)
        {
            return null;
        }

        return new AdminProfileDto
        {
            Id = admin.id,
            FullName = admin.full_name ?? "",
            Email = admin.email,
            Phone = admin.phone ?? "",
            Role = admin.role,
            RoleLabel = "مسؤول",
            Branch = "",
            AcademicYear = "",
            ActivityHistory = new List<AdminProfileActivityGroupDto>
            {
                new AdminProfileActivityGroupDto
                {
                    DateLabel = "اليوم",
                    Items = new List<AdminProfileActivityItemDto>
                    {
                        new AdminProfileActivityItemDto
                        {
                            Type = "achievement",
                            Text = "تم فتح لوحة تحكم الأدمن",
                            Time = DateTime.Now.ToString("HH:mm"),
                            Color = "purple"
                        }
                    }
                }
            }
        };
    }

    public async Task<AdminProfileDto?> UpdateProfileAsync(Guid adminId, UpdateAdminProfileDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
        {
            return null;
        }

        string? cleanPhone = null;

        if (!string.IsNullOrWhiteSpace(dto.Phone))
        {
            var phone = dto.Phone.Trim().Replace(" ", "").Replace("-", "");

            if (phone.StartsWith("0"))
                phone = "+970" + phone.Substring(1);

            if (!IsValidInternationalPhone(phone))
                throw new ArgumentException("رقم الهاتف يجب أن يكون بصيغة دولية مثل: +970599123456");

            cleanPhone = phone;
        }

        var admin = await _context.users
            .FirstOrDefaultAsync(u => u.id == adminId && u.role != null && u.role.ToLower() == "admin");

        if (admin == null)
        {
            return null;
        }

        admin.full_name = dto.FullName.Trim();
        admin.phone = cleanPhone;

        await _context.SaveChangesAsync();

        return await GetProfileAsync(adminId);
    }

    private static bool IsValidInternationalPhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        return System.Text.RegularExpressions.Regex.IsMatch(value, @"^\+[1-9]\d{7,14}$");
    }

    public async Task<bool> ChangePasswordAsync(Guid adminId, ChangeAdminPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
            string.IsNullOrWhiteSpace(dto.NewPassword) ||
            dto.NewPassword != dto.ConfirmNewPassword ||
            dto.NewPassword.Length < 6)
        {
            return false;
        }

        var admin = await _context.users
            .FirstOrDefaultAsync(u => u.id == adminId && u.role != null && u.role.ToLower() == "admin");

        if (admin == null)
        {
            return false;
        }

        if (!VerifyStoredPassword(dto.CurrentPassword, admin.password_hash))
        {
            return false;
        }

        admin.password_hash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAccountAsync(Guid adminId)
    {
        var admin = await _context.users
            .FirstOrDefaultAsync(u => u.id == adminId && u.role != null && u.role.ToLower() == "admin");

        if (admin == null)
        {
            return false;
        }

        admin.is_active = false;

        await _context.SaveChangesAsync();

        return true;
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