using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class user
{
    public Guid id { get; set; }

    public string full_name { get; set; } = null!;

    public string email { get; set; } = null!;

    public string password_hash { get; set; } = null!;

    public string role { get; set; } = null!;

    public bool? is_active { get; set; }

    public DateTime? created_at { get; set; }

    public int? points { get; set; }

    public bool onboarding_completed { get; set; }

    public DateTime? onboarding_completed_at { get; set; }

    public string? phone { get; set; }

    public virtual ICollection<admin_log> admin_logs { get; set; } = new List<admin_log>();

    public virtual ICollection<ai_recommendation> ai_recommendations { get; set; } = new List<ai_recommendation>();

    public virtual ICollection<exam_result> exam_results { get; set; } = new List<exam_result>();

    public virtual ICollection<lesson_progress> lesson_progresses { get; set; } = new List<lesson_progress>();

    public virtual student_preference? student_preference { get; set; }

    public virtual student_profile? student_profile { get; set; }

    public virtual ICollection<study_plan> study_plans { get; set; } = new List<study_plan>();

    public virtual ICollection<study_session> study_sessions { get; set; } = new List<study_session>();

    public virtual ICollection<subject_analysis> subject_analyses { get; set; } = new List<subject_analysis>();

    public virtual ICollection<user_achievement> user_achievements { get; set; } = new List<user_achievement>();

    public virtual user_stat? user_stat { get; set; }
}
