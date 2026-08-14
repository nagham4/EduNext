using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Student;

public class SetupSubjectsDto
{
    [Required]
    public List<Guid> SubjectIds { get; set; } = new();
}