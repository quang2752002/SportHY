using Dms.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("CauHinhLichThiDau")]
    public class CauHinhLichThiDau : BaseEntity
    {
        public int MonTheThaoId { get; set; }
        [ForeignKey(nameof(MonTheThaoId))]
        public virtual MonTheThao MonTheThao { get; set; } = null!;

        // ① DÀN TRẢI LỊCH THI ĐẤU
        public bool MoiVongMotNgay { get; set; } = true;
        public int KhoangCachGiuaCacVongGio { get; set; } = 12;
        public bool UuTienChungKetNgayCuoi { get; set; } = true;

        // ② GIẢM TẢI THỂ LỰC VĐV
        public int SoTranToiDaMoiDoiMoiNgay { get; set; } = 1;
        public int NghiToiThieuGiua2TranPhut { get; set; } = 120;

        // ③ KHUNG GIỜ & CA THI ĐẤU
        public bool ChiaCaThiDau { get; set; } = true;

        [Required]
        [MaxLength(5)]
        public string CaSangBatDau { get; set; } = "08:00";

        [Required]
        [MaxLength(5)]
        public string CaSangKetThuc { get; set; } = "11:30";

        [Required]
        [MaxLength(5)]
        public string CaChieuBatDau { get; set; } = "14:00";

        [Required]
        [MaxLength(5)]
        public string CaChieuKetThuc { get; set; } = "17:30";

        [MaxLength(5)]
        public string? CaToBatDau { get; set; }

        [MaxLength(5)]
        public string? CaToKetThuc { get; set; }

        // ④ SÂN ĐẤU & TRỌNG TÀI
        public int ThoiGianDemDonSanPhut { get; set; } = 15;
        public int SoTranToiDaMoiTrongTaiMoiNgay { get; set; } = 4;
        public int NghiToiThieuTrongTaiPhut { get; set; } = 15;

        // ⑤ VĐV THI ĐẤU NHIỀU MÔN
        public int ThoiGianDemDiChuyenPhut { get; set; } = 30;

        // ⑥ GIÁ TRỊ MẶC ĐỊNH KHI MỞ MODAL XẾP LỊCH
        public int ThoiLuongTranMacDinhPhut { get; set; } = 60;
        public int SoHiepDauMacDinh { get; set; } = 0;
        public int ThoiGianMoiHiepPhut { get; set; } = 0;

        [MaxLength(500)]
        public string? GhiChu { get; set; }
    }
}
