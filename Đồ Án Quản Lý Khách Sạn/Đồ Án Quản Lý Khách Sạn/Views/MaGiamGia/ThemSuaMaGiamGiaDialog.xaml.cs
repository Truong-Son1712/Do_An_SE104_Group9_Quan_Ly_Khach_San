using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;
using Microsoft.EntityFrameworkCore;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.MaGiamGia
{
    public partial class ThemSuaMaGiamGiaDialog : Window
    {
        private readonly int? _maGG;
        private List<LoaiPhong> _loaiPhongs = new();
        private List<LoaiDichVu> _loaiDichVus = new();
        private readonly List<(ComboBox CbLoai, ComboBox CbTen, TextBox TxtTiLe, int? MaChiTiet)> _rows = new();

        public ThemSuaMaGiamGiaDialog(int? maGG = null)
        {
            InitializeComponent();
            _maGG = maGG;
            TxtTitle.Text = maGG.HasValue ? "Sửa Mã Giảm Giá" : "Thêm Mã Giảm Giá";
            LoadReferenceData();
            if (maGG.HasValue) LoadEdit(maGG.Value);
            UpdateSoLuongState();
        }

        private void LoadReferenceData()
        {
            using var ctx = new HotelDbContext();
            _loaiPhongs  = ctx.LoaiPhongs.OrderBy(l => l.TenLoaiPhong).ToList();
            _loaiDichVus = ctx.LoaiDichVus.Where(l => l.IsActive).OrderBy(l => l.TenLoaiDV).ToList();
        }

        private void LoadEdit(int maGG)
        {
            using var ctx = new HotelDbContext();
            var mg = ctx.MaGiamGias.Include(m => m.ChiTiets).FirstOrDefault(m => m.MaGG == maGG);
            if (mg == null) return;

            TxtTenMa.Text           = mg.TenMa;
            TxtMoTa.Text            = mg.MoTa ?? "";
            DpkBatDau.SelectedDate  = mg.NgayBatDau;
            DpkKetThuc.SelectedDate = mg.NgayKetThuc;

            if (mg.SoLuongToiDa.HasValue)
            {
                ChkVoHan.IsChecked  = false;
                TxtSoLuong.Text     = mg.SoLuongToiDa.Value.ToString();
                TxtSoLuong.IsEnabled = true;
            }
            else
            {
                ChkVoHan.IsChecked  = true;
                TxtSoLuong.IsEnabled = false;
            }

            foreach (var ct in mg.ChiTiets)
                AddRow(ct.LoaiApDung, ct.MaLoai, ct.TiLeGiam, ct.MaChiTiet);
        }

        // Bật/tắt ô nhập số lượng theo checkbox vô hạn
        private void ChkVoHan_Changed(object sender, RoutedEventArgs e) => UpdateSoLuongState();

        private void UpdateSoLuongState()
        {
            bool voHan = ChkVoHan.IsChecked == true;
            TxtSoLuong.IsEnabled = !voHan;
            if (voHan) TxtSoLuong.Text = "";
        }

        private void BtnThemQuyTac_Click(object sender, RoutedEventArgs e)
            => AddRow("LoaiPhong", 0, 0, null);

        private void AddRow(string loaiApDung, int maLoai, decimal tiLeGiam, int? maChiTiet)
        {
            var row = new Grid { Margin = new Thickness(0, 0, 0, 6) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(34) });

            var cbLoai = new ComboBox { Margin = new Thickness(0, 0, 6, 0) };
            cbLoai.Items.Add(new ComboBoxItem { Content = "Loại Phòng",   Tag = "LoaiPhong" });
            cbLoai.Items.Add(new ComboBoxItem { Content = "Loại Dịch Vụ", Tag = "LoaiDichVu" });
            cbLoai.SelectedIndex = loaiApDung == "LoaiDichVu" ? 1 : 0;
            Grid.SetColumn(cbLoai, 0);

            var cbTen = new ComboBox { Margin = new Thickness(0, 0, 6, 0) };
            Grid.SetColumn(cbTen, 1);
            FillTenCombo(cbTen, loaiApDung, maLoai);

            cbLoai.SelectionChanged += (s, _) =>
            {
                var sel = (cbLoai.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "LoaiPhong";
                FillTenCombo(cbTen, sel, 0);
            };

            var txtTiLe = new TextBox
            {
                Text = tiLeGiam > 0 ? tiLeGiam.ToString("0.##") : "",
                Tag  = "VD: 20 → giảm 20%",
                Margin = new Thickness(0, 0, 6, 0),
                VerticalContentAlignment = VerticalAlignment.Center,
                ToolTip = "Nhập số từ 0.01 đến 100. VD: 20 = giảm 20% (khách trả 80%)"
            };
            txtTiLe.SetValue(System.Windows.Controls.Validation.ErrorTemplateProperty, null);
            Grid.SetColumn(txtTiLe, 2);

            var btnXoa = new Button
            {
                Content = "✕",
                Width = 28, Height = 28,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE")),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828")),
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                FontWeight = FontWeights.Bold
            };
            Grid.SetColumn(btnXoa, 3);

            row.Children.Add(cbLoai);
            row.Children.Add(cbTen);
            row.Children.Add(txtTiLe);
            row.Children.Add(btnXoa);

            var rowTuple = (cbLoai, cbTen, txtTiLe, maChiTiet);
            _rows.Add(rowTuple);
            PnlQuyTacRows.Children.Add(row);
            TxtEmptyRules.Visibility = Visibility.Collapsed;

            btnXoa.Click += (s, _) =>
            {
                PnlQuyTacRows.Children.Remove(row);
                _rows.Remove(rowTuple);
                if (_rows.Count == 0) TxtEmptyRules.Visibility = Visibility.Visible;
            };
        }

        private void FillTenCombo(ComboBox cb, string loaiApDung, int selectedMaLoai)
        {
            cb.Items.Clear();
            if (loaiApDung == "LoaiPhong")
            {
                foreach (var lp in _loaiPhongs)
                {
                    var item = new ComboBoxItem { Content = lp.TenLoaiPhong, Tag = lp.MaLoaiPhong };
                    cb.Items.Add(item);
                    if (lp.MaLoaiPhong == selectedMaLoai) cb.SelectedItem = item;
                }
            }
            else
            {
                foreach (var ldv in _loaiDichVus)
                {
                    var item = new ComboBoxItem { Content = ldv.TenLoaiDV, Tag = ldv.MaLoaiDV };
                    cb.Items.Add(item);
                    if (ldv.MaLoaiDV == selectedMaLoai) cb.SelectedItem = item;
                }
            }
            if (cb.SelectedIndex < 0 && cb.Items.Count > 0) cb.SelectedIndex = 0;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            PnlError.Visibility = Visibility.Collapsed;

            string tenMa = TxtTenMa.Text.Trim().ToUpper();
            if (string.IsNullOrEmpty(tenMa))              { ShowError("Vui lòng nhập tên mã giảm giá."); return; }
            if (!DpkBatDau.SelectedDate.HasValue)          { ShowError("Vui lòng chọn ngày bắt đầu."); return; }
            if (!DpkKetThuc.SelectedDate.HasValue)         { ShowError("Vui lòng chọn ngày kết thúc."); return; }
            if (DpkKetThuc.SelectedDate <= DpkBatDau.SelectedDate)
            { ShowError("Ngày kết thúc phải sau ngày bắt đầu."); return; }
            if (_rows.Count == 0) { ShowError("Vui lòng thêm ít nhất một quy tắc giảm giá."); return; }

            // Số lượng tối đa
            int? soLuongToiDa = null;
            if (ChkVoHan.IsChecked != true)
            {
                if (!int.TryParse(TxtSoLuong.Text.Trim(), out int sl) || sl <= 0)
                { ShowError("Số lượt tối đa phải là số nguyên dương."); return; }
                soLuongToiDa = sl;
            }

            // Validate từng dòng
            var chiTiets = new List<(string LoaiApDung, int MaLoai, decimal TiLeGiam, int? MaChiTiet)>();
            foreach (var (cbLoai, cbTen, txtTiLe, maCT) in _rows)
            {
                string loai = (cbLoai.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "LoaiPhong";
                int maLoai  = (int)((cbTen.SelectedItem as ComboBoxItem)?.Tag ?? 0);
                if (maLoai == 0) { ShowError("Vui lòng chọn đầy đủ tên loại phòng/dịch vụ cho mỗi quy tắc."); return; }
                if (!decimal.TryParse(txtTiLe.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal tiLe)
                    || tiLe <= 0 || tiLe > 100)
                { ShowError("Tỉ lệ giảm phải là số từ 0.01 đến 100."); return; }
                chiTiets.Add((loai, maLoai, tiLe, maCT));
            }

            try
            {
                using var ctx = new HotelDbContext();
                var now = DateTime.Now;

                // Kiểm tra trùng tên với mã ĐANG HOẠT ĐỘNG (Active + trong thời hạn + còn lượt)
                // Bỏ qua mã đang sửa, bỏ qua mã đã Inactive/hết hạn
                bool trungActive = ctx.MaGiamGias
                    .Where(m => m.TenMa == tenMa
                             && m.MaGG  != (_maGG ?? -1)
                             && m.TrangThai == "Active"
                             && m.NgayBatDau  <= now
                             && m.NgayKetThuc >= now)
                    .Any();
                if (trungActive)
                {
                    ShowError($"Tên mã \"{tenMa}\" đang được dùng bởi một mã khác đang hoạt động.");
                    return;
                }

                Models.MaGiamGia entity;
                if (_maGG.HasValue)
                {
                    entity = ctx.MaGiamGias.Include(m => m.ChiTiets).First(m => m.MaGG == _maGG);
                    ctx.ChiTietMaGiamGias.RemoveRange(entity.ChiTiets);
                }
                else
                {
                    entity = new Models.MaGiamGia
                    {
                        NgayTao = DateTime.Now,
                        MaNVTao = SessionManager.CurrentUser?.MaNV
                    };
                    ctx.MaGiamGias.Add(entity);
                }

                entity.TenMa        = tenMa;
                entity.MoTa         = TxtMoTa.Text.Trim();
                entity.NgayBatDau   = DpkBatDau.SelectedDate!.Value;
                entity.NgayKetThuc  = DpkKetThuc.SelectedDate!.Value;
                entity.SoLuongToiDa = soLuongToiDa;
                ctx.SaveChanges();

                foreach (var (loai, maLoai, tiLe, _) in chiTiets)
                {
                    ctx.ChiTietMaGiamGias.Add(new Models.ChiTietMaGiamGia
                    {
                        MaGG       = entity.MaGG,
                        LoaiApDung = loai,
                        MaLoai     = maLoai,
                        TiLeGiam   = tiLe
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
