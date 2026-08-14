using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class AiRecommendation
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string? RecommendationText { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
