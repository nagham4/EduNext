using backend.DTOs.Admin;

namespace backend.Repositories.Admin;

public interface IAdminDashboardRepository
{
    Task<string> GetAdminNameAsync(Guid adminId);

    Task<int> GetTotalStudentsAsync();

    Task<int> GetTotalSubjectsAsync();

    Task<int> GetTotalLessonsAsync();

    Task<int> GetTotalExamsAsync();

    Task<int> GetCompletedExamsAsync();

    Task<int> GetNewStudentsThisMonthAsync();

    Task<int> GetNewSubjectsThisMonthAsync();

    Task<int> GetNewLessonsThisMonthAsync();

    Task<int> GetCompletedExamsThisMonthAsync();

    Task<AdminLessonCompletionDto> GetLessonCompletionAsync();

    Task<List<AdminMostActiveSubjectDto>> GetMostActiveSubjectsAsync(int limit);

    Task<List<AdminRecentActivityDto>> GetRecentActivitiesAsync(int limit);

    Task<List<AdminStudentPerformanceTrendDto>> GetStudentPerformanceTrendsAsync(int days);
}