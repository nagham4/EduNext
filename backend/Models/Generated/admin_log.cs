using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class admin_log
{
    public Guid id { get; set; }

    public Guid? admin_id { get; set; }

    public string? action_type { get; set; }

    public string? description { get; set; }

    public DateTime? created_at { get; set; }

    public virtual user? admin { get; set; }
}
