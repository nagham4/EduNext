using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class StudyPlan
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public bool? IsAiGenerated { get; set; }

    public DateTime? CreatedAt { get; set; }

    public Guid? SubjectId { get; set; }

    public List<string>? StudyDays { get; set; }

    public int? DailyDurationMinutes { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<StudyPlanItem> StudyPlanItems { get; set; } = new List<StudyPlanItem>();

    public virtual Subject? Subject { get; set; }

    public virtual User? User { get; set; }
}
