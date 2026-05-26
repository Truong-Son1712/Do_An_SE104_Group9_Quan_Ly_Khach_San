using System.Windows;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.NhanVien
{
    public partial class DoiMatKhauDialog : Window
    {
        private readonly int _maNV;
        private readonly bool _isSelf;

        public DoiMatKhauDialog(int maNV)
        {
            InitializeComponent();
            _maNV  = maNV;
            _isSelf = maNV == SessionManager.CurrentUser?.MaNV;

            if (!_isSelf && SessionManager.IsAdmin)
                PnlCu.Visibility = Visibility.Collapsed;

            LoadNhanVienInfo();
        }

        private void LoadNhanVienInfo()
        {
            using var ctx = new HotelDbContext();
            var nv = ctx.NhanViens.Find(_maNV);
            if (nv == null) return;

            TxtHoTen.Text    = nv.HoTen;
            TxtTaiKhoan.Text = nv.TaiKhoan;
            TxtVaiTro.Text   = nv.VaiTro switch
            {
                "Admin"   => "Quản Trị Viên",
                "QuanLy"  => "Quản Lý",
                "LeTan"   => "Lễ Tân",
                _         => nv.VaiTro
            };
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            PnlError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(PbMoi.Password))
            { ShowError("Vui lòng nhập mật khẩu mới."); return; }
            if (PbMoi.Password.Length < 6)
            { ShowError("Mật khẩu phải có ít nhất 6 ký tự."); return; }
            if (PbMoi.Password != PbXacNhan.Password)
            { ShowError("Mật khẩu xác nhận không khớp."); return; }

            try
            {
                using var ctx = new HotelDbContext();
                var nv = ctx.NhanViens.Find(_maNV);
                if (nv == null) return;

                // Kiểm tra mật khẩu cũ nếu cần
                if (_isSelf || !SessionManager.IsAdmin)
                {
                    if (!BCrypt.Net.BCrypt.Verify(PbCu.Password, nv.MatKhau))
                    { ShowError("Mật khẩu hiện tại không đúng."); return; }
                }

                nv.MatKhau = BCrypt.Net.BCrypt.HashPassword(PbMoi.Password);
                ctx.SaveChanges();

                MessageBox.Show("Đổi mật khẩu thành công!", "Thành Công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
        private void ShowError(string msg) { TxtError.Text = msg; PnlError.Visibility = Visibility.Visible; }
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();

    }
}
