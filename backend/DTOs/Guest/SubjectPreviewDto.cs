namespace backend.DTOs.Guest
{
    public class SubjectPreviewDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Stream { get; set; }
    }
}