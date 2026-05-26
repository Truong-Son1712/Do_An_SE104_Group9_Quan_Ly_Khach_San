using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Helpers
{
    public static class SessionManager
    {
        public static NhanVien? CurrentUser { get; private set; }

        public static bool IsLoggedIn => CurrentUser != null;
        public static bool IsAdmin => CurrentUser?.VaiTro == "Admin";
        public static bool IsQuanLy => CurrentUser?.VaiTro is "Admin" or "QuanLy";
        public static bool IsLeTan => CurrentUser != null;

        public static string TenVaiTro => CurrentUser?.VaiTro switch
        {
            "Admin"  => "Quản Trị Viên",
            "QuanLy" => "Quản Lý",
            "LeTan"  => "Lễ Tân",
            _        => "Không xác định"
        };

        public static void Login(NhanVien user) => CurrentUser = user;
        public static void Logout() => CurrentUser = null;
    }
}
