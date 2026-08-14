using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class UserAchievement
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public Guid? AchievementId { get; set; }

    public DateTime? EarnedAt { get; set; }

    public virtual Achievement? Achievement { get; set; }

    public virtual User? User { get; set; }
}
