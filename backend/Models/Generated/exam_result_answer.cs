using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class exam_result_answer
{
    public Guid id { get; set; }

    public Guid? exam_result_id { get; set; }

    public Guid? question_id { get; set; }

    public string? selected_answer { get; set; }

    public bool? is_correct { get; set; }

    public virtual exam_result? exam_result { get; set; }

    public virtual question? question { get; set; }
}
