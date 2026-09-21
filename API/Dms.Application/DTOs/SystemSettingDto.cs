namespace Dms.Application.DTOs
{
    public class SystemSettingDto
    {
        public string Id { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? Created { get; set; }
    }
}
