using System.ComponentModel;
using System.Windows;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.NhanVien
{
    public partial class PhanQuyenDialog : Window
    {
        private readonly int    _maNV;
        private readonly string _vaiTro;

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

        // Mô tả chi tiết cho từng quyền
        private static readonly Dictionary<string, string> MoTaQuyen = new()
        {
            [Quyen.HuyHoaDon]       = "Cho phép hủy hóa đơn và rollback trạng thái đặt phòng",
            [Quyen.XemBaoCao]       = "Xem báo cáo doanh thu, thống kê công suất phòng",
            [Quyen.QuanLyLoaiPhong] = "Thêm, sửa, xóa loại phòng và giá",
            [Quyen.QuanLyDichVu]    = "Thêm, sửa, xóa loại dịch vụ và giá",
            [Quyen.QuanLyMaGiamGia] = "Tạo và quản lý mã giảm giá",
            [Quyen.QuanLyNhanVien]  = "Xem và quản lý danh sách nhân viên",
            [Quyen.QuanLyLoaiKH]    = "Thêm, sửa loại khách hàng và hệ số giá",
        };

        public PhanQuyenDialog(int maNV, string hoTen, string vaiTro)
        {
            InitializeComponent();
            _maNV   = maNV;
            _vaiTro = vaiTro;

            TxtTitle.Text       = $"Phân Quyền – {hoTen}";
            TxtNhanVienInfo.Text = hoTen;
            TxtVaiTroInfo.Text   = $"Vai trò: {(vaiTro == "QuanLy" ? "Quản Lý" : "Lễ Tân")}";

            LoadQuyens();
        }

        private void LoadQuyens()
        {
            using var ctx = new HotelDbContext();
            var currentQuyens = ctx.NhanVienQuyens
                .Where(q => q.MaNV == _maNV)
                .Select(q => q.MaQuyen)
                .ToHashSet();

            IcQuyens.ItemsSource = Quyen.TenQuyen.Select(kv => new QuyenItem
            {
                MaQuyen  = kv.Key,
                TenQuyen = kv.Value,
                MoTa     = MoTaQuyen.TryGetValue(kv.Key, out var m) ? m : "",
                DuocCap  = currentQuyens.Contains(kv.Key)
            }).ToList();
        }

        private void BtnMacDinh_Click(object sender, RoutedEventArgs e)
        {
            var defaults = _vaiTro == "QuanLy" ? Quyen.MacDinhQuanLy : Quyen.MacDinhLeTan;
            if (IcQuyens.ItemsSource is IEnumerable<QuyenItem> items)
                foreach (var item in items)
                    item.DuocCap = defaults.Contains(item.MaQuyen);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            PnlError.Visibility = Visibility.Collapsed;
            try
            {
                var items = (IcQuyens.ItemsSource as IEnumerable<QuyenItem>)?.ToList() ?? new();

                using var ctx = new HotelDbContext();
                // Xóa toàn bộ quyền cũ
                var old = ctx.NhanVienQuyens.Where(q => q.MaNV == _maNV).ToList();
                ctx.NhanVienQuyens.RemoveRange(old);

                // Thêm lại những quyền được tick
                foreach (var item in items.Where(i => i.DuocCap))
                    ctx.NhanVienQuyens.Add(new NhanVienQuyen { MaNV = _maNV, MaQuyen = item.MaQuyen });

                ctx.SaveChanges();

                // Nếu đang phân quyền cho người dùng hiện tại → reload quyền trong session
                if (SessionManager.CurrentUser?.MaNV == _maNV)
                    SessionManager.ReloadPermissions();

                MessageBox.Show("Đã lưu quyền thành công.", "Thành Công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                TxtError.Text       = ex.Message;
                PnlError.Visibility = Visibility.Visible;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
    }
}
