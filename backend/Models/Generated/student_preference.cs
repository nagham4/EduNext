using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class student_preference
{
    public Guid user_id { get; set; }

    public string branch_code { get; set; } = null!;

    public string study_hours_code { get; set; } = null!;

    public string goal_code { get; set; } = null!;

    public string level_code { get; set; } = null!;

    public string exam_experience_code { get; set; } = null!;

    public bool has_other_difficult_subject { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<student_preference_difficult_subject> student_preference_difficult_subjects { get; set; } = new List<student_preference_difficult_subject>();

    public virtual ICollection<student_preference_learning_method> student_preference_learning_methods { get; set; } = new List<student_preference_learning_method>();

    public virtual user user { get; set; } = null!;
}
