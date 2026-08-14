using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class student_preference_difficult_subject
{
    public Guid user_id { get; set; }

    public Guid subject_id { get; set; }

    public DateTime created_at { get; set; }

    public virtual subject subject { get; set; } = null!;

    public virtual student_preference user { get; set; } = null!;
}
