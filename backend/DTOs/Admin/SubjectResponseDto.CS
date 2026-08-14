namespace backend.DTOs.Admin{
// DTO جديد لعرض المادة مع إحصائياتها (مطلوب في الـ Dashboard)
public record SubjectResponseDto(
    Guid Id, 
    string Title, 
    int LessonsCount, 
    int ExamsCount
);}