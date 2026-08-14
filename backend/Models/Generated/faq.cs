using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class faq
{
    public Guid id { get; set; }

    public string question { get; set; } = null!;

    public string answer { get; set; } = null!;

    public DateTime? created_at { get; set; }
}
