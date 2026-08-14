using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class AdminLog
{
    public Guid Id { get; set; }

    public Guid? AdminId { get; set; }

    public string? ActionType { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? Admin { get; set; }
}
