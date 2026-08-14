namespace backend.DTOs.Admin;

public class AdminSubjectsPageDto
{
    public AdminSubjectsStatsDto Stats { get; set; } = new();

    public List<AdminSubjectDto> Subjects { get; set; } = new();

    public List<string> Departments { get; set; } = new();

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int TotalItems { get; set; }

    public int PageSize { get; set; }
}

public class AdminSubjectsStatsDto
{
    public int TotalSubjects { get; set; }

    public int TotalLessons { get; set; }

    public int TotalExams { get; set; }

    public int NewSubjectsThisMonth { get; set; }
}

public class AdminSubjectDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public string Abbr { get; set; } = "";

    public string Department { get; set; } = "";

    public int LessonsCount { get; set; }

    public int ExamsCount { get; set; }
}

public class AdminSubjectLessonDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = "";

    public int Order { get; set; }

    public int? UnitOrder { get; set; }

    public string? UnitTitle { get; set; }
}

public class CreateAdminSubjectDto
{
    public string Name { get; set; } = "";

    public string Department { get; set; } = "";
}

public class UpdateAdminSubjectDto
{
    public string Name { get; set; } = "";

    public string Department { get; set; } = "";
}