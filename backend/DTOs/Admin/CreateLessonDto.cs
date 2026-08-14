using System;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Admin
{
    public record CreateLessonDto
    (
        [Required] 
        Guid SubjectId,
    [Required, MaxLength(150)] 
    string Title,
    [MaxLength(500)] 
    string VideoUrl, // رابط الفيديو
    string Summary, // ملخص الدرس
    string Content, // محتوى الدرس
    [Range(1, int.MaxValue)]
     int OrderNumber // رقم ترتيب الدرس
    );
}