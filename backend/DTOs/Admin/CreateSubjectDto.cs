using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Admin
{
    public record CreateSubjectDto(
    [Required, MaxLength(150)] 
    string Title,
    [MaxLength(500)] 
    string Description
);
}