using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class StudentProfile
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string? Stream { get; set; }

    public string? CurrentGrade { get; set; }

    public int? ExamYear { get; set; }

    public string? PreferredStudyTime { get; set; }

    public string? PreferredStudyPlace { get; set; }

    public string? PrimaryGoal { get; set; }

    public bool? IsOnboardingCompleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<string> LearningMethods { get; set; } = null!;

    public string? ExamExperience { get; set; }

    public virtual ICollection<StudentProfileSubject> StudentProfileSubjects { get; set; } = new List<StudentProfileSubject>();

    public virtual User User { get; set; } = null!;
}
