using System.Windows;
using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels
{
    public class CaiDatViewModel : BaseViewModel
    {
        public string TenNhanVien    => SessionManager.CurrentUser?.HoTen ?? "";
        public string TaiKhoan       => SessionManager.CurrentUser?.TaiKhoan ?? "";
        public string VaiTro         => SessionManager.TenVaiTro;
        public string Email          => SessionManager.CurrentUser?.Email ?? "(chưa cập nhật)";
        public string SDT            => SessionManager.CurrentUser?.SDT ?? "(chưa cập nhật)";
        public string DiaChi         => SessionManager.CurrentUser?.DiaChi ?? "(chưa cập nhật)";

        public ICommand DoiMatKhauCommand { get; }

        public CaiDatViewModel()
        {
            DoiMatKhauCommand = new RelayCommand(_ => DoiMatKhau());
        }

        private void DoiMatKhau()
        {
            if (SessionManager.CurrentUser == null) return;
            var dlg = new Views.NhanVien.DoiMatKhauDialog(SessionManager.CurrentUser.MaNV);
            dlg.ShowDialog();
        }
    }
}
