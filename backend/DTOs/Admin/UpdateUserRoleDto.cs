using System.ComponentModel.DataAnnotations;
using backend.Models.Enums;

namespace backend.DTOs.Admin
{
   public record UpdateUserRoleDto(
    [Required] UserRole Role
);
}