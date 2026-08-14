using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class StudySession
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid? SubjectId { get; set; }

    public Guid? LessonId { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public int DurationMinutes { get; set; }

    public string SessionType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Lesson? Lesson { get; set; }

    public virtual Subject? Subject { get; set; }

    public virtual User User { get; set; } = null!;
}
