/**
 * Hàm sinh slug tiếng Việt chuẩn SEO ở frontend
 * Ví dụ: "Giải Bóng Đá Vô Địch Tỉnh 2026!" -> "giai-bong-da-vo-dich-tinh-2026"
 */
export function generateSlug(text: string): string {
  if (!text) return '';

  let str = text.toLowerCase().trim();

  // Đổi ký tự có dấu thành không dấu
  str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, 'a');
  str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, 'e');
  str = str.replace(/ì|í|ị|ỉ|ĩ/g, 'i');
  str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, 'o');
  str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, 'u');
  str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, 'y');
  str = str.replace(/đ/g, 'd');

  // Xóa các ký tự đặc biệt
  str = str.replace(/[^a-z0-9\s-]/g, '');

  // Đổi nhiều khoảng trắng / gạch ngang liên tiếp thành 1 gạch ngang
  str = str.replace(/[\s-]+/g, '-');

  // Xóa gạch ngang ở đầu và cuối
  str = str.replace(/^-+|-+$/g, '');

  return str;
}
