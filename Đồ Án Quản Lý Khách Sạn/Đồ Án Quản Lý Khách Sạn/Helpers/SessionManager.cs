using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Helpers
{
    public static class SessionManager
    {
        public static NhanVien? CurrentUser { get; private set; }

        private static HashSet<string> _permissions = new();

        public static bool IsLoggedIn => CurrentUser != null;
        public static bool IsAdmin    => CurrentUser?.VaiTro == "Admin";
        public static bool IsQuanLy   => CurrentUser?.VaiTro is "Admin" or "QuanLy";
        public static bool IsLeTan    => CurrentUser != null;

        public static string TenVaiTro
        {
            get
            {
                if (CurrentUser == null) return "";
                // Tra cứu tên loại nhân viên từ DB theo VaiTroCode
                try
                {
                    using var ctx = new HotelDbContext();
                    var ten = ctx.LoaiNhanViens
                        .Where(l => l.VaiTroCode == CurrentUser.VaiTro)
                        .Select(l => l.TenLoai)
                        .FirstOrDefault();
                    return ten ?? CurrentUser.VaiTro;
                }
                catch { return CurrentUser.VaiTro; }
            }
        }

        /// Kiểm tra quyền cụ thể. Admin luôn trả về true.
        public static bool HasPermission(string maQuyen)
        {
            if (IsAdmin) return true;
            return _permissions.Contains(maQuyen);
        }

        public static void Login(NhanVien user)
        {
            CurrentUser = user;
            LoadPermissions();
        }

        public static void Logout()
        {
            CurrentUser = null;
            _permissions.Clear();
        }

        /// Reload quyền từ DB (gọi sau khi admin phân quyền lại).
        public static void ReloadPermissions() => LoadPermissions();

        /// Đọc lại thông tin CurrentUser từ DB (gọi sau khi nhân viên tự sửa hồ sơ của mình).
        public static void RefreshCurrentUser()
        {
            if (CurrentUser == null) return;
            try
            {
                using var ctx = new HotelDbContext();
                var fresh = ctx.NhanViens.Find(CurrentUser.MaNV);
                if (fresh != null) CurrentUser = fresh;
            }
            catch { /* giữ nguyên CurrentUser cũ nếu lỗi DB */ }
        }

        private static void LoadPermissions()
        {
            _permissions.Clear();
            if (CurrentUser == null || IsAdmin) return;
            using var ctx = new HotelDbContext();

            // 1. Ưu tiên quyền cá nhân (đã được Admin phân quyền riêng)
            var individualPerms = ctx.NhanVienQuyens
                .Where(q => q.MaNV == CurrentUser.MaNV)
                .Select(q => q.MaQuyen)
                .ToHashSet();

            if (individualPerms.Any())
            {
                _permissions = individualPerms;
                return;
            }

            // 2. Fallback: dùng quyền mặc định theo loại nhân viên
            var maLoaiNV = ctx.LoaiNhanViens
                .Where(l => l.VaiTroCode == CurrentUser.VaiTro)
                .Select(l => (int?)l.MaLoaiNV)
                .FirstOrDefault();

            if (maLoaiNV.HasValue)
                _permissions = ctx.LoaiNhanVienQuyens
                    .Where(q => q.MaLoaiNV == maLoaiNV.Value)
                    .Select(q => q.MaQuyen)
                    .ToHashSet();
        }
    }
}
