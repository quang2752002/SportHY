namespace Dms.Application.DTOs
{
    public class MenuDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public int? ParentId { get; set; }
        public string? ParentTitle { get; set; }
        public DateTime? Created { get; set; }
        public List<MenuDto> Children { get; set; } = new List<MenuDto>();
    }
}
