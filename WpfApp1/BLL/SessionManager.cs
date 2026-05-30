using WpfApp1.DTO;

namespace WpfApp1.BLL
{
    /// <summary>
    /// Singleton lưu thông tin người dùng đang đăng nhập.
    /// Truy cập từ bất kỳ đâu: SessionManager.Current.User
    /// </summary>
    public sealed class SessionManager
    {
        private static readonly SessionManager _instance = new();
        public static SessionManager Current => _instance;

        private SessionManager() { }

        public UserDto? User      { get; private set; }
        public bool     IsLoggedIn => User != null;

        public void SetUser(UserDto user) => User = user;
        public void Clear()              => User = null;

        // Kiểm tra quyền nhanh
        public bool IsAdmin    => User?.RoleName == UserRole.Roles.Admin;
        public bool IsNhanVien => User?.RoleName == UserRole.Roles.NhanVien;
        public bool IsGiamDoc  => User?.RoleName == UserRole.Roles.GiamDoc;
        public bool IsKeToan   => User?.RoleName == UserRole.Roles.KeToan;
    }
}