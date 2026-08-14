using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class user_achievement
{
    public Guid id { get; set; }

    public Guid? user_id { get; set; }

    public Guid? achievement_id { get; set; }

    public DateTime? earned_at { get; set; }

    public virtual achievement? achievement { get; set; }

    public virtual user? user { get; set; }
}
