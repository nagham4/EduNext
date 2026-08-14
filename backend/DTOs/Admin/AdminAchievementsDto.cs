namespace backend.DTOs.Admin;

public class AdminAchievementsPageDto
{
    public List<AdminAchievementDto> Achievements { get; set; } = new();

    public List<AdminAchievementChartDto> Analytics { get; set; } = new();

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int TotalItems { get; set; }

    public int PageSize { get; set; }

    public int TotalActiveStudents { get; set; }
}

public class AdminAchievementDto
{
    public Guid Id { get; set; }

    public string TitleAr { get; set; } = "";

    public string TitleEn { get; set; } = "";

    public string DescAr { get; set; } = "";

    public string DescEn { get; set; } = "";

    public string Type { get; set; } = "";

    public string TypeLabel { get; set; } = "";

    public string TypeColor { get; set; } = "";

    public int TargetValue { get; set; }

    public string Reward { get; set; } = "";

    public string RewardLabel { get; set; } = "";

    public int RewardValue { get; set; }

    public string Status { get; set; } = "";

    public bool IsActive { get; set; }

    public int UnlockedBy { get; set; }

    public int UnlockPercent { get; set; }

    public bool HasUnlockedStudents { get; set; }
}

public class AdminAchievementChartDto
{
    public Guid Id { get; set; }

    public string TitleAr { get; set; } = "";

    public string Type { get; set; } = "";

    public string TypeColor { get; set; } = "";

    public int UnlockedBy { get; set; }

    public int TotalActiveStudents { get; set; }

    public int UnlockPercent { get; set; }
}

public class CreateAdminAchievementDto
{
    public string TitleAr { get; set; } = "";

    public string TitleEn { get; set; } = "";

    public string DescAr { get; set; } = "";

    public string DescEn { get; set; } = "";

    public string Type { get; set; } = "lessons";

    public int TargetValue { get; set; }

    public string Reward { get; set; } = "points";

    public int RewardValue { get; set; }

    public string Status { get; set; } = "active";
}

public class UpdateAdminAchievementDto : CreateAdminAchievementDto
{
}