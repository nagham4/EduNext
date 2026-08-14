using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class ExamResult
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public Guid? ExamId { get; set; }

    public int? Score { get; set; }

    public string? StrengthPoints { get; set; }

    public string? WeaknessPoints { get; set; }

    public string? LevelMessage { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Exam? Exam { get; set; }

    public virtual ICollection<ExamResultAnswer> ExamResultAnswers { get; set; } = new List<ExamResultAnswer>();

    public virtual User? User { get; set; }
}
