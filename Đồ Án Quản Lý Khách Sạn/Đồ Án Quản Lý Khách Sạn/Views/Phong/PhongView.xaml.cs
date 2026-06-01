using System.Windows;
using System.Windows.Controls;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.Phong
{
    public partial class PhongView : UserControl
    {
        public PhongView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            bool isAdmin = SessionManager.IsAdmin;

            // ── Dropdown loại khách ─────────────────────────────────────────
            LoadLoaiKhachCombo();
            TxtHeSo.IsReadOnly   = !isAdmin;
            BtnLuuHeSo.IsEnabled = isAdmin;
            BtnLuuHeSo.Opacity   = isAdmin ? 1.0 : 0.4;
            CboLoaiKhach.IsEnabled = isAdmin;

            // ── Sức chứa tối đa & tỷ lệ phụ thu ────────────────────────────
            int sucChuaMax     = AppConfig.GetSucChuaToiDa();
            decimal tiLePhuThu = AppConfig.GetTiLePhuThu();
            TxtSucChuaMax.Text  = sucChuaMax.ToString();
            TxtTiLePhuThu.Text  = (tiLePhuThu * 100m).ToString("0.##");
            TxtSucChuaMax.IsReadOnly  = !isAdmin;
            TxtTiLePhuThu.IsReadOnly  = !isAdmin;
            BtnLuuSucChua.IsEnabled   = isAdmin;
            BtnLuuSucChua.Opacity     = isAdmin ? 1.0 : 0.4;
            RefreshSucChuaNote(sucChuaMax, tiLePhuThu);
        }

        private void LoadLoaiKhachCombo()
        {
            using var ctx = new HotelDbContext();
            var list = ctx.LoaiKhachHangs.OrderBy(l => l.TenLoai).ToList();
            CboLoaiKhach.ItemsSource   = list;
            CboLoaiKhach.SelectedIndex = list.Any() ? 0 : -1;
        }

        private void RefreshHeSoNote(LoaiKhachHang loai)
        {
            TxtHeSoNote.Text =
                $"Tiền phòng × {loai.HeSoGia:0.####} khi có khách loại \"{loai.TenLoai}\"";
        }

        private void RefreshSucChuaNote(int sucChuaMax, decimal tiLePhuThu)
        {
            TxtSucChuaNote.Text =
                $"Số khách vượt sức chứa phòng → phụ thu {tiLePhuThu:P0}  |  " +
                $"Tối đa {sucChuaMax} người/phòng";
        }

        private void CboLoaiKhach_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboLoaiKhach.SelectedItem is not LoaiKhachHang loai) return;
            TxtHeSo.Text = loai.HeSoGia.ToString("0.####",
                System.Globalization.CultureInfo.InvariantCulture);
            RefreshHeSoNote(loai);
        }

        private void BtnLuuHeSo_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionManager.IsAdmin)
            {
                MessageBox.Show("Chỉ Admin mới có quyền thay đổi hệ số.", "Không có quyền",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CboLoaiKhach.SelectedItem is not LoaiKhachHang loai)
            {
                MessageBox.Show("Vui lòng chọn loại khách cần thay đổi.", "Chưa chọn loại",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtHeSo.Text.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal heSo)
                || heSo <= 0)
            {
                MessageBox.Show("Hệ số phải là số dương (ví dụ: 1.5).", "Giá trị không hợp lệ",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var ctx = new HotelDbContext();
            var entity = ctx.LoaiKhachHangs.Find(loai.MaCode);
            if (entity == null) return;
            entity.HeSoGia = heSo;
            ctx.SaveChanges();

            // Đồng bộ legacy config nếu là NuocNgoai
            if (loai.MaCode == "NuocNgoai")
                AppConfig.SetHeSoNuocNgoai(heSo);

            // Cập nhật lại combo
            LoadLoaiKhachCombo();
            var updated = (CboLoaiKhach.ItemsSource as List<LoaiKhachHang>)
                          ?.FirstOrDefault(l => l.MaCode == loai.MaCode);
            if (updated != null) CboLoaiKhach.SelectedItem = updated;

            MessageBox.Show($"Đã lưu hệ số cho \"{loai.TenLoai}\": ×{heSo:0.####}",
                "Lưu Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnLuuSucChua_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionManager.IsAdmin)
            {
                MessageBox.Show("Chỉ Admin mới có quyền thay đổi cài đặt này.", "Không có quyền",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxtSucChuaMax.Text.Trim(), out int sucChuaMax) || sucChuaMax < 1)
            {
                MessageBox.Show("Sức chứa tối đa phải là số nguyên dương.", "Giá trị không hợp lệ",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtTiLePhuThu.Text.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal tiLePct)
                || tiLePct < 0)
            {
                MessageBox.Show("Tỷ lệ phụ thu phải là số không âm (ví dụ: 25 cho 25%).",
                    "Giá trị không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Sức chứa tối đa phải >= sức chứa của loại phòng lớn nhất hiện có
            using (var ctx = new Data.HotelDbContext())
            {
                int maxSucChuaPhong = ctx.LoaiPhongs.Max(lp => (int?)lp.SucChua) ?? 0;
                if (sucChuaMax < maxSucChuaPhong)
                {
                    MessageBox.Show(
                        $"Sức chứa tối đa ({sucChuaMax}) không thể nhỏ hơn sức chứa " +
                        $"của loại phòng lớn nhất hiện có ({maxSucChuaPhong} người).\n" +
                        $"Vui lòng nhập giá trị >= {maxSucChuaPhong}.",
                        "Giá trị không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            decimal tiLePhuThu = tiLePct / 100m;
            AppConfig.SetSucChuaToiDa(sucChuaMax);
            AppConfig.SetTiLePhuThu(tiLePhuThu);
            RefreshSucChuaNote(sucChuaMax, tiLePhuThu);
            MessageBox.Show(
                $"Đã lưu cài đặt:\n• Sức chứa tối đa: {sucChuaMax} người\n• Phụ thu khi vượt sức chứa: {tiLePct:0.##}%",
                "Lưu Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
