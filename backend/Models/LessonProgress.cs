using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class LessonProgress
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public Guid? LessonId { get; set; }

    public bool? Completed { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual Lesson? Lesson { get; set; }

    public virtual User? User { get; set; }
}
