namespace backend.DTOs.Admin;

public record ExamResultSummaryDto(
    Guid ResultId,
    string StudentName,
    string ExamTitle,
    double Score,
    DateTime Date,
    string PerformanceLevel // ممتاز، جيد، ضعيف
);