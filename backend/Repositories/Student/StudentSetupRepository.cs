using backend.Data.Generated;
using backend.Models.Generated;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Student;

public class StudentSetupRepository : IStudentSetupRepository
{
    private readonly AppDbContext _db;

    public StudentSetupRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<user?> GetUserByIdAsync(Guid userId)
    {
        return _db.users
            .FirstOrDefaultAsync(u => u.id == userId);
    }

    public Task<student_profile?> GetProfileForReadAsync(Guid userId)
    {
        return _db.student_profiles
            .AsNoTracking()
            .Include(p => p.student_profile_subjects)
            .ThenInclude(x => x.subject)
            .FirstOrDefaultAsync(p => p.user_id == userId);
    }

    public Task<student_profile?> GetProfileForUpdateAsync(Guid userId)
    {
        return _db.student_profiles
            .FirstOrDefaultAsync(p => p.user_id == userId);
    }

    public Task<student_profile?> GetProfileWithSubjectsForUpdateAsync(Guid userId)
    {
        return _db.student_profiles
            .Include(p => p.student_profile_subjects)
            .ThenInclude(x => x.subject)
            .FirstOrDefaultAsync(p => p.user_id == userId);
    }

    public student_profile CreateProfile(Guid userId, DateTime utcNow)
    {
        var profile = new student_profile
        {
            id = Guid.NewGuid(),
            user_id = userId,
            learning_methods = new List<string>(),
            is_onboarding_completed = false,
            created_at = utcNow,
            updated_at = utcNow
        };

        _db.student_profiles.Add(profile);

        return profile;
    }

    public Task<List<subject>> GetSubjectsByIdsAsync(List<Guid> subjectIds)
    {
        return _db.subjects
            .AsNoTracking()
            .Where(s => subjectIds.Contains(s.id))
            .ToListAsync();
    }

    public Task<List<subject>> GetSubjectsByBranchAndNamesAsync(string branch, List<string> subjectNames)
    {
        return _db.subjects
            .AsNoTracking()
            .Where(s => s.stream == branch && subjectNames.Contains(s.name))
            .OrderBy(s => s.name)
            .ToListAsync();
    }

    public Task<bool> BranchExistsAsync(string branch)
    {
        return _db.subjects
            .AsNoTracking()
            .AnyAsync(s => s.stream == branch);
    }

    public Task<List<subject>> GetAllSubjectsForOnboardingAsync()
    {
        return _db.subjects
            .AsNoTracking()
            .Where(s =>
                !string.IsNullOrWhiteSpace(s.stream) &&
                !string.IsNullOrWhiteSpace(s.name)
            )
            .OrderBy(s => s.stream)
            .ThenBy(s => s.name)
            .ToListAsync();
    }

    public void RemoveProfileSubjects(IEnumerable<student_profile_subject> subjects)
    {
        _db.student_profile_subjects.RemoveRange(subjects);
    }

    public void AddProfileSubject(student_profile_subject profileSubject)
    {
        _db.student_profile_subjects.Add(profileSubject);
    }

    public void AddAiRecommendation(ai_recommendation recommendation)
    {
        _db.ai_recommendations.Add(recommendation);
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}
