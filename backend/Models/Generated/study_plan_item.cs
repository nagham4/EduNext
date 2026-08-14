using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class study_plan_item
{
    public Guid id { get; set; }

    public Guid study_plan_id { get; set; }

    public Guid lesson_id { get; set; }

    public int? order_number { get; set; }

    public bool? is_completed { get; set; }

    public DateTime? created_at { get; set; }

    public virtual lesson lesson { get; set; } = null!;

    public virtual study_plan study_plan { get; set; } = null!;
}
