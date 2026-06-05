using System.ComponentModel;
using System.Windows;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.NhanVien
{
    public partial class LoaiNhanVienDialog : Window
    {
        private readonly int? _maLoaiNV;
        private readonly bool _isBuiltIn;

        private class QuyenItem : INotifyPropertyChanged
        {
            private bool _duocCap;
            public string MaQuyen  { get; set; } = "";
            public string TenQuyen { get; set; } = "";
            public string MoTa     { get; set; } = "";
            public bool DuocCap
            {
                get => _duocCap;
                set { _duocCap = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DuocCap))); }
            }
            public event PropertyChangedEventHandler? PropertyChanged;
        }

        private static readonly Dictionary<string, string> MoTaQuyen = new()
        {
            [Quyen.HuyHoaDon]       = "Hủy hóa đơn và rollback đặt phòng",
            [Quyen.XemBaoCao]       = "Xem báo cáo doanh thu và thống kê",
            [Quyen.QuanLyLoaiPhong] = "Thêm, sửa, xóa loại phòng và giá",
            [Quyen.QuanLyDichVu]    = "Thêm, sửa, xóa loại dịch vụ và giá",
            [Quyen.QuanLyMaGiamGia] = "Tạo và quản lý mã giảm giá",
            [Quyen.QuanLyNhanVien]  = "Xem và quản lý danh sách nhân viên",
            [Quyen.QuanLyLoaiKH]    = "CRUD loại khách hàng và hệ số giá",
            [Quyen.CauHinhHeThong]  = "Truy cập và chỉnh sửa cấu hình hệ thống",
        };

        public LoaiNhanVienDialog(int? maLoaiNV = null)
        {
            InitializeComponent();
            _maLoaiNV = maLoaiNV;

            if (maLoaiNV.HasValue)
            {
                using var ctx = new HotelDbContext();
                var loai = ctx.LoaiNhanViens.Find(maLoaiNV.Value);
                if (loai != null)
                {
                    _isBuiltIn = loai.IsBuiltIn;
                    TxtTitle.Text       = $"Chỉnh Sửa – {loai.TenLoai}";
                    TxtTenLoai.Text     = loai.TenLoai;
                    TxtVaiTroCode.Text  = loai.VaiTroCode;
                    TxtMoTa.Text        = loai.MoTa;
                    // Chỉ khóa VaiTroCode (không được đổi mã nội bộ), tên và mô tả vẫn sửa được
                    TxtVaiTroCode.IsReadOnly = loai.IsBuiltIn;
                }
            }
            LoadQuyens();
        }

        private void LoadQuyens()
        {
            HashSet<string> current = new();
            if (_maLoaiNV.HasValue)
            {
                using var ctx = new HotelDbContext();
                current = ctx.LoaiNhanVienQuyens
                    .Where(q => q.MaLoaiNV == _maLoaiNV.Value)
                    .Select(q => q.MaQuyen)
                    .ToHashSet();
            }
            else
            {
                // Mới tạo: mặc định như LeTan (không có quyền gì)
                current = Quyen.MacDinhLeTan;
            }

            IcQuyens.ItemsSource = Quyen.TenQuyen.Select(kv => new QuyenItem
            {
                MaQuyen  = kv.Key,
                TenQuyen = kv.Value,
                MoTa     = MoTaQuyen.TryGetValue(kv.Key, out var m) ? m : "",
                DuocCap  = current.Contains(kv.Key)
            }).ToList();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            PnlError.Visibility = Visibility.Collapsed;

            string tenLoai    = TxtTenLoai.Text.Trim();
            string vaiTroCode = TxtVaiTroCode.Text.Trim();

            if (string.IsNullOrEmpty(tenLoai))
            { ShowError("Vui lòng nhập tên loại nhân viên."); return; }
            if (!_maLoaiNV.HasValue) // chỉ validate VaiTroCode khi tạo mới
            {
                if (string.IsNullOrEmpty(vaiTroCode) || vaiTroCode.Contains(" "))
                { ShowError("Mã vai trò không được để trống và không được có khoảng trắng."); return; }
            }
            // Kiểm tra tên không trùng với loại khác
            using (var chkCtx = new HotelDbContext())
            {
                bool trungTen = chkCtx.LoaiNhanViens.Any(l =>
                    l.TenLoai == tenLoai &&
                    (_maLoaiNV == null || l.MaLoaiNV != _maLoaiNV.Value));
                if (trungTen)
                { ShowError($"Tên loại \"{tenLoai}\" đã tồn tại. Vui lòng dùng tên khác."); return; }
            }

            var items = (IcQuyens.ItemsSource as IEnumerable<QuyenItem>)?.ToList() ?? new();

            try
            {
                using var ctx = new HotelDbContext();

                if (_maLoaiNV.HasValue)
                {
                    // Cập nhật
                    var loai = ctx.LoaiNhanViens.Find(_maLoaiNV.Value);
                    if (loai == null) return;
                    // Tên và mô tả luôn được sửa; chỉ VaiTroCode là bất biến
                    if (!string.IsNullOrEmpty(tenLoai)) loai.TenLoai = tenLoai;
                    loai.MoTa = TxtMoTa.Text.Trim();

                    // Đồng bộ quyền mặc định của loại
                    var old = ctx.LoaiNhanVienQuyens.Where(q => q.MaLoaiNV == _maLoaiNV.Value).ToList();
                    ctx.LoaiNhanVienQuyens.RemoveRange(old);
                    var newQuyens = items.Where(i => i.DuocCap).Select(i => i.MaQuyen).ToList();
                    foreach (var q in newQuyens)
                        ctx.LoaiNhanVienQuyens.Add(new LoaiNhanVienQuyen { MaLoaiNV = _maLoaiNV.Value, MaQuyen = q });

                    // Reset quyền cá nhân của tất cả nhân viên thuộc loại này
                    var maNVList = ctx.NhanViens
                        .Where(n => n.VaiTro == loai.VaiTroCode)
                        .Select(n => n.MaNV)
                        .ToList();
                    foreach (var maNV in maNVList)
                    {
                        var oldNVQ = ctx.NhanVienQuyens.Where(q => q.MaNV == maNV).ToList();
                        ctx.NhanVienQuyens.RemoveRange(oldNVQ);
                        foreach (var q in newQuyens)
                            ctx.NhanVienQuyens.Add(new NhanVienQuyen { MaNV = maNV, MaQuyen = q });
                    }
                    ctx.SaveChanges();

                    // Reload quyền nếu người đang đăng nhập thuộc loại này
                    if (maNVList.Contains(SessionManager.CurrentUser?.MaNV ?? -1))
                        SessionManager.ReloadPermissions();
                }
                else
                {
                    // Kiểm tra VaiTroCode trùng
                    if (ctx.LoaiNhanViens.Any(l => l.VaiTroCode == vaiTroCode))
                    { ShowError($"Mã vai trò \"{vaiTroCode}\" đã tồn tại."); return; }

                    var loai = new LoaiNhanVien
                    {
                        TenLoai    = tenLoai,
                        VaiTroCode = vaiTroCode,
                        MoTa       = TxtMoTa.Text.Trim(),
                        IsBuiltIn  = false
                    };
                    ctx.LoaiNhanViens.Add(loai);
                    ctx.SaveChanges();

                    foreach (var item in items.Where(i => i.DuocCap))
                        ctx.LoaiNhanVienQuyens.Add(new LoaiNhanVienQuyen { MaLoaiNV = loai.MaLoaiNV, MaQuyen = item.MaQuyen });
                }

                ctx.SaveChanges();
                DialogResult = true;
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void ShowError(string msg) { TxtError.Text = msg; PnlError.Visibility = Visibility.Visible; }
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
    }
}
