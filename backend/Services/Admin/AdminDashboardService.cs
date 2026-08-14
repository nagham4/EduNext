using backend.DTOs.Admin;
using backend.Repositories.Admin;

namespace backend.Services.Admin;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly IAdminDashboardRepository _dashboardRepository;

    public AdminDashboardService(IAdminDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<AdminDashboardDto> GetDashboardAsync(Guid adminId)
    {
        var adminName = await _dashboardRepository.GetAdminNameAsync(adminId);

        var totalStudents = await _dashboardRepository.GetTotalStudentsAsync();
        var totalSubjects = await _dashboardRepository.GetTotalSubjectsAsync();
        var totalLessons = await _dashboardRepository.GetTotalLessonsAsync();
        var totalExams = await _dashboardRepository.GetTotalExamsAsync();
        var completedExams = await _dashboardRepository.GetCompletedExamsAsync();

        var newStudentsThisMonth = await _dashboardRepository.GetNewStudentsThisMonthAsync();
        var newSubjectsThisMonth = await _dashboardRepository.GetNewSubjectsThisMonthAsync();
        var newLessonsThisMonth = await _dashboardRepository.GetNewLessonsThisMonthAsync();
        var completedExamsThisMonth = await _dashboardRepository.GetCompletedExamsThisMonthAsync();

        var mostActiveSubjects = await _dashboardRepository.GetMostActiveSubjectsAsync(10);
        var recentActivities = await _dashboardRepository.GetRecentActivitiesAsync(12);
        var studentPerformanceTrends = await _dashboardRepository.GetStudentPerformanceTrendsAsync(7);

        return new AdminDashboardDto
        {
            Header = new AdminDashboardHeaderDto
            {
                AdminName = adminName,
                LastLoginMessage = "إليك نظرة سريعة على ما يحدث في المنصة اليوم."
            },

            Summary = new AdminDashboardSummaryDto
            {
                TotalStudents = totalStudents,
                TotalSubjects = totalSubjects,
                TotalLessons = totalLessons,
                TotalExams = totalExams,
                CompletedExams = completedExams,

                NewStudentsThisMonth = newStudentsThisMonth,
                NewSubjectsThisMonth = newSubjectsThisMonth,
                NewLessonsThisMonth = newLessonsThisMonth,
                CompletedExamsThisMonth = completedExamsThisMonth
            },

            LessonCompletion = new AdminLessonCompletionDto(),

            MostActiveSubjects = mostActiveSubjects,

            RecentActivities = recentActivities,

            StudentPerformanceTrends = studentPerformanceTrends,

            HasData = totalStudents > 0 ||
                      totalSubjects > 0 ||
                      totalLessons > 0 ||
                      totalExams > 0
        };
    }
}