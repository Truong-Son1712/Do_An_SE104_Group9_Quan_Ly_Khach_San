using System.Windows;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.KhachHang
{
    public partial class LoaiKhachHangDialog : Window
    {
        private readonly string? _maCode; // null = tạo mới

        public LoaiKhachHangDialog(string? maCode = null)
        {
            InitializeComponent();
            _maCode = maCode;

            if (maCode != null)
            {
                TxtTitle.Text = "Chỉnh Sửa Loại Khách";
                LoadData(maCode);
            }
        }

        private void LoadData(string maCode)
        {
            using var ctx = new HotelDbContext();
            var item = ctx.LoaiKhachHangs.Find(maCode);
            if (item == null) return;
            TxtTenLoai.Text = item.TenLoai;
            TxtHeSoGia.Text = item.HeSoGia.ToString("0.####",
                System.Globalization.CultureInfo.InvariantCulture);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            PnlError.Visibility = Visibility.Collapsed;

            string tenLoai = TxtTenLoai.Text.Trim();
            if (string.IsNullOrEmpty(tenLoai))
            { ShowError("Vui lòng nhập tên loại khách."); return; }

            if (!decimal.TryParse(TxtHeSoGia.Text.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal heSo)
                || heSo < 1m)
            { ShowError("Hệ số giá phải >= 1 (1.0 = giá gốc, không được thấp hơn giá gốc)."); return; }

            try
            {
                using var ctx = new HotelDbContext();

                if (_maCode == null)
                {
                    // Tự sinh MaCode từ tên (bỏ dấu cách, thêm timestamp để tránh trùng)
                    string baseCode = new string(tenLoai
                        .Where(c => char.IsLetterOrDigit(c))
                        .ToArray());
                    if (string.IsNullOrEmpty(baseCode)) baseCode = "Loai";
                    string maCode = baseCode + "_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    ctx.LoaiKhachHangs.Add(new LoaiKhachHang
                    {
                        MaCode  = maCode,
                        TenLoai = tenLoai,
                        HeSoGia = heSo
                    });
                }
                else
                {
                    var item = ctx.LoaiKhachHangs.Find(_maCode);
                    if (item == null) return;
                    item.TenLoai = tenLoai;
                    item.HeSoGia = heSo;
                }

                ctx.SaveChanges();
                LoaiKhachTextConverter.ClearCache(); // cập nhật converter sau khi lưu
                DialogResult = true;
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
        private void ShowError(string msg) { TxtError.Text = msg; PnlError.Visibility = Visibility.Visible; }
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
    }
}
