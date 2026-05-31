using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;
using Microsoft.EntityFrameworkCore;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels
{
    public class PhongViewModel : BaseViewModel
    {
        private ObservableCollection<Phong> _phongs = new();
        private Phong? _selectedPhong;
        private string _searchText = string.Empty;
        private string _filterTrangThai = "TatCa";

        public ObservableCollection<Phong> Phongs
        {
            get => _phongs;
            set => Set(ref _phongs, value);
        }

        public Phong? SelectedPhong
        {
            get => _selectedPhong;
            set => Set(ref _selectedPhong, value);
        }

        public string SearchText
        {
            get => _searchText;
            set { Set(ref _searchText, value); LoadData(); }
        }

        public string FilterTrangThai
        {
            get => _filterTrangThai;
            set { Set(ref _filterTrangThai, value); LoadData(); }
        }

        public bool CanEdit => SessionManager.IsQuanLy;

        public ICommand RefreshCommand   { get; }
        public ICommand ThemCommand      { get; }
        public ICommand SuaCommand       { get; }
        public ICommand XoaCommand       { get; }
        public ICommand DoiTrangThaiCommand { get; }

        public PhongViewModel()
        {
            RefreshCommand      = new RelayCommand(_ => LoadData());
            ThemCommand         = new RelayCommand(_ => ThemPhong(),   _ => CanEdit);
            SuaCommand          = new RelayCommand(_ => SuaPhong(),    _ => SelectedPhong != null && CanEdit);
            XoaCommand          = new RelayCommand(_ => XoaPhong(),    _ => SelectedPhong != null && CanEdit);
            DoiTrangThaiCommand = new RelayCommand(DoiTrangThai,       _ => SelectedPhong != null);
            LoadData();
        }

        public void LoadData()
        {
            try
            {
                using var ctx = new HotelDbContext();
                var query = ctx.Phongs.Include(p => p.LoaiPhong).AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                    query = query.Where(p => p.SoPhong.Contains(SearchText) ||
                                            (p.LoaiPhong != null && p.LoaiPhong.TenLoaiPhong.Contains(SearchText)));

                if (FilterTrangThai != "TatCa" && Enum.TryParse<TrangThaiPhong>(FilterTrangThai, out var tt))
                    query = query.Where(p => p.TrangThai == tt);

                Phongs = new ObservableCollection<Phong>(
                    query.OrderBy(p => p.Tang).ThenBy(p => p.SoPhong).ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu phòng: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThemPhong()
        {
            var dlg = new Views.Phong.PhongDialog();
            if (dlg.ShowDialog() == true) LoadData();
        }

        private bool IsPhongBusy() =>
            SelectedPhong?.TrangThai == TrangThaiPhong.DaDat ||
            SelectedPhong?.TrangThai == TrangThaiPhong.DangSuDung;

        private void SuaPhong()
        {
            if (SelectedPhong == null) return;
            if (IsPhongBusy())
            {
                MessageBox.Show("Không thể chỉnh sửa phòng đang có khách đặt hoặc đang sử dụng.",
                    "Không Thể Thực Hiện", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var dlg = new Views.Phong.PhongDialog(SelectedPhong.MaPhong);
            if (dlg.ShowDialog() == true) LoadData();
        }

        private void XoaPhong()
        {
            if (SelectedPhong == null) return;
            if (IsPhongBusy())
            {
                MessageBox.Show("Không thể xóa phòng đang có khách đặt hoặc đang sử dụng.",
                    "Không Thể Thực Hiện", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (MessageBox.Show($"Xác nhận xóa phòng {SelectedPhong.SoPhong}?",
                "Xác Nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

            try
            {
                using var ctx = new HotelDbContext();
                var phong = ctx.Phongs.Find(SelectedPhong.MaPhong);
                if (phong != null) { ctx.Phongs.Remove(phong); ctx.SaveChanges(); }
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể xóa phòng: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DoiTrangThai(object? param)
        {
            if (SelectedPhong == null) return;

            if (SelectedPhong.TrangThai == TrangThaiPhong.DaDat ||
                SelectedPhong.TrangThai == TrangThaiPhong.DangSuDung)
            {
                MessageBox.Show(
                    "Không thể thay đổi trạng thái phòng đang có khách đặt hoặc đang sử dụng.\n" +
                    "Trạng thái sẽ tự động cập nhật khi hoàn tất quy trình đặt/trả phòng.",
                    "Không Thể Thực Hiện", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dlg = new Views.Phong.DoiTrangThaiPhongDialog(SelectedPhong.MaPhong);
            if (dlg.ShowDialog() == true) LoadData();
        }
    }
}
