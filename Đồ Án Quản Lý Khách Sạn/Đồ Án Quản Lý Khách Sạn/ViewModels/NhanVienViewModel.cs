using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels
{
    public class NhanVienViewModel : BaseViewModel
    {
        private ObservableCollection<NhanVien> _nhanViens = new();
        private NhanVien? _selected;
        private string _searchText = string.Empty;

        public ObservableCollection<NhanVien> NhanViens
        {
            get => _nhanViens;
            set => Set(ref _nhanViens, value);
        }

        public NhanVien? Selected
        {
            get => _selected;
            set => Set(ref _selected, value);
        }

        public string SearchText
        {
            get => _searchText;
            set { Set(ref _searchText, value); LoadData(); }
        }

        public bool IsAdmin  => SessionManager.IsAdmin;
        public bool IsQuanLy => SessionManager.IsQuanLy;

        private int CurrentMaNV => SessionManager.CurrentUser?.MaNV ?? -1;

        // Sửa/đổi MK: cho phép sửa bản thân + sửa người khác theo quyền
        private bool CanEdit =>
            Selected != null &&
            (Selected.MaNV == CurrentMaNV ||               // luôn được sửa bản thân
             IsAdmin ||                                     // admin sửa tất cả
             Selected.VaiTro == "LeTan");                  // quanly chỉ sửa letan

        // Xóa/đổi trạng thái: không được thao tác với bản thân
        private bool CanActOnOther =>
            Selected != null &&
            Selected.MaNV != CurrentMaNV &&
            (IsAdmin || Selected.VaiTro == "LeTan");

        public ICommand RefreshCommand      { get; }
        public ICommand ThemCommand         { get; }
        public ICommand SuaCommand          { get; }
        public ICommand XoaCommand          { get; }
        public ICommand DoiTrangThaiCommand { get; }
        public ICommand DoiMatKhauCommand   { get; }
        public ICommand PhanQuyenCommand    { get; }

        public NhanVienViewModel()
        {
            RefreshCommand      = new RelayCommand(_ => LoadData());
            ThemCommand         = new RelayCommand(_ => Them());
            SuaCommand          = new RelayCommand(_ => Sua(),         _ => CanEdit);
            XoaCommand          = new RelayCommand(_ => Xoa(),         _ => CanActOnOther);
            DoiTrangThaiCommand = new RelayCommand(_ => DoiTrangThai(), _ => CanActOnOther);
            DoiMatKhauCommand   = new RelayCommand(_ => DoiMatKhau(),   _ => CanEdit);
            PhanQuyenCommand    = new RelayCommand(_ => PhanQuyen(),
                _ => IsAdmin && Selected != null && Selected.VaiTro != "Admin");
            LoadData();
        }

        public void LoadData()
        {
            try
            {
                using var ctx = new HotelDbContext();
                var q = ctx.NhanViens.AsQueryable();

                // QuanLy chỉ thấy LeTan (và chính mình)
                if (!IsAdmin)
                    q = q.Where(n => n.VaiTro == "LeTan" || n.MaNV == SessionManager.CurrentUser!.MaNV);

                if (!string.IsNullOrWhiteSpace(SearchText))
                    q = q.Where(n => n.HoTen.Contains(SearchText) || n.TaiKhoan.Contains(SearchText));

                NhanViens = new ObservableCollection<NhanVien>(
                    q.OrderBy(n => n.VaiTro).ThenBy(n => n.HoTen).ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Them()
        {
            var dlg = new Views.NhanVien.NhanVienDialog();
            if (dlg.ShowDialog() == true) LoadData();
        }

        private void Sua()
        {
            if (Selected == null) return;
            var dlg = new Views.NhanVien.NhanVienDialog(Selected.MaNV);
            if (dlg.ShowDialog() == true) LoadData();
        }

        private void Xoa()
        {
            if (Selected == null) return;

            try
            {
                using var ctx = new HotelDbContext();

                // Kiểm tra phân quyền lần cuối
                var nv = ctx.NhanViens.Find(Selected.MaNV);
                if (nv == null) return;
                if (!IsAdmin && nv.VaiTro != "LeTan")
                { MessageBox.Show("Quản Lý chỉ được xóa tài khoản Lễ Tân.", "Không có quyền", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

                // Kiểm tra lịch sử hóa đơn
                bool coLichSu = ctx.HoaDons.Any(h => h.MaNV == Selected.MaNV);
                if (coLichSu)
                {
                    MessageBox.Show(
                        $"Không thể xóa nhân viên \"{Selected.HoTen}\" vì đã có lịch sử lập hóa đơn trong hệ thống.",
                        "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show(
                        $"Xóa vĩnh viễn tài khoản nhân viên \"{Selected.HoTen}\"?\nThao tác này không thể hoàn tác.",
                        "Xác Nhận Xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                        MessageBoxResult.No) != MessageBoxResult.Yes) return;

                ctx.NhanViens.Remove(nv);
                ctx.SaveChanges();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DoiTrangThai()
        {
            if (Selected == null) return;
            string action = Selected.IsActive ? "vô hiệu hóa" : "kích hoạt";
            if (MessageBox.Show($"Xác nhận {action} tài khoản \"{Selected.HoTen}\"?",
                "Xác Nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

            try
            {
                using var ctx = new HotelDbContext();
                var nv = ctx.NhanViens.Find(Selected.MaNV);
                if (nv != null) { nv.IsActive = !nv.IsActive; ctx.SaveChanges(); }
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DoiMatKhau()
        {
            if (Selected == null) return;
            var dlg = new Views.NhanVien.DoiMatKhauDialog(Selected.MaNV);
            dlg.ShowDialog();
        }

        private void PhanQuyen()
        {
            if (Selected == null || Selected.VaiTro == "Admin") return;
            var dlg = new Views.NhanVien.PhanQuyenDialog(Selected.MaNV, Selected.HoTen, Selected.VaiTro);
            dlg.ShowDialog();
        }
    }
}
