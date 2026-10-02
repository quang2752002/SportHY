using System;

namespace Dms.Application.Common
{
    /// <summary>
    /// Tiện ích sinh mã ngẫu nhiên có cả chữ và số để tái sử dụng trong toàn hệ thống.
    /// </summary>
    public static class CodeGeneratorHelper
    {
        /// <summary>
        /// Sinh mã ngẫu nhiên bao gồm cả chữ và số (Alphanumeric).
        /// Đảm bảo luôn luôn có ít nhất 1 chữ cái in hoa và ít nhất 1 chữ số.
        /// </summary>
        /// <param name="prefix">Tiền tố của mã (ví dụ: "DM", "MON", "VDV"...). Mặc định là rỗng.</param>
        /// <param name="length">Độ dài phần ngẫu nhiên (mặc định 6 ký tự).</param>
        /// <returns>Mã định danh ngẫu nhiên có cả số và chữ (ví dụ: "DMA8B3C9", "MON7X9K2F").</returns>
        public static string GenerateRandomCode(string prefix = "", int length = 6)
        {
            if (length < 2) length = 2;

            const string letters = "ABCDEFGHJKLMNPQRSTUVWXYZ"; // Bỏ I, O để tránh nhầm 1, 0
            const string digits = "23456789";                  // Bỏ 0, 1 để tránh nhầm O, I
            const string allChars = letters + digits;

            var chars = new char[length];
            // Luôn đảm bảo có ít nhất 1 chữ cái và 1 chữ số
            chars[0] = letters[Random.Shared.Next(letters.Length)];
            chars[1] = digits[Random.Shared.Next(digits.Length)];

            for (int i = 2; i < length; i++)
            {
                chars[i] = allChars[Random.Shared.Next(allChars.Length)];
            }

            // Trộn ngẫu nhiên thứ tự ký tự
            for (int i = chars.Length - 1; i > 0; i--)
            {
                int j = Random.Shared.Next(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            var randomStr = new string(chars);

            return string.IsNullOrWhiteSpace(prefix)
                ? randomStr
                : $"{prefix.Trim().ToUpperInvariant()}{randomStr}";
        }
    }
}
