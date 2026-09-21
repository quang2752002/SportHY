using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Dms.Application.Common
{
    public static class SlugHelper
    {
        /// <summary>
        /// Chuyển đổi tên tiếng Việt có dấu thành slug URL thân thiện với SEO.
        /// Ví dụ: "Hội Khỏe Phù Đổng Toàn Tỉnh 2026!" -> "hoi-khoe-phu-dong-toan-tinh-2026"
        /// </summary>
        public static string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            text = text.Trim().ToLowerInvariant();

            // Thay thế chữ 'đ'/'Đ' thủ công trước vì FormD không tách 'đ' thành 'd'
            text = text.Replace('đ', 'd').Replace('Đ', 'd');

            // Chuẩn hóa FormD để tách dấu
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            var cleanText = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

            // Thay thế ký tự không phải chữ/số bằng dấu gạch ngang
            cleanText = Regex.Replace(cleanText, @"[^a-z0-9\s-]", "");

            // Gộp nhiều khoảng trắng hoặc gạch ngang liên tiếp thành 1 dấu gạch ngang
            cleanText = Regex.Replace(cleanText, @"[\s-]+", "-").Trim('-');

            return cleanText;
        }
    }
}
