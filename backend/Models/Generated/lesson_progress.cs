using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class lesson_progress
{
    public Guid id { get; set; }

    public Guid? user_id { get; set; }

    public Guid? lesson_id { get; set; }

    public bool? completed { get; set; }

    public DateTime? completed_at { get; set; }

    public virtual lesson? lesson { get; set; }

    public virtual user? user { get; set; }
}
