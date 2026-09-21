using System;
using System.Collections.Generic;
using System.Linq;

namespace Dms.Application.Common
{
    /// <summary>
    /// Danh mục phân quyền động Permissions phân theo các Module và Entity tiếng Việt
    /// </summary>
    public static class Permissions
    {
        // 1. Quản trị hệ thống & tài khoản (System / Users / Roles)
        public static class System
        {
            public const string ManageUsers = "Permissions.System.ManageUsers";
            public const string ConfigSettings = "Permissions.System.ConfigSettings";
            public const string BackupData = "Permissions.System.BackupData";
        }

        public static class Users
        {
            public const string View = "Permissions.Users.View";
            public const string Create = "Permissions.Users.Create";
            public const string Edit = "Permissions.Users.Edit";
            public const string Delete = "Permissions.Users.Delete";
            public const string ManageRoles = "Permissions.Users.ManageRoles";
        }

        public static class Menus
        {
            public const string View = "Permissions.Menus.View";
            public const string Create = "Permissions.Menus.Create";
            public const string Edit = "Permissions.Menus.Edit";
            public const string Delete = "Permissions.Menus.Delete";
        }

        public static class SystemSettings
        {
            public const string View = "Permissions.SystemSettings.View";
            public const string Edit = "Permissions.SystemSettings.Edit";
        }

        // ==========================================
        // 2. CÁC MODULE QUẢN LÝ THỂ THAO TIẾNG VIỆT
        // ==========================================

        // Khối tham gia (Khoi)
        public static class Khoi
        {
            public const string View = "Permissions.Khoi.View";
            public const string Create = "Permissions.Khoi.Create";
            public const string Edit = "Permissions.Khoi.Edit";
            public const string Delete = "Permissions.Khoi.Delete";
        }

        // Đơn vị / Đoàn tham gia (DonVi)
        public static class DonVi
        {
            public const string View = "Permissions.DonVi.View";
            public const string Create = "Permissions.DonVi.Create";
            public const string Edit = "Permissions.DonVi.Edit";
            public const string Delete = "Permissions.DonVi.Delete";
            public const string ManageAthletes = "Permissions.DonVi.ManageAthletes"; // Quản lý VĐV trực thuộc
        }

        // Giải đấu (GiaiDau)
        public static class GiaiDau
        {
            public const string View = "Permissions.GiaiDau.View";
            public const string Create = "Permissions.GiaiDau.Create";
            public const string Edit = "Permissions.GiaiDau.Edit";
            public const string Delete = "Permissions.GiaiDau.Delete";
            public const string AssignManager = "Permissions.GiaiDau.AssignManager"; // Phân công người quản lý giải
        }

        // Danh mục môn & Môn thể thao (DanhMucMonTheThao, MonTheThao)
        public static class DanhMucMonTheThao
        {
            public const string View = "Permissions.DanhMucMonTheThao.View";
            public const string Create = "Permissions.DanhMucMonTheThao.Create";
            public const string Edit = "Permissions.DanhMucMonTheThao.Edit";
            public const string Delete = "Permissions.DanhMucMonTheThao.Delete";
        }

        public static class MonTheThao
        {
            public const string View = "Permissions.MonTheThao.View";
            public const string Create = "Permissions.MonTheThao.Create";
            public const string Edit = "Permissions.MonTheThao.Edit";
            public const string Delete = "Permissions.MonTheThao.Delete";
        }

        // Môn thi thuộc giải (GiaiDauMonTheThao)
        public static class GiaiDauMonTheThao
        {
            public const string View = "Permissions.GiaiDauMonTheThao.View";
            public const string Create = "Permissions.GiaiDauMonTheThao.Create";
            public const string Edit = "Permissions.GiaiDauMonTheThao.Edit";
            public const string Delete = "Permissions.GiaiDauMonTheThao.Delete";
        }

        // Nội dung thi đấu (NoiDungThiDau)
        public static class NoiDungThiDau
        {
            public const string View = "Permissions.NoiDungThiDau.View";
            public const string Create = "Permissions.NoiDungThiDau.Create";
            public const string Edit = "Permissions.NoiDungThiDau.Edit";
            public const string Delete = "Permissions.NoiDungThiDau.Delete";
        }

        // Vận động viên (VanDongVien)
        public static class VanDongVien
        {
            public const string View = "Permissions.VanDongVien.View";
            public const string Create = "Permissions.VanDongVien.Create";
            public const string Edit = "Permissions.VanDongVien.Edit";
            public const string Delete = "Permissions.VanDongVien.Delete";
        }

        // Đội thi đấu & Thành viên (Doi, ThanhVienDoi)
        public static class Doi
        {
            public const string View = "Permissions.Doi.View";
            public const string Create = "Permissions.Doi.Create";
            public const string Edit = "Permissions.Doi.Edit";
            public const string Delete = "Permissions.Doi.Delete";
        }

        // Đăng ký thi đấu (DangKyThiDau)
        public static class DangKyThiDau
        {
            public const string View = "Permissions.DangKyThiDau.View";
            public const string Create = "Permissions.DangKyThiDau.Create";
            public const string Edit = "Permissions.DangKyThiDau.Edit";
            public const string Delete = "Permissions.DangKyThiDau.Delete";
            public const string Approve = "Permissions.DangKyThiDau.Approve"; // Duyệt đơn đăng ký
        }

        // Bảng đấu & Vòng đấu (BangDau, VongDau)
        public static class BangDau
        {
            public const string View = "Permissions.BangDau.View";
            public const string Create = "Permissions.BangDau.Create";
            public const string Edit = "Permissions.BangDau.Edit";
            public const string Delete = "Permissions.BangDau.Delete";
        }

        public static class VongDau
        {
            public const string View = "Permissions.VongDau.View";
            public const string Create = "Permissions.VongDau.Create";
            public const string Edit = "Permissions.VongDau.Edit";
            public const string Delete = "Permissions.VongDau.Delete";
        }

        // Cụm sân & Sân đấu (CumSan, SanDau)
        public static class SanDau
        {
            public const string View = "Permissions.SanDau.View";
            public const string Create = "Permissions.SanDau.Create";
            public const string Edit = "Permissions.SanDau.Edit";
            public const string Delete = "Permissions.SanDau.Delete";
        }

        // Trọng tài & Phân công (TrongTai, PhanCongTrongTai)
        public static class TrongTai
        {
            public const string View = "Permissions.TrongTai.View";
            public const string Create = "Permissions.TrongTai.Create";
            public const string Edit = "Permissions.TrongTai.Edit";
            public const string Delete = "Permissions.TrongTai.Delete";
            public const string Assign = "Permissions.TrongTai.Assign";       // Phân công trọng tài
            public const string Supervise = "Permissions.TrongTai.Supervise"; // Giám sát trọng tài
        }

        // Thư ký bàn & Thư ký giải (ThuKy)
        public static class ThuKy
        {
            public const string View = "Permissions.ThuKy.View";
            public const string Create = "Permissions.ThuKy.Create";
            public const string Edit = "Permissions.ThuKy.Edit";
            public const string Delete = "Permissions.ThuKy.Delete";
        }

        // Trận đấu & Kết quả (TranDau, HiepDau, KetQuaTranDau)
        public static class TranDau
        {
            public const string View = "Permissions.TranDau.View";
            public const string Create = "Permissions.TranDau.Create";
            public const string Edit = "Permissions.TranDau.Edit";
            public const string Delete = "Permissions.TranDau.Delete";
            public const string UpdateScore = "Permissions.TranDau.UpdateScore"; // Nhập / cập nhật điểm số
            public const string VerifyReport = "Permissions.TranDau.VerifyReport"; // Xác nhận biên bản
            public const string ExportReport = "Permissions.TranDau.ExportReport"; // Xuất biên bản kết quả
        }

        // Huy chương (HuyChuong, LoaiHuyChuong)
        public static class HuyChuong
        {
            public const string View = "Permissions.HuyChuong.View";
            public const string Create = "Permissions.HuyChuong.Create";
            public const string Edit = "Permissions.HuyChuong.Edit";
            public const string Delete = "Permissions.HuyChuong.Delete";
        }

        private static readonly List<string> _allPermissions = typeof(Permissions)
            .GetNestedTypes(global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static)
            .SelectMany(type => type.GetFields(global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static | global::System.Reflection.BindingFlags.FlattenHierarchy))
            .Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
            .Select(field => (string)field.GetValue(null)!)
            .ToList();

        /// <summary>
        /// Tự động lấy toàn bộ hằng số Permission bằng Reflection
        /// </summary>
        public static List<string> GetAllPermissions() => _allPermissions;
    }
}
