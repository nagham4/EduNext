using backend.DTOs.Student;

namespace backend.Services.Student;

public interface IStudentSetupService
{
    Task<OnboardingOptionsDto> GetOnboardingOptionsAsync();

    Task<StudentSetupDto> GetAsync(Guid userId);

    Task<StudentSetupDto> SaveBasicInfoAsync(Guid userId, SetupBasicInfoDto dto);

    Task<StudentSetupDto> SaveSubjectsAsync(Guid userId, SetupSubjectsDto dto);

    Task<StudentSetupDto> SaveStudyPreferencesAsync(Guid userId, SetupStudyPreferencesDto dto);

    Task<StudentSetupDto> CompleteAsync(Guid userId);

    Task<StudentSetupDto> CompleteOnboardingAsync(Guid userId, CompleteOnboardingDto dto);
}