using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Question
{
    public Guid Id { get; set; }

    public Guid? ExamId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string? OptionA { get; set; }

    public string? OptionB { get; set; }

    public string? OptionC { get; set; }

    public string? OptionD { get; set; }

    public string? CorrectAnswer { get; set; }

    public string? SolutionText { get; set; }

    public virtual Exam? Exam { get; set; }

    public virtual ICollection<ExamResultAnswer> ExamResultAnswers { get; set; } = new List<ExamResultAnswer>();
}
