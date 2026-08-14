using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class subject_unit
{
    public Guid id { get; set; }

    public Guid subject_id { get; set; }

    public string title { get; set; } = null!;

    public int order_number { get; set; }

    public DateTime? created_at { get; set; }

    public virtual ICollection<lesson> lessons { get; set; } = new List<lesson>();

    public virtual subject subject { get; set; } = null!;
}
