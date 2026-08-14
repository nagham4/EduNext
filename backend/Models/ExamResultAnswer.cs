using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class ExamResultAnswer
{
    public Guid Id { get; set; }

    public Guid? ExamResultId { get; set; }

    public Guid? QuestionId { get; set; }

    public string? SelectedAnswer { get; set; }

    public bool? IsCorrect { get; set; }

    public virtual ExamResult? ExamResult { get; set; }

    public virtual Question? Question { get; set; }
}
