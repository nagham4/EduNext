using System;
using System.ComponentModel.DataAnnotations;
using backend.Models.Enums;

namespace backend.DTOs.Admin
{
    public record CreateExamDto(
    [Required] string Title,
    [Required] Guid SubjectId,
    Guid? LessonId, // اختياري إذا كان امتحان لدرس معين
    [Required] ExamType Type, // شامل أو قصير
    [Required, MinLength(1, ErrorMessage = "الامتحان لازم يحتوي على سؤال واحد على الأقل")] 
    List<CreateQuestionDto> Questions
);
}