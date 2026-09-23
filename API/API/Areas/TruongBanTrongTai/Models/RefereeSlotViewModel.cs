namespace API.Areas.TruongBanTrongTai.Models;

public class RefereeSlotViewModel
{
    public int TranDauId { get; set; }

    public string VaiTro { get; set; } = string.Empty;

    public int? CurrentId { get; set; }

    public string? CurrentName { get; set; }

    public string BadgeClass { get; set; } = "bg-primary text-white";
}
