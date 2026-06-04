using System.Windows;
using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels
{
    /// ViewModel cho màn hình Cài đặt và thông tin cá nhân nhân viên.
    /// Quản lý việc hiển thị thông tin hồ sơ và thực hiện lệnh đổi mật khẩu.
    public class CaiDatViewModel : BaseViewModel
    {
        #region 1. Properties - Thông tin cá nhân hiển thị
        /// Lấy họ tên hiển thị của nhân viên đang đăng nhập.
        public string TenNhanVien    => SessionManager.CurrentUser?.HoTen ?? "";
        /// Lấy tên tài khoản đăng nhập của nhân viên hiện tại.
        public string TaiKhoan       => SessionManager.CurrentUser?.TaiKhoan ?? "";
        /// Lấy tên vai trò hoặc chức vụ hiển thị của nhân viên.
        public string VaiTro         => SessionManager.TenVaiTro;
        /// Lấy địa chỉ email của nhân viên (hiển thị thông báo nếu chưa cập nhật).
        public string Email          => SessionManager.CurrentUser?.Email ?? "(chưa cập nhật)";
        /// Lấy số điện thoại liên lạc của nhân viên.
        public string SDT            => SessionManager.CurrentUser?.SDT ?? "(chưa cập nhật)";
        /// Lấy địa chỉ nơi ở của nhân viên.
        public string DiaChi         => SessionManager.CurrentUser?.DiaChi ?? "(chưa cập nhật)";
        public string CCCD           => string.IsNullOrWhiteSpace(SessionManager.CurrentUser?.CCCD)
                                            ? "(chưa cập nhật)" : SessionManager.CurrentUser!.CCCD;
        #endregion

        #region 2. Commands - Các nút lệnh tương tác
        /// Lệnh yêu cầu thực hiện đổi mật khẩu tài khoản.
        public ICommand DoiMatKhauCommand { get; }
        #endregion

        #region 3. Constructor & Methods - Xử lý logic
        /// Khởi tạo ViewModel Cài đặt và đăng ký các lệnh command.
        public CaiDatViewModel()
        {
            DoiMatKhauCommand = new RelayCommand(_ => DoiMatKhau());
        }
        /// Hiển thị hộp thoại (Dialog) cho phép nhân viên tự thay đổi mật khẩu của mình.
        private void DoiMatKhau()
        {
            if (SessionManager.CurrentUser == null) return;
            var dlg = new Views.NhanVien.DoiMatKhauDialog(SessionManager.CurrentUser.MaNV);
            dlg.ShowDialog();
        }
        #endregion
    }
}
