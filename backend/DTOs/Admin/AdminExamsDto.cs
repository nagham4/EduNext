namespace backend.DTOs.Admin;

public class AdminExamsPageDto
{
    public AdminExamsStatsDto Stats { get; set; } = new();

    public List<AdminExamDto> Exams { get; set; } = new();

    public List<AdminExamSubjectDto> Subjects { get; set; } = new();

    public List<AdminExamLessonDto> Lessons { get; set; } = new();
}

public class AdminExamsStatsDto
{
    public int TotalExams { get; set; }

    public int ActiveExams { get; set; }

    public int AverageScore { get; set; }

    public int TestedStudents { get; set; }
}

public class AdminExamDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = "";

    public Guid? SubjectId { get; set; }

    public string SubjectName { get; set; } = "";

    public Guid? LessonId { get; set; }

    public string LessonTitle { get; set; } = "";

    public string Type { get; set; } = "";

    public string TypeLabel { get; set; } = "";

    public string TypeColor { get; set; } = "";

    public int QuestionCount { get; set; }

    public bool IsActive { get; set; }

    public List<AdminExamQuestionDto> Questions { get; set; } = new();
}

public class AdminExamSubjectDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public string Department { get; set; } = "";
}

public class AdminExamLessonDto
{
    public Guid Id { get; set; }

    public Guid? SubjectId { get; set; }

    public string Title { get; set; } = "";
}

public class AdminExamQuestionDto
{
    public Guid? Id { get; set; }

    public string Text { get; set; } = "";

    public List<string> Options { get; set; } = new();

    public int CorrectAnswer { get; set; }

    public string SolutionText { get; set; } = "";
}

public class CreateAdminExamDto
{
    public string Title { get; set; } = "";

    public Guid SubjectId { get; set; }

    public Guid? LessonId { get; set; }

    public string Type { get; set; } = "short";

    public List<AdminExamQuestionDto> Questions { get; set; } = new();
}

public class UpdateAdminExamDto
{
    public string Title { get; set; } = "";

    public Guid SubjectId { get; set; }

    public Guid? LessonId { get; set; }

    public string Type { get; set; } = "short";

    public List<AdminExamQuestionDto> Questions { get; set; } = new();
}