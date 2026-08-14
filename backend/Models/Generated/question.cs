using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class question
{
    public Guid id { get; set; }

    public Guid? exam_id { get; set; }

    public string question_text { get; set; } = null!;

    public string? option_a { get; set; }

    public string? option_b { get; set; }

    public string? option_c { get; set; }

    public string? option_d { get; set; }

    public string? correct_answer { get; set; }

    public string? solution_text { get; set; }

    public virtual exam? exam { get; set; }

    public virtual ICollection<exam_result_answer> exam_result_answers { get; set; } = new List<exam_result_answer>();
}
