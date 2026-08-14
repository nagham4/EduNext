using backend.DTOs.Student;

namespace backend.Services.Student;

public interface IStudentDashboardService
{
    Task<StudentDashboardDto> GetDashboardAsync(Guid userId);
}