using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class exam_result
{
    public Guid id { get; set; }

    public Guid? user_id { get; set; }

    public Guid? exam_id { get; set; }

    public int? score { get; set; }

    public string? strength_points { get; set; }

    public string? weakness_points { get; set; }

    public string? level_message { get; set; }

    public DateTime? created_at { get; set; }

    public virtual exam? exam { get; set; }

    public virtual ICollection<exam_result_answer> exam_result_answers { get; set; } = new List<exam_result_answer>();

    public virtual user? user { get; set; }
}
