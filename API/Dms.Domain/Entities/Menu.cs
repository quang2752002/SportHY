using Dms.Domain.Common;

namespace Dms.Domain.Entities
{
    public class Menu : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public int? ParentId { get; set; }
        public virtual Menu? Parent { get; set; }
        public virtual ICollection<Menu> Children { get; set; } = new List<Menu>();
    }
}
