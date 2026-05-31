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

        public ICommand RefreshCommand        { get; }
        public ICommand ThemCommand           { get; }
        public ICommand SuaCommand            { get; }
        public ICommand DoiTrangThaiCommand   { get; }
        public ICommand DoiMatKhauCommand     { get; }

        public NhanVienViewModel()
        {
            RefreshCommand      = new RelayCommand(_ => LoadData());
            ThemCommand         = new RelayCommand(_ => Them());
            SuaCommand          = new RelayCommand(_ => Sua(),         _ => Selected != null);
            DoiTrangThaiCommand = new RelayCommand(_ => DoiTrangThai(), _ => Selected != null && Selected.MaNV != SessionManager.CurrentUser?.MaNV);
            DoiMatKhauCommand   = new RelayCommand(_ => DoiMatKhau(),   _ => Selected != null);
            LoadData();
        }

        public void LoadData()
        {
            try
            {
                using var ctx = new HotelDbContext();
                var q = ctx.NhanViens.AsQueryable();
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
    }
}
