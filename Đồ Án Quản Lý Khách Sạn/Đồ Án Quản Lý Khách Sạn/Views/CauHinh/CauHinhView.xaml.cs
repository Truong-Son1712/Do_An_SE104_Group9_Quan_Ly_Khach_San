using System.Windows;
using System.Windows.Controls;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.CauHinh
{
    public partial class CauHinhView : UserControl
    {
        public CauHinhView() => InitializeComponent();

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Cấu hình: Admin hoặc ai có quyền CauHinhHeThong mới được sửa
            bool canEdit = SessionManager.HasPermission(Helpers.Quyen.CauHinhHeThong);
            TxtHeSo.IsReadOnly        = !canEdit;
            CboLoaiKhach.IsEnabled    = canEdit;
            BtnLuuHeSo.IsEnabled      = canEdit;
            BtnLuuHeSo.Opacity        = canEdit ? 1.0 : 0.4;
            TxtSucChuaMax.IsReadOnly  = !canEdit;
            TxtTiLePhuThu.IsReadOnly  = !canEdit;
            BtnLuuSucChua.IsEnabled   = canEdit;
            BtnLuuSucChua.Opacity     = canEdit ? 1.0 : 0.4;
            TxtBoSungGia.IsReadOnly   = !canEdit;
            BtnLuuBoSung.IsEnabled    = canEdit;
            BtnLuuBoSung.Opacity      = canEdit ? 1.0 : 0.4;
            TxtTiLeCoc.IsReadOnly     = !canEdit;
            BtnLuuCoc.IsEnabled       = canEdit;
            BtnLuuCoc.Opacity         = canEdit ? 1.0 : 0.4;
            TxtBoSungDV.IsReadOnly    = !canEdit;
            BtnLuuBoSungDV.IsEnabled  = canEdit;
            BtnLuuBoSungDV.Opacity    = canEdit ? 1.0 : 0.4;
            TxtVAT.IsReadOnly         = !canEdit;
            BtnLuuVAT.IsEnabled       = canEdit;
            BtnLuuVAT.Opacity         = canEdit ? 1.0 : 0.4;

            LoadLoaiKhachCombo();

            int sucChuaMax     = AppConfig.GetSucChuaToiDa();
            decimal tiLePhuThu = AppConfig.GetTiLePhuThu();
            TxtSucChuaMax.Text  = sucChuaMax.ToString();
            TxtTiLePhuThu.Text  = (tiLePhuThu * 100m).ToString("0.##");
            RefreshSucChuaNote(sucChuaMax, tiLePhuThu);

            decimal boSung = AppConfig.GetTiLeBoSungGia();
            TxtBoSungGia.Text = boSung.ToString("0.##");
            RefreshBoSungNote(boSung);

            decimal boSungDV = AppConfig.GetTiLeBoSungGiaDV();
            TxtBoSungDV.Text = boSungDV.ToString("0.##");
            RefreshBoSungDVNote(boSungDV);

            decimal vat = AppConfig.GetVAT();
            TxtVAT.Text = vat.ToString("0.##");
            RefreshVATNote(vat);

            decimal coc = AppConfig.GetTiLeCoc();
            TxtTiLeCoc.Text = coc.ToString("0.##");
            RefreshCocNote(coc);
        }

        private void LoadLoaiKhachCombo()
        {
            using var ctx = new HotelDbContext();
            var list = ctx.LoaiKhachHangs.OrderBy(l => l.TenLoai).ToList();
            CboLoaiKhach.ItemsSource   = list;
            CboLoaiKhach.SelectedIndex = list.Any() ? 0 : -1;
        }

        private void CboLoaiKhach_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboLoaiKhach.SelectedItem is not LoaiKhachHang loai) return;
            TxtHeSo.Text = loai.HeSoGia.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);
            TxtHeSoNote.Text = $"Tiền phòng × {loai.HeSoGia:0.####} khi có khách loại \"{loai.TenLoai}\"";
        }

        private void BtnLuuHeSo_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionManager.HasPermission(Helpers.Quyen.CauHinhHeThong)) { ShowWarn("Bạn không có quyền thay đổi hệ số."); return; }
            if (CboLoaiKhach.SelectedItem is not LoaiKhachHang loai) { ShowWarn("Vui lòng chọn loại khách."); return; }
            if (!decimal.TryParse(TxtHeSo.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal heSo) || heSo < 1m || heSo > 2m)
            { ShowWarn("Hệ số loại khách phải trong khoảng từ 1 đến 2."); return; }

            using var ctx = new HotelDbContext();
            var entity = ctx.LoaiKhachHangs.Find(loai.MaLKH);
            if (entity == null) return;
            entity.HeSoGia = heSo;
            ctx.SaveChanges();
            LoadLoaiKhachCombo();
            var updated = (CboLoaiKhach.ItemsSource as List<LoaiKhachHang>)?.FirstOrDefault(l => l.MaCode == loai.MaCode);
            if (updated != null) CboLoaiKhach.SelectedItem = updated;
            MessageBox.Show($"Đã lưu hệ số cho \"{loai.TenLoai}\": ×{heSo:0.####}", "Lưu Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnLuuSucChua_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionManager.HasPermission(Helpers.Quyen.CauHinhHeThong)) { ShowWarn("Bạn không có quyền thay đổi cài đặt này."); return; }
            if (!int.TryParse(TxtSucChuaMax.Text.Trim(), out int sc) || sc < 1 || sc > 20) { ShowWarn("Sức chứa tối đa phải trong khoảng từ 1 đến 20 người."); return; }
            if (!decimal.TryParse(TxtTiLePhuThu.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal pct) || pct < 0 || pct > 100)
            { ShowWarn("Tỷ lệ phụ thu sức chứa phải trong khoảng từ 0% đến 100%."); return; }

            using (var ctx = new HotelDbContext())
            {
                int maxSC = ctx.LoaiPhongs.Max(l => (int?)l.SucChua) ?? 0;
                if (sc < maxSC) { ShowWarn($"Sức chứa tối đa ({sc}) không thể nhỏ hơn loại phòng lớn nhất hiện có ({maxSC} người)."); return; }
            }
            AppConfig.SetSucChuaToiDa(sc);
            AppConfig.SetTiLePhuThu(pct / 100m);
            RefreshSucChuaNote(sc, pct / 100m);
            MessageBox.Show($"Đã lưu:\n• Sức chứa tối đa: {sc} người\n• Phụ thu vượt sức chứa: {pct:0.##}%", "Lưu Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnLuuBoSung_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionManager.HasPermission(Helpers.Quyen.CauHinhHeThong)) { ShowWarn("Bạn không có quyền điều chỉnh giá."); return; }
            if (!decimal.TryParse(TxtBoSungGia.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal tiLe) || tiLe < -100 || tiLe > 100)
            { ShowWarn("Tỉ lệ điều chỉnh giá phòng phải trong khoảng từ -100% đến 100%."); return; }

            AppConfig.SetTiLeBoSungGia(tiLe);
            RefreshBoSungNote(tiLe);
            string msg = tiLe == 0 ? "Đã về giá gốc." : tiLe > 0 ? $"Đã tăng giá {tiLe:0.##}% toàn bộ phòng." : $"Đã giảm giá {Math.Abs(tiLe):0.##}% toàn bộ phòng.";
            MessageBox.Show(msg, "Lưu Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnLuuVAT_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionManager.HasPermission(Helpers.Quyen.CauHinhHeThong)) { ShowWarn("Bạn không có quyền thay đổi thuế VAT."); return; }
            if (!decimal.TryParse(TxtVAT.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal vat) || vat < 0 || vat > 20)
            { ShowWarn("Thuế suất VAT phải trong khoảng từ 0% đến 20%."); return; }

            AppConfig.SetVAT(vat);
            RefreshVATNote(vat);
            MessageBox.Show($"Đã lưu thuế suất VAT: {vat:0.##}%", "Lưu Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshSucChuaNote(int sc, decimal tiLe) =>
            TxtSucChuaNote.Text = $"Số khách vượt sức chứa phòng → phụ thu {tiLe:P0}  |  Tối đa {sc} người/phòng";

        private void RefreshBoSungNote(decimal tiLe) =>
            TxtBoSungNote.Text = tiLe == 0 ? "Hiện đang dùng giá gốc (không điều chỉnh)"
                               : tiLe > 0  ? $"Tất cả loại phòng đang tăng {tiLe:0.##}% so với giá gốc"
                                           : $"Tất cả loại phòng đang giảm {Math.Abs(tiLe):0.##}% so với giá gốc";

        private void RefreshBoSungDVNote(decimal tiLe) =>
            TxtBoSungDVNote.Text = tiLe == 0 ? "Hiện đang dùng giá gốc dịch vụ (không điều chỉnh)"
                                 : tiLe > 0  ? $"Tất cả dịch vụ đang tăng {tiLe:0.##}% so với giá gốc"
                                             : $"Tất cả dịch vụ đang giảm {Math.Abs(tiLe):0.##}% so với giá gốc";

        private void BtnLuuBoSungDV_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionManager.HasPermission(Helpers.Quyen.CauHinhHeThong)) { ShowWarn("Bạn không có quyền điều chỉnh giá dịch vụ."); return; }
            if (!decimal.TryParse(TxtBoSungDV.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal tiLe) || tiLe < -100 || tiLe > 100)
            { ShowWarn("Tỉ lệ điều chỉnh giá dịch vụ phải trong khoảng từ -100% đến 100%."); return; }

            AppConfig.SetTiLeBoSungGiaDV(tiLe);
            RefreshBoSungDVNote(tiLe);
            string msg = tiLe == 0 ? "Đã về giá gốc dịch vụ." :
                         tiLe > 0  ? $"Đã tăng giá {tiLe:0.##}% toàn bộ dịch vụ." :
                                     $"Đã giảm giá {Math.Abs(tiLe):0.##}% toàn bộ dịch vụ.";
            MessageBox.Show(msg, "Lưu Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshVATNote(decimal vat) =>
            TxtVATNote.Text = $"Áp dụng {vat:0.##}% VAT lên toàn bộ tiền phòng và dịch vụ khi lập hóa đơn";

        private void BtnLuuCoc_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionManager.HasPermission(Helpers.Quyen.CauHinhHeThong)) { ShowWarn("Bạn không có quyền thay đổi tỉ lệ tiền cọc."); return; }
            if (!decimal.TryParse(TxtTiLeCoc.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal coc) || coc < 0 || coc > 100)
            { ShowWarn("Tỉ lệ tiền cọc phải trong khoảng từ 0% đến 100%."); return; }

            AppConfig.SetTiLeCoc(coc);
            RefreshCocNote(coc);
            MessageBox.Show($"Đã lưu tỉ lệ tiền cọc: {coc:0.##}%", "Lưu Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshCocNote(decimal coc) =>
            TxtCocNote.Text = $"Tiền cọc tự động tính {coc:0.##}% giá phòng dự tính khi tạo đặt phòng";

        private static void ShowWarn(string msg) =>
            MessageBox.Show(msg, "Không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
