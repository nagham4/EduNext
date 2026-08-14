using backend.DTOs.Student;

namespace backend.Services.Student;

public interface IStudentAnalyticsService
{
    Task<StudentAnalyticsDto> GetAnalyticsAsync(Guid userId);
}