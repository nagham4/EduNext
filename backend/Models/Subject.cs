using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Subject
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Stream { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual ICollection<StudentProfileSubject> StudentProfileSubjects { get; set; } = new List<StudentProfileSubject>();

    public virtual ICollection<StudyPlan> StudyPlans { get; set; } = new List<StudyPlan>();

    public virtual ICollection<StudySession> StudySessions { get; set; } = new List<StudySession>();

    public virtual ICollection<SubjectAnalysis> SubjectAnalyses { get; set; } = new List<SubjectAnalysis>();

    public virtual ICollection<SubjectUnit> SubjectUnits { get; set; } = new List<SubjectUnit>();
}
