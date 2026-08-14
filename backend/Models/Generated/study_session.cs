using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class study_session
{
    public Guid id { get; set; }

    public Guid user_id { get; set; }

    public Guid? subject_id { get; set; }

    public Guid? lesson_id { get; set; }

    public DateTime started_at { get; set; }

    public DateTime? ended_at { get; set; }

    public int duration_minutes { get; set; }

    public string session_type { get; set; } = null!;

    public DateTime created_at { get; set; }

    public virtual lesson? lesson { get; set; }

    public virtual subject? subject { get; set; }

    public virtual user user { get; set; } = null!;
}
