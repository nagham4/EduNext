namespace backend.DTOs.Admin;

public class AdminAnalyticsDto
{
    public AdminAnalyticsStatsDto Stats { get; set; } = new();

    public List<AdminDailyExamDto> DailyExams { get; set; } = new();

    public List<AdminPopularSubjectDto> PopularSubjects { get; set; } = new();

    public List<AdminActivityTimeDto> ActivityTimes { get; set; } = new();

    public List<AdminSubjectLessonCompletionDto> LessonCompletion { get; set; } = new();
}

public class AdminAnalyticsStatsDto
{
    public int CompletedExams { get; set; }

    public int ActiveStudents { get; set; }

    public string CompletedExamsChange { get; set; } = "";

    public string ActiveStudentsChange { get; set; } = "";
}

public class AdminDailyExamDto
{
    public string Name { get; set; } = "";

    public int Value { get; set; }
}

public class AdminPopularSubjectDto
{
    public string Name { get; set; } = "";

    // النسبة من إجمالي دقائق الدراسة للمواد
    public int Value { get; set; }

    // عدد دقائق الدراسة الفعلي
    public int ActivityCount { get; set; }

    public string Unit { get; set; } = "دقيقة";

    public string Color { get; set; } = "";
}

public class AdminActivityTimeDto
{
    public string Time { get; set; } = "";

    public string Level { get; set; } = "";

    public int Percent { get; set; }

    public int Minutes { get; set; }
}

public class AdminSubjectLessonCompletionDto
{
    public string Subject { get; set; } = "";

    public int Percent { get; set; }

    public int CompletedCount { get; set; }

    public int RequiredCount { get; set; }
}