using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class user_stat
{
    public Guid user_id { get; set; }

    public int points { get; set; }

    public int level { get; set; }

    public int streak_days { get; set; }

    public DateOnly? last_activity_date { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual user user { get; set; } = null!;
}
