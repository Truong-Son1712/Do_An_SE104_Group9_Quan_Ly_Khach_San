using System.Windows;
using System.Windows.Controls;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.KhachHang
{
    public partial class KhachHangDialog : Window
    {
        private readonly int? _maKH;
        private int? _maKHDuplicate; // mã KH trùng CMND (nếu có)

        public KhachHangDialog(int? maKH = null)
        {
            InitializeComponent();
            _maKH = maKH;
            LoadLoaiKhachCombo();
            if (maKH.HasValue) LoadData(maKH.Value);
        }

        private void LoadLoaiKhachCombo()
        {
            using var ctx = new HotelDbContext();
            var list = ctx.LoaiKhachHangs.OrderBy(l => l.TenLoai).ToList();
            CboLoaiKhach.ItemsSource = list;
            if (!_maKH.HasValue && list.Any())
                CboLoaiKhach.SelectedValue = list.First().MaLKH;
        }

        private void LoadData(int id)
        {
            using var ctx = new HotelDbContext();
            var kh = ctx.KhachHangs.Find(id);
            if (kh == null) return;

            TxtTitle.Text              = "Chỉnh Sửa Khách Hàng";
            TxtHoTen.Text              = kh.HoTen;
            TxtCMND.Text               = kh.CMND;
            TxtSDT.Text                = kh.SDT;
            TxtEmail.Text              = kh.Email;
            TxtQuocTich.Text           = kh.QuocTich;
            TxtDiaChi.Text             = kh.DiaChi;
            DpNgaySinh.SelectedDate    = kh.NgaySinh;
            CboLoaiKhach.SelectedValue = kh.MaLoaiKH;

            foreach (ComboBoxItem item in CboGioiTinh.Items)
                if (item.Tag?.ToString() == kh.GioiTinh) { item.IsSelected = true; break; }
        }

        // ── Kiểm tra trùng CMND ngay khi rời ô nhập ────────────────────────
        private void TxtCMND_LostFocus(object sender, RoutedEventArgs e)
        {
            string cmnd = TxtCMND.Text.Trim();
            PnlDuplicate.Visibility = Visibility.Collapsed;
            _maKHDuplicate          = null;

            if (string.IsNullOrEmpty(cmnd)) return;

            using var ctx = new HotelDbContext();
            var existing = ctx.KhachHangs
                .FirstOrDefault(k => k.CMND == cmnd && k.MaKH != (_maKH ?? -1));

            if (existing != null)
            {
                _maKHDuplicate          = existing.MaKH;
                TxtDuplicateInfo.Text   = $"Khách hàng: {existing.HoTen}  |  SDT: {existing.SDT ?? "—"}  |  Mã KH: #{existing.MaKH}";
                PnlDuplicate.Visibility = Visibility.Visible;
            }
        }

        // ── Mở hồ sơ khách hàng trùng ──────────────────────────────────────
        private void BtnXemKhachCu_Click(object sender, RoutedEventArgs e)
        {
            if (_maKHDuplicate == null) return;
            var dlg = new KhachHangDialog(_maKHDuplicate);
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            PnlError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(TxtHoTen.Text))
            { ShowError("Vui lòng nhập họ tên."); return; }

            string cmnd = TxtCMND.Text.Trim();
            if (string.IsNullOrWhiteSpace(cmnd))
            { ShowError("Vui lòng nhập CMND/CCCD."); return; }

            string gioiTinh = (CboGioiTinh.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Nam";
            int    maLoaiKH = CboLoaiKhach.SelectedValue is int v ? v
                            : (CboLoaiKhach.SelectedItem as LoaiKhachHang)?.MaLKH ?? 1;

            try
            {
                using var ctx = new HotelDbContext();

                // Kiểm tra trùng CMND trước khi lưu (bảo vệ lần cuối)
                var existing = ctx.KhachHangs
                    .FirstOrDefault(k => k.CMND == cmnd && k.MaKH != (_maKH ?? -1));

                if (existing != null)
                {
                    ShowError($"CMND/CCCD \"{cmnd}\" đã thuộc về khách hàng \"{existing.HoTen}\" (Mã KH: #{existing.MaKH}). Không thể lưu trùng.");
                    _maKHDuplicate          = existing.MaKH;
                    TxtDuplicateInfo.Text   = $"Khách hàng: {existing.HoTen}  |  SDT: {existing.SDT ?? "—"}  |  Mã KH: #{existing.MaKH}";
                    PnlDuplicate.Visibility = Visibility.Visible;
                    return;
                }

                if (_maKH.HasValue)
                {
                    var kh = ctx.KhachHangs.Find(_maKH.Value);
                    if (kh == null) return;
                    Map(kh, gioiTinh, maLoaiKH);
                }
                else
                {
                    var kh = new Models.KhachHang();
                    Map(kh, gioiTinh, maLoaiKH);
                    ctx.KhachHangs.Add(kh);
                }

                ctx.SaveChanges();
                DialogResult = true;
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void Map(Models.KhachHang kh, string gioiTinh, int maLoaiKH)
        {
            kh.HoTen    = TxtHoTen.Text.Trim();
            kh.CMND     = TxtCMND.Text.Trim();
            kh.SDT      = TxtSDT.Text.Trim();
            kh.Email    = TxtEmail.Text.Trim();
            kh.QuocTich = string.IsNullOrWhiteSpace(TxtQuocTich.Text) ? "Việt Nam" : TxtQuocTich.Text.Trim();
            kh.DiaChi   = TxtDiaChi.Text.Trim();
            kh.GioiTinh = gioiTinh;
            kh.MaLoaiKH = maLoaiKH;
            kh.NgaySinh = DpNgaySinh.SelectedDate;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
        private void ShowError(string msg) { TxtError.Text = msg; PnlError.Visibility = Visibility.Visible; }
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
    }
}
