using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class student_profile
{
    public Guid id { get; set; }

    public Guid user_id { get; set; }

    public string? stream { get; set; }

    public string? current_grade { get; set; }

    public int? exam_year { get; set; }

    public string? preferred_study_time { get; set; }

    public string? preferred_study_place { get; set; }

    public string? primary_goal { get; set; }

    public bool? is_onboarding_completed { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public List<string> learning_methods { get; set; } = new();

    public string? exam_experience { get; set; }

    public virtual ICollection<student_profile_subject> student_profile_subjects { get; set; } = new List<student_profile_subject>();

    public virtual user user { get; set; } = null!;
}
