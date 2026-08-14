using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class student_preference_learning_method
{
    public Guid user_id { get; set; }

    public string method_code { get; set; } = null!;

    public DateTime created_at { get; set; }

    public virtual student_preference user { get; set; } = null!;
}
