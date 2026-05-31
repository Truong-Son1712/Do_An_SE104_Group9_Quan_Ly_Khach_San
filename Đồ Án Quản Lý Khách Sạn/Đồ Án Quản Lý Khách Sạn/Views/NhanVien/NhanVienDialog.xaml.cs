using System.Windows;
using System.Windows.Controls;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.NhanVien
{
    public partial class NhanVienDialog : Window
    {
        private readonly int? _maNV;

        public NhanVienDialog(int? maNV = null)
        {
            InitializeComponent();
            _maNV = maNV;
            if (maNV.HasValue) LoadData(maNV.Value);
            else PnlMatKhau.Visibility = Visibility.Visible;
        }

        private void LoadData(int id)
        {
            using var ctx = new HotelDbContext();
            var nv = ctx.NhanViens.Find(id);
            if (nv == null) return;

            TxtTitle.Text = "Chỉnh Sửa Nhân Viên";
            TxtHoTen.Text  = nv.HoTen;
            TxtTaiKhoan.Text = nv.TaiKhoan;
            TxtTaiKhoan.IsReadOnly = true; // không đổi tài khoản
            TxtEmail.Text  = nv.Email;
            TxtSDT.Text    = nv.SDT;
            TxtDiaChi.Text = nv.DiaChi;
            PnlMatKhau.Visibility = Visibility.Collapsed; // không đổi mật khẩu ở đây

            foreach (ComboBoxItem item in CboVaiTro.Items)
                if (item.Tag?.ToString() == nv.VaiTro) { item.IsSelected = true; break; }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            PnlError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(TxtHoTen.Text))
            { ShowError("Vui lòng nhập họ tên."); return; }
            if (string.IsNullOrWhiteSpace(TxtTaiKhoan.Text))
            { ShowError("Vui lòng nhập tài khoản."); return; }

            string vaiTro = (CboVaiTro.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "LeTan";

            try
            {
                using var ctx = new HotelDbContext();
                if (_maNV.HasValue)
                {
                    var nv = ctx.NhanViens.Find(_maNV.Value);
                    if (nv == null) return;
                    nv.HoTen  = TxtHoTen.Text.Trim();
                    nv.VaiTro = vaiTro;
                    nv.Email  = TxtEmail.Text.Trim();
                    nv.SDT    = TxtSDT.Text.Trim();
                    nv.DiaChi = TxtDiaChi.Text.Trim();
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(PbMatKhau.Password))
                    { ShowError("Vui lòng nhập mật khẩu."); return; }
                    if (PbMatKhau.Password != PbXacNhan.Password)
                    { ShowError("Mật khẩu xác nhận không khớp."); return; }
                    if (ctx.NhanViens.Any(n => n.TaiKhoan == TxtTaiKhoan.Text.Trim()))
                    { ShowError("Tài khoản đã tồn tại."); return; }

                    ctx.NhanViens.Add(new Models.NhanVien
                    {
                        HoTen    = TxtHoTen.Text.Trim(),
                        TaiKhoan = TxtTaiKhoan.Text.Trim(),
                        MatKhau  = BCrypt.Net.BCrypt.HashPassword(PbMatKhau.Password),
                        VaiTro   = vaiTro,
                        Email    = TxtEmail.Text.Trim(),
                        SDT      = TxtSDT.Text.Trim(),
                        DiaChi   = TxtDiaChi.Text.Trim(),
                        IsActive = true
                    });
                }
                ctx.SaveChanges();
                DialogResult = true;
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
        private void ShowError(string msg) { TxtError.Text = msg; PnlError.Visibility = Visibility.Visible; }
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();

    }
}
