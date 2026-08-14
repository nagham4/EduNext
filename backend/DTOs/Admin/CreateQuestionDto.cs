using System;
using System.ComponentModel.DataAnnotations;
namespace backend.DTOs.Admin{   
public record CreateQuestionDto(
    [Required] string Text,
    [Required] string OptionA,
    [Required] string OptionB,
    [Required] string OptionC,
    [Required] string OptionD,
    [Required] string CorrectAnswer // تخزن فيه الرمز (A, B, C, أو D)
);}