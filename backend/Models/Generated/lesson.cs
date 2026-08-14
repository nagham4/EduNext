using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class lesson
{
    public Guid id { get; set; }

    public Guid? subject_id { get; set; }

    public string title { get; set; } = null!;

    public string? video_url { get; set; }

    public string? summary { get; set; }

    public string? content { get; set; }

    public int? order_number { get; set; }

    public int? video_duration_seconds { get; set; }

    public string? pdf_url { get; set; }

    public string? resources_url { get; set; }

    public Guid? subject_unit_id { get; set; }

    public virtual ICollection<exam> exams { get; set; } = new List<exam>();

    public virtual ICollection<lesson_progress> lesson_progresses { get; set; } = new List<lesson_progress>();

    public virtual ICollection<study_plan_item> study_plan_items { get; set; } = new List<study_plan_item>();

    public virtual ICollection<study_session> study_sessions { get; set; } = new List<study_session>();

    public virtual subject? subject { get; set; }

    public virtual subject_unit? subject_unit { get; set; }
}
