using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Achievement
{
    public Guid Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? ConditionType { get; set; }

    public int? ConditionValue { get; set; }

    public string? TitleAr { get; set; }

    public string? TitleEn { get; set; }

    public string? DescriptionAr { get; set; }

    public string? DescriptionEn { get; set; }

    public string? RewardType { get; set; }

    public int? RewardValue { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
