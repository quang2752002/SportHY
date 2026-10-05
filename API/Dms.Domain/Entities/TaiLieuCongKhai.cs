using Dms.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("TaiLieuCongKhai")]
    public class TaiLieuCongKhai : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string TieuDe { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LoaiTaiLieu { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? MoTa { get; set; }

        [Required]
        [MaxLength(255)]
        public string TenTepGoc { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string TenTepLuu { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string LoaiNoiDung { get; set; } = "application/octet-stream";

        public long KichThuocTep { get; set; }

        public bool CongKhai { get; set; } = true;
    }
}
