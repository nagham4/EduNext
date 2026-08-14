using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Lesson
{
    public Guid Id { get; set; }

    public Guid? SubjectId { get; set; }

    public string Title { get; set; } = null!;

    public string? VideoUrl { get; set; }

    public string? Summary { get; set; }

    public string? Content { get; set; }

    public int? OrderNumber { get; set; }

    public int? VideoDurationSeconds { get; set; }

    public string? PdfUrl { get; set; }

    public string? ResourcesUrl { get; set; }

    public Guid? SubjectUnitId { get; set; }

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();

    public virtual ICollection<StudyPlanItem> StudyPlanItems { get; set; } = new List<StudyPlanItem>();

    public virtual ICollection<StudySession> StudySessions { get; set; } = new List<StudySession>();

    public virtual Subject? Subject { get; set; }

    public virtual SubjectUnit? SubjectUnit { get; set; }
}
