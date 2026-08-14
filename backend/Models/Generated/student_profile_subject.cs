using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class student_profile_subject
{
    public Guid id { get; set; }

    public Guid student_profile_id { get; set; }

    public Guid subject_id { get; set; }

    public DateTime? created_at { get; set; }

    public virtual student_profile student_profile { get; set; } = null!;

    public virtual subject subject { get; set; } = null!;
}
