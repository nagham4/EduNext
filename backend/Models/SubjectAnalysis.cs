using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class SubjectAnalysis
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public Guid? SubjectId { get; set; }

    public string? Strengths { get; set; }

    public string? Weaknesses { get; set; }

    public string? ImprovementTip { get; set; }

    public virtual Subject? Subject { get; set; }

    public virtual User? User { get; set; }
}
