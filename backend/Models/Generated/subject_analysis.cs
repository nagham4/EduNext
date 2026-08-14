using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class subject_analysis
{
    public Guid id { get; set; }

    public Guid? user_id { get; set; }

    public Guid? subject_id { get; set; }

    public string? strengths { get; set; }

    public string? weaknesses { get; set; }

    public string? improvement_tip { get; set; }

    public virtual subject? subject { get; set; }

    public virtual user? user { get; set; }
}
