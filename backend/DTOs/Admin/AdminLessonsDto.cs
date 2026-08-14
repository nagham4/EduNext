namespace backend.DTOs.Admin;

public class AdminLessonsPageDto
{
    public List<AdminLessonDto> Lessons { get; set; } = new();

    public List<AdminLessonSubjectDto> Subjects { get; set; } = new();

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int TotalItems { get; set; }

    public int PageSize { get; set; }
}

public class AdminLessonDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = "";

    public Guid? SubjectId { get; set; }

    public string SubjectName { get; set; } = "";

    public string SubjectDepartment { get; set; } = "";

    public string Description { get; set; } = "";

    public int Order { get; set; }

    public Guid? UnitId { get; set; }

    public int? UnitOrder { get; set; }

    public string UnitTitle { get; set; } = "";

    public string VideoUrl { get; set; } = "";

    public string Content { get; set; } = "";

    public string PdfUrl { get; set; } = "";

    public string ResourcesUrl { get; set; } = "";
}

public class AdminLessonSubjectDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public string Department { get; set; } = "";
}

public class CreateAdminLessonDto
{
    public Guid SubjectId { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public string Content { get; set; } = "";

    public int Order { get; set; } = 1;

    public string VideoUrl { get; set; } = "";
}

public class UpdateAdminLessonDto
{
    public Guid SubjectId { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public string Content { get; set; } = "";

    public int Order { get; set; } = 1;

    public string VideoUrl { get; set; } = "";
}