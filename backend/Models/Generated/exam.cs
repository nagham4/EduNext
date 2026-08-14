using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class exam
{
    public Guid id { get; set; }

    public Guid? subject_id { get; set; }

    public Guid? lesson_id { get; set; }

    public string type { get; set; } = null!;

    public string title { get; set; } = null!;

    public bool is_active { get; set; }

    public DateTime created_at { get; set; }

    public virtual ICollection<exam_result> exam_results { get; set; } = new List<exam_result>();

    public virtual lesson? lesson { get; set; }

    public virtual ICollection<question> questions { get; set; } = new List<question>();

    public virtual subject? subject { get; set; }
}
