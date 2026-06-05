using System.Windows;
using System.Windows.Controls;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
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
            BuildVaiTroCombo();
            if (maNV.HasValue) LoadData(maNV.Value);
            else PnlMatKhau.Visibility = Visibility.Visible;
        }

        // Load danh sách loại nhân viên từ DB
        private void BuildVaiTroCombo()
        {
            CboVaiTro.Items.Clear();
            using var ctx = new HotelDbContext();

            // Admin thấy tất cả loại (trừ Admin); QuanLy chỉ thấy loại không phải Admin/QuanLy
            var loais = ctx.LoaiNhanViens
                .OrderBy(l => l.MaLoaiNV)
                .ToList()
                .Where(l => SessionManager.IsAdmin
                    ? true                                            // Admin thấy tất cả kể cả Admin
                    : l.VaiTroCode != "Admin" && l.VaiTroCode != "QuanLy") // QuanLy chỉ tạo LeTan/custom
                .ToList();

            foreach (var loai in loais)
                CboVaiTro.Items.Add(new ComboBoxItem
                {
                    Content = loai.TenLoai,
                    Tag     = loai.VaiTroCode
                });

            CboVaiTro.SelectedIndex = CboVaiTro.Items.Count > 0 ? 0 : -1;
        }

        private void LoadData(int id)
        {
            using var ctx = new HotelDbContext();
            var nv = ctx.NhanViens.Find(id);
            if (nv == null) return;

            TxtTitle.Text         = "Chỉnh Sửa Nhân Viên";
            TxtHoTen.Text         = nv.HoTen;
            TxtTaiKhoan.Text      = nv.TaiKhoan;
            TxtTaiKhoan.IsReadOnly = true;
            TxtCCCD.Text          = nv.CCCD ?? "";
            TxtSDT.Text           = nv.SDT ?? "";
            TxtEmail.Text         = nv.Email ?? "";
            TxtDiaChi.Text        = nv.DiaChi ?? "";
            DpkNgaySinh.SelectedDate   = nv.NgaySinh;
            DpkNgayVaoLam.SelectedDate = nv.NgayVaoLam;
            PnlMatKhau.Visibility = Visibility.Collapsed;

            foreach (ComboBoxItem item in CboGioiTinh.Items)
                if (item.Tag?.ToString() == nv.GioiTinh) { item.IsSelected = true; break; }

            // Nếu QuanLy đang sửa LeTan: chỉ cho giữ LeTan
            // Nếu Admin: chọn đúng vai trò hiện tại
            foreach (ComboBoxItem item in CboVaiTro.Items)
                if (item.Tag?.ToString() == nv.VaiTro) { item.IsSelected = true; break; }

            // QuanLy không được thay đổi vai trò của người khác
            if (!SessionManager.IsAdmin)
                CboVaiTro.IsEnabled = false;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            PnlError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(TxtHoTen.Text))
            { ShowError("Vui lòng nhập họ tên."); return; }
            if (string.IsNullOrWhiteSpace(TxtTaiKhoan.Text))
            { ShowError("Vui lòng nhập tài khoản."); return; }
            if (string.IsNullOrWhiteSpace(TxtCCCD.Text))
            { ShowError("Vui lòng nhập CCCD/CMND. Đây là trường bắt buộc."); return; }

            string vaiTro    = (CboVaiTro.SelectedItem   as ComboBoxItem)?.Tag?.ToString() ?? "LeTan";
            string gioiTinh  = (CboGioiTinh.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Nam";

            // Kiểm tra phân quyền: QuanLy chỉ được thao tác với LeTan
            if (!SessionManager.IsAdmin && vaiTro != "LeTan")
            { ShowError("Quản Lý chỉ được phân vai trò Lễ Tân."); return; }

            try
            {
                using var ctx = new HotelDbContext();
                if (_maNV.HasValue)
                {
                    var nv = ctx.NhanViens.Find(_maNV.Value);
                    if (nv == null) return;

                    // QuanLy chỉ sửa được LeTan
                    if (!SessionManager.IsAdmin && nv.VaiTro != "LeTan")
                    { ShowError("Quản Lý chỉ được chỉnh sửa tài khoản Lễ Tân."); return; }

                    string oldVaiTro = nv.VaiTro;
                    nv.HoTen      = TxtHoTen.Text.Trim();
                    nv.CCCD       = TxtCCCD.Text.Trim();
                    nv.VaiTro     = vaiTro;
                    nv.GioiTinh   = gioiTinh;
                    nv.NgaySinh   = DpkNgaySinh.SelectedDate;
                    nv.NgayVaoLam = DpkNgayVaoLam.SelectedDate;
                    nv.Email      = TxtEmail.Text.Trim();
                    nv.SDT        = TxtSDT.Text.Trim();
                    nv.DiaChi     = TxtDiaChi.Text.Trim();

                    // Cập nhật MaLoaiNV theo vai trò mới
                    nv.MaLoaiNV = ctx.LoaiNhanViens
                        .Where(l => l.VaiTroCode == vaiTro)
                        .Select(l => (int?)l.MaLoaiNV)
                        .FirstOrDefault();

                    // Nếu đổi vai trò → reset quyền về mặc định của loại mới
                    if (oldVaiTro != vaiTro)
                    {
                        var maLoaiNV = nv.MaLoaiNV;
                        var newQuyens = maLoaiNV.HasValue
                            ? ctx.LoaiNhanVienQuyens
                                .Where(q => q.MaLoaiNV == maLoaiNV.Value)
                                .Select(q => q.MaQuyen).ToList()
                            : new List<string>();
                        if (!newQuyens.Any() && vaiTro == "QuanLy")
                            newQuyens = Helpers.Quyen.MacDinhQuanLy.ToList();

                        var oldQ = ctx.NhanVienQuyens.Where(q => q.MaNV == nv.MaNV).ToList();
                        ctx.NhanVienQuyens.RemoveRange(oldQ);
                        foreach (var q in newQuyens)
                            ctx.NhanVienQuyens.Add(new Models.NhanVienQuyen { MaNV = nv.MaNV, MaQuyen = q });
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(PbMatKhau.Password))
                    { ShowError("Vui lòng nhập mật khẩu."); return; }
                    if (PbMatKhau.Password != PbXacNhan.Password)
                    { ShowError("Mật khẩu xác nhận không khớp."); return; }
                    if (PbMatKhau.Password.Length < 6)
                    { ShowError("Mật khẩu phải ít nhất 6 ký tự."); return; }
                    if (ctx.NhanViens.Any(n => n.TaiKhoan == TxtTaiKhoan.Text.Trim()))
                    { ShowError("Tài khoản đã tồn tại."); return; }

                    var maLoaiNVMoi = ctx.LoaiNhanViens
                        .Where(l => l.VaiTroCode == vaiTro)
                        .Select(l => (int?)l.MaLoaiNV)
                        .FirstOrDefault();

                    var newNV = new Models.NhanVien
                    {
                        HoTen      = TxtHoTen.Text.Trim(),
                        TaiKhoan   = TxtTaiKhoan.Text.Trim(),
                        MatKhau    = BCrypt.Net.BCrypt.HashPassword(PbMatKhau.Password),
                        VaiTro     = vaiTro,
                        MaLoaiNV   = maLoaiNVMoi,
                        GioiTinh   = gioiTinh,
                        NgaySinh   = DpkNgaySinh.SelectedDate,
                        NgayVaoLam = DpkNgayVaoLam.SelectedDate,
                        CCCD       = TxtCCCD.Text.Trim(),
                        Email      = TxtEmail.Text.Trim(),
                        SDT        = TxtSDT.Text.Trim(),
                        DiaChi     = TxtDiaChi.Text.Trim(),
                        IsActive   = true
                    };
                    ctx.NhanViens.Add(newNV);
                    ctx.SaveChanges();

                    // Cấp quyền mặc định theo LoaiNhanVienQuyen (dùng FK trực tiếp, không dùng navigation)
                    var maLoaiNV = maLoaiNVMoi;
                    var loaiDefaults = maLoaiNV.HasValue
                        ? ctx.LoaiNhanVienQuyens
                            .Where(q => q.MaLoaiNV == maLoaiNV.Value)
                            .Select(q => q.MaQuyen)
                            .ToList()
                        : new List<string>();
                    // Fallback cứng nếu DB chưa có dữ liệu
                    if (!loaiDefaults.Any() && vaiTro == "QuanLy")
                        loaiDefaults = Helpers.Quyen.MacDinhQuanLy.ToList();
                    foreach (var q in loaiDefaults)
                        ctx.NhanVienQuyens.Add(new Models.NhanVienQuyen { MaNV = newNV.MaNV, MaQuyen = q });
                }
                ctx.SaveChanges();

                // Nếu vừa sửa chính mình → refresh SessionManager để Tài Khoản hiện đúng
                if (_maNV.HasValue && _maNV.Value == Helpers.SessionManager.CurrentUser?.MaNV)
                    Helpers.SessionManager.RefreshCurrentUser();

                DialogResult = true;
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
        private void ShowError(string msg) { TxtError.Text = msg; PnlError.Visibility = Visibility.Visible; }
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
    }
}
