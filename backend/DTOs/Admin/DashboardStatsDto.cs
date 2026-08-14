namespace backend.DTOs.Admin
{
    public record DashboardStatsDto(
    int TotalStudents,
    int TotalSubjects,
    int TotalLessons,
    int TotalExams,
    int TotalSubmittedExams,
    double AverageScore,
    double OverallSuccessRate, // نسبة نجاح الطلاب
    double LessonCompletionRate, // معدل إكمال الدروس
    string MostActiveSubject // أكثر مادة تفاعلاً
);
}