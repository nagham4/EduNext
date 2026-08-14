using backend.Data.Generated;
using backend.DTOs.Admin;
using backend.Models.Generated;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Admin;

public class AdminAchievementsRepository : IAdminAchievementsRepository
{
    private readonly AppDbContext _context;

    public AdminAchievementsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminAchievementsPageDto> GetAchievementsPageAsync(
        string? search,
        string? type,
        string? status,
        int page,
        int pageSize
    )
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 5 : pageSize;

        var query = _context.achievements
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();

            query = query.Where(a =>
                (a.title_ar != null && a.title_ar.ToLower().Contains(normalizedSearch)) ||
                (a.title_en != null && a.title_en.ToLower().Contains(normalizedSearch)) ||
                (a.title != null && a.title.ToLower().Contains(normalizedSearch))
            );
        }

        if (!string.IsNullOrWhiteSpace(type) && type != "all")
        {
            query = query.Where(a => a.condition_type == type);
        }

        if (!string.IsNullOrWhiteSpace(status) && status != "all")
        {
            if (status == "active")
            {
                query = query.Where(a => a.is_active == true);
            }
            else if (status == "inactive")
            {
                query = query.Where(a => a.is_active != true);
            }
        }

        var totalItems = await query.CountAsync();

        var totalPages = totalItems == 0
            ? 1
            : (int)Math.Ceiling((double)totalItems / pageSize);

        if (page > totalPages)
        {
            page = totalPages;
        }

        var totalActiveStudents = await GetTotalActiveStudentsAsync();

        var rawAchievements = await query
            .OrderByDescending(a => a.created_at)
            .ThenBy(a => a.title_ar)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.id,
                a.title,
                a.description,
                a.condition_type,
                a.condition_value,
                a.title_ar,
                a.title_en,
                a.description_ar,
                a.description_en,
                a.reward_type,
                a.reward_value,
                a.is_active
            })
            .ToListAsync();

        var achievementIds = rawAchievements.Select(a => a.id).ToList();

        var unlockedCounts = await GetUnlockedCountsAsync(achievementIds);

        var achievements = rawAchievements
            .Select(a =>
            {
                var typeValue = a.condition_type ?? "";
                var rewardValue = a.reward_type ?? "points";

                var unlockedBy = unlockedCounts.TryGetValue(a.id, out var count)
                    ? count
                    : 0;

                return BuildAchievementDto(
                    a.id,
                    a.title,
                    a.description,
                    typeValue,
                    a.condition_value,
                    a.title_ar,
                    a.title_en,
                    a.description_ar,
                    a.description_en,
                    rewardValue,
                    a.reward_value,
                    a.is_active,
                    unlockedBy,
                    totalActiveStudents
                );
            })
            .ToList();

        return new AdminAchievementsPageDto
        {
            Achievements = achievements,
            Analytics = await GetAnalyticsAsync(totalActiveStudents),
            CurrentPage = page,
            TotalPages = totalPages,
            TotalItems = totalItems,
            PageSize = pageSize,
            TotalActiveStudents = totalActiveStudents
        };
    }

    public async Task<AdminAchievementDto?> CreateAchievementAsync(CreateAdminAchievementDto dto)
    {
        if (!IsValidAchievement(dto))
        {
            return null;
        }

        var normalizedType = NormalizeType(dto.Type);
        var normalizedReward = NormalizeReward(dto.Reward);

        var achievement = new achievement
        {
            id = Guid.NewGuid(),

            title = dto.TitleAr.Trim(),
            description = dto.DescAr.Trim(),
            condition_type = normalizedType,
            condition_value = dto.TargetValue,

            title_ar = dto.TitleAr.Trim(),
            title_en = dto.TitleEn.Trim(),
            description_ar = dto.DescAr.Trim(),
            description_en = dto.DescEn.Trim(),

            reward_type = normalizedReward,
            reward_value = dto.RewardValue,
            is_active = dto.Status == "active",

            created_at = GetUnspecifiedNow(),
            updated_at = null
        };

        _context.achievements.Add(achievement);

        await _context.SaveChangesAsync();

        return await GetAchievementByIdAsync(achievement.id);
    }

    public async Task<AdminAchievementDto?> UpdateAchievementAsync(Guid id, UpdateAdminAchievementDto dto)
    {
        if (!IsValidAchievement(dto))
        {
            return null;
        }

        var achievement = await _context.achievements
            .FirstOrDefaultAsync(a => a.id == id);

        if (achievement == null)
        {
            return null;
        }

        var normalizedType = NormalizeType(dto.Type);
        var normalizedReward = NormalizeReward(dto.Reward);

        achievement.title = dto.TitleAr.Trim();
        achievement.description = dto.DescAr.Trim();
        achievement.condition_type = normalizedType;
        achievement.condition_value = dto.TargetValue;

        achievement.title_ar = dto.TitleAr.Trim();
        achievement.title_en = dto.TitleEn.Trim();
        achievement.description_ar = dto.DescAr.Trim();
        achievement.description_en = dto.DescEn.Trim();

        achievement.reward_type = normalizedReward;
        achievement.reward_value = dto.RewardValue;
        achievement.is_active = dto.Status == "active";
        achievement.updated_at = GetUnspecifiedNow();

        await _context.SaveChangesAsync();

        return await GetAchievementByIdAsync(id);
    }

    public async Task<bool> DeleteAchievementAsync(Guid id)
    {
        var achievement = await _context.achievements
            .FirstOrDefaultAsync(a => a.id == id);

        if (achievement == null)
        {
            return false;
        }

        achievement.is_active = false;
        achievement.updated_at = GetUnspecifiedNow();

        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<AdminAchievementDto?> GetAchievementByIdAsync(Guid id)
    {
        var rawAchievement = await _context.achievements
            .AsNoTracking()
            .Where(a => a.id == id)
            .Select(a => new
            {
                a.id,
                a.title,
                a.description,
                a.condition_type,
                a.condition_value,
                a.title_ar,
                a.title_en,
                a.description_ar,
                a.description_en,
                a.reward_type,
                a.reward_value,
                a.is_active
            })
            .FirstOrDefaultAsync();

        if (rawAchievement == null)
        {
            return null;
        }

        var totalActiveStudents = await GetTotalActiveStudentsAsync();
        var unlockedBy = await GetUnlockedCountAsync(rawAchievement.id);

        return BuildAchievementDto(
            rawAchievement.id,
            rawAchievement.title,
            rawAchievement.description,
            rawAchievement.condition_type ?? "",
            rawAchievement.condition_value,
            rawAchievement.title_ar,
            rawAchievement.title_en,
            rawAchievement.description_ar,
            rawAchievement.description_en,
            rawAchievement.reward_type ?? "points",
            rawAchievement.reward_value,
            rawAchievement.is_active,
            unlockedBy,
            totalActiveStudents
        );
    }

    private AdminAchievementDto BuildAchievementDto(
        Guid id,
        string? title,
        string? description,
        string typeValue,
        int? conditionValue,
        string? titleAr,
        string? titleEn,
        string? descriptionAr,
        string? descriptionEn,
        string rewardValue,
        int? rewardAmount,
        bool? isActive,
        int unlockedBy,
        int totalActiveStudents
    )
    {
        return new AdminAchievementDto
        {
            Id = id,
            TitleAr = titleAr ?? title ?? "",
            TitleEn = titleEn ?? title ?? "",
            DescAr = descriptionAr ?? description ?? "",
            DescEn = descriptionEn ?? description ?? "",
            Type = typeValue,
            TypeLabel = GetTypeLabel(typeValue),
            TypeColor = GetTypeColor(typeValue),
            TargetValue = conditionValue ?? 0,
            Reward = rewardValue,
            RewardLabel = GetRewardLabel(rewardValue),
            RewardValue = rewardAmount ?? 0,
            Status = isActive == true ? "active" : "inactive",
            IsActive = isActive == true,
            UnlockedBy = unlockedBy,
            UnlockPercent = CalculatePercent(unlockedBy, totalActiveStudents),
            HasUnlockedStudents = unlockedBy > 0
        };
    }

    private async Task<List<AdminAchievementChartDto>> GetAnalyticsAsync(int totalActiveStudents)
    {
        var achievements = await _context.achievements
            .AsNoTracking()
            .Select(a => new
            {
                a.id,
                a.title,
                a.title_ar,
                a.condition_type
            })
            .ToListAsync();

        if (!achievements.Any())
        {
            return new List<AdminAchievementChartDto>();
        }

        var achievementIds = achievements.Select(a => a.id).ToList();

        var unlockedCounts = await GetUnlockedCountsAsync(achievementIds);

        return achievements
            .Select(a =>
            {
                var typeValue = a.condition_type ?? "";

                var unlockedBy = unlockedCounts.TryGetValue(a.id, out var count)
                    ? count
                    : 0;

                return new AdminAchievementChartDto
                {
                    Id = a.id,
                    TitleAr = a.title_ar ?? a.title ?? "",
                    Type = typeValue,
                    TypeColor = GetTypeColor(typeValue),
                    UnlockedBy = unlockedBy,
                    TotalActiveStudents = totalActiveStudents,
                    UnlockPercent = CalculatePercent(unlockedBy, totalActiveStudents)
                };
            })
            .Where(a => a.UnlockedBy > 0)
            .OrderByDescending(a => a.UnlockedBy)
            .ThenBy(a => a.TitleAr)
            .Take(6)
            .ToList();
    }

    private async Task<int> GetTotalActiveStudentsAsync()
    {
        return await _context.users
            .AsNoTracking()
            .CountAsync(u => u.role == "student" && u.is_active == true);
    }

    private async Task<int> GetUnlockedCountAsync(Guid achievementId)
    {
        return await _context.user_achievements
            .AsNoTracking()
            .Where(ua =>
                ua.achievement_id.HasValue &&
                ua.achievement_id.Value == achievementId &&
                ua.user_id.HasValue
            )
            .Select(ua => ua.user_id!.Value)
            .Distinct()
            .CountAsync();
    }

    private async Task<Dictionary<Guid, int>> GetUnlockedCountsAsync(List<Guid> achievementIds)
    {
        if (!achievementIds.Any())
        {
            return new Dictionary<Guid, int>();
        }

        return await _context.user_achievements
            .AsNoTracking()
            .Where(ua =>
                ua.achievement_id.HasValue &&
                ua.user_id.HasValue &&
                achievementIds.Contains(ua.achievement_id.Value)
            )
            .GroupBy(ua => ua.achievement_id!.Value)
            .Select(g => new
            {
                AchievementId = g.Key,
                Count = g.Select(x => x.user_id!.Value).Distinct().Count()
            })
            .ToDictionaryAsync(x => x.AchievementId, x => x.Count);
    }

    private static int CalculatePercent(int value, int total)
    {
        if (total <= 0)
        {
            return 0;
        }

        return (int)Math.Round((double)value / total * 100);
    }

    private static bool IsValidAchievement(CreateAdminAchievementDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TitleAr)) return false;
        if (string.IsNullOrWhiteSpace(dto.TitleEn)) return false;
        if (string.IsNullOrWhiteSpace(dto.DescAr)) return false;
        if (string.IsNullOrWhiteSpace(dto.DescEn)) return false;
        if (dto.TargetValue <= 0) return false;
        if (dto.RewardValue < 0) return false;

        var type = NormalizeType(dto.Type);
        var reward = NormalizeReward(dto.Reward);

        return IsAllowedType(type) && IsAllowedReward(reward);
    }

    private static string NormalizeType(string type)
    {
        return type.Trim() switch
        {
            "دروس" => "lessons",
            "امتحانات" => "exams",
            "سلاسل دراسة" => "streaks",
            "نقاط" => "points",
            "تعاون" => "collaboration",
            "lessons" => "lessons",
            "exams" => "exams",
            "streaks" => "streaks",
            "points" => "points",
            "collaboration" => "collaboration",
            _ => type.Trim()
        };
    }

    private static string NormalizeReward(string reward)
    {
        return reward.Trim() switch
        {
            "نقاط" => "points",
            "وسام" => "badge",
            "رفع مستوى" => "level",
            "points" => "points",
            "badge" => "badge",
            "level" => "level",
            _ => reward.Trim()
        };
    }

    private static bool IsAllowedType(string type)
    {
        return type is "lessons" or "exams" or "streaks" or "points" or "collaboration";
    }

    private static bool IsAllowedReward(string reward)
    {
        return reward is "points" or "badge" or "level";
    }

    private static string GetTypeLabel(string type)
    {
        return type switch
        {
            "lessons" => "دروس",
            "exams" => "امتحانات",
            "streaks" => "سلاسل دراسة",
            "points" => "نقاط",
            "collaboration" => "تعاون",
            _ => type
        };
    }

    private static string GetRewardLabel(string reward)
    {
        return reward switch
        {
            "points" => "نقاط",
            "badge" => "وسام",
            "level" => "رفع مستوى",
            _ => reward
        };
    }

    private static string GetTypeColor(string type)
    {
        return type switch
        {
            "lessons" => "#135bec",
            "exams" => "#e74c3c",
            "streaks" => "#f39c12",
            "points" => "#9b59b6",
            "collaboration" => "#2ecc71",
            _ => "#135bec"
        };
    }

    private static DateTime GetUnspecifiedNow()
    {
        var now = DateTime.Now;

        return new DateTime(
            now.Year,
            now.Month,
            now.Day,
            now.Hour,
            now.Minute,
            now.Second,
            DateTimeKind.Unspecified
        );
    }
}