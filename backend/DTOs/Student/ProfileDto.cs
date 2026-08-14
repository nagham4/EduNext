public class ProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string Role { get; set; } = "";
    public string? Branch { get; set; }
    public string? AcademicYear { get; set; }
    public List<ActivityGroupDto> ActivityHistory { get; set; } = new();
}

public class ActivityGroupDto
{
    public string DateLabel { get; set; } = "";
    public List<ActivityItemDto> Items { get; set; } = new();
}

public class ActivityItemDto
{
    public string Type { get; set; } = "";
    public string Text { get; set; } = "";
    public string Time { get; set; } = "";
    public string Color { get; set; } = "blue";
}