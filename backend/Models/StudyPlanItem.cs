using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class StudyPlanItem
{
    public Guid Id { get; set; }

    public Guid StudyPlanId { get; set; }

    public Guid LessonId { get; set; }

    public int? OrderNumber { get; set; }

    public bool? IsCompleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;

    public virtual StudyPlan StudyPlan { get; set; } = null!;
}
