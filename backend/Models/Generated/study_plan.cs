using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class study_plan
{
    public Guid id { get; set; }

    public Guid? user_id { get; set; }

    public string? title { get; set; }

    public string? description { get; set; }

    public bool? is_ai_generated { get; set; }

    public DateTime? created_at { get; set; }

    public Guid? subject_id { get; set; }

    public List<string>? study_days { get; set; }

    public int? daily_duration_minutes { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<study_plan_item> study_plan_items { get; set; } = new List<study_plan_item>();

    public virtual subject? subject { get; set; }

    public virtual user? user { get; set; }
}
