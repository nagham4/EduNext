using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class SubjectUnit
{
    public Guid Id { get; set; }

    public Guid SubjectId { get; set; }

    public string Title { get; set; } = null!;

    public int OrderNumber { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual Subject Subject { get; set; } = null!;
}
