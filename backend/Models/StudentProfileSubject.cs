using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class StudentProfileSubject
{
    public Guid Id { get; set; }

    public Guid StudentProfileId { get; set; }

    public Guid SubjectId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual StudentProfile StudentProfile { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}
