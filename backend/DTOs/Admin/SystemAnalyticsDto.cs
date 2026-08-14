namespace backend.DTOs.Admin
{
public record SystemAnalyticsDto(
    double SuccessRate, // نسبة النجاح
    string MostInteractiveSubject, // أكثر مادة تفاعلاً
    string MostCompletedLesson, // أكثر درس تم إكماله
    List<SubjectPerformanceReport> PerformanceReports // تقارير عامة
);
public record SubjectPerformanceReport(string SubjectName, double AvgScore, int StudentCount);}