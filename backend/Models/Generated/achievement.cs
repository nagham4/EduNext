using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class achievement
{
    public Guid id { get; set; }

    public string? title { get; set; }

    public string? description { get; set; }

    public string? condition_type { get; set; }

    public int? condition_value { get; set; }

    public string? title_ar { get; set; }

    public string? title_en { get; set; }

    public string? description_ar { get; set; }

    public string? description_en { get; set; }

    public string? reward_type { get; set; }

    public int? reward_value { get; set; }

    public bool? is_active { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<user_achievement> user_achievements { get; set; } = new List<user_achievement>();
}