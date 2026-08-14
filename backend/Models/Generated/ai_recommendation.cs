using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class ai_recommendation
{
    public Guid id { get; set; }

    public Guid? user_id { get; set; }

    public string? recommendation_text { get; set; }

    public DateTime? created_at { get; set; }

    public virtual user? user { get; set; }
}
