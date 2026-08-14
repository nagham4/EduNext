using backend.Models.Generated;

namespace backend.Repositories.Student;

public interface IStudentProfileRepository
{
    Task<user?> GetUserWithProfileAsync(Guid userId);

    Task<user?> GetUserForUpdateAsync(Guid userId);

    Task<List<StudentProfileActivityData>> GetLessonActivitiesAsync(Guid userId, int limit);

    Task<List<StudentProfileActivityData>> GetExamActivitiesAsync(Guid userId, int limit);

    Task<List<StudentProfileActivityData>> GetAchievementActivitiesAsync(Guid userId, int limit);

    Task SaveChangesAsync();
}

public class StudentProfileActivityData
{
    public DateTime Date { get; set; }

    public string Type { get; set; } = "";

    public string Text { get; set; } = "";

    public string Color { get; set; } = "blue";
}