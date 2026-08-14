using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Exam
{
    public Guid Id { get; set; }

    public Guid? SubjectId { get; set; }

    public Guid? LessonId { get; set; }

    public string Type { get; set; } = null!;

    public string Title { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();

    public virtual Lesson? Lesson { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual Subject? Subject { get; set; }
}
