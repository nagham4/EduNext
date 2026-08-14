using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Admin
{
    public record UpdateSubjectDto(
    [Required, MaxLength(150)]
     string Title,
    [MaxLength(500)] string Description
);
}