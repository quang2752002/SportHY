using Microsoft.AspNetCore.Identity;

namespace Dms.Domain.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? DonViId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(DonViId))]
        public virtual DonVi? DonVi { get; set; }

        public int? TrongTaiId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(TrongTaiId))]
        public virtual TrongTai? TrongTai { get; set; }

        public int? ThuKyId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(ThuKyId))]
        public virtual ThuKy? ThuKy { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
