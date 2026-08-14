using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class subject
{
    public Guid id { get; set; }

    public string name { get; set; } = null!;

    public string? description { get; set; }

    public string? stream { get; set; }

    public DateTime? created_at { get; set; }

    public virtual ICollection<exam> exams { get; set; } = new List<exam>();

    public virtual ICollection<lesson> lessons { get; set; } = new List<lesson>();

    public virtual ICollection<student_preference_difficult_subject> student_preference_difficult_subjects { get; set; } = new List<student_preference_difficult_subject>();

    public virtual ICollection<student_profile_subject> student_profile_subjects { get; set; } = new List<student_profile_subject>();

    public virtual ICollection<study_plan> study_plans { get; set; } = new List<study_plan>();

    public virtual ICollection<study_session> study_sessions { get; set; } = new List<study_session>();

    public virtual ICollection<subject_analysis> subject_analyses { get; set; } = new List<subject_analysis>();

    public virtual ICollection<subject_unit> subject_units { get; set; } = new List<subject_unit>();
}
