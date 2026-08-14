using backend.DTOs.Student;

namespace backend.Services.Student;

public interface IStudentProfileService
{
    Task<ProfileDto> GetProfileAsync(Guid userId);

    Task UpdateProfileAsync(Guid userId, UpdateProfileDto dto);

    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);

    Task DeleteAccountAsync(Guid userId);
}