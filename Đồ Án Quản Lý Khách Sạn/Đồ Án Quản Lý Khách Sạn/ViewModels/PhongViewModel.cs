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
        private int _filterMaLoaiPhong;
        private int _filterSucChua;
        private int _filterTang;

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

        public int FilterMaLoaiPhong
        {
            get => _filterMaLoaiPhong;
            set { Set(ref _filterMaLoaiPhong, value); LoadData(); }
        }

        public int FilterSucChua
        {
            get => _filterSucChua;
            set { Set(ref _filterSucChua, value); LoadData(); }
        }

        public int FilterTang
        {
            get => _filterTang;
            set { Set(ref _filterTang, value); LoadData(); }
        }

        public ObservableCollection<KeyValuePair<int, string>> LoaiPhongOptions { get; private set; } = new();
        public ObservableCollection<KeyValuePair<int, string>> SucChuaOptions   { get; private set; } = new();
        public ObservableCollection<KeyValuePair<int, string>> TangOptions      { get; private set; } = new();

        public bool CanEdit => SessionManager.IsQuanLy;

        public ICommand RefreshCommand   { get; }
        public ICommand ThemCommand      { get; }
        public ICommand SuaCommand       { get; }
        public ICommand XoaCommand       { get; }
        public ICommand DoiTrangThaiCommand { get; }

        public PhongViewModel()
        {
            RefreshCommand      = new RelayCommand(_ => Refresh());
            ThemCommand         = new RelayCommand(_ => ThemPhong(),   _ => CanEdit);
            SuaCommand          = new RelayCommand(_ => SuaPhong(),    _ => SelectedPhong != null && CanEdit);
            XoaCommand          = new RelayCommand(_ => XoaPhong(),    _ => SelectedPhong != null && CanEdit);
            DoiTrangThaiCommand = new RelayCommand(DoiTrangThai,       _ => SelectedPhong != null);
            Refresh();
        }

        private void Refresh() { LoadFilterOptions(); LoadData(); }

        private void LoadFilterOptions()
        {
            using var ctx = new HotelDbContext();

            var all = new KeyValuePair<int, string>(0, "Tất cả");

            LoaiPhongOptions = new ObservableCollection<KeyValuePair<int, string>>(
                new[] { all }.Concat(
                    ctx.LoaiPhongs.OrderBy(l => l.TenLoaiPhong)
                        .Select(l => new KeyValuePair<int, string>(l.MaLoaiPhong, l.TenLoaiPhong))
                        .ToList()));

            SucChuaOptions = new ObservableCollection<KeyValuePair<int, string>>(
                new[] { all }.Concat(
                    ctx.LoaiPhongs.Select(l => l.SucChua).Distinct().OrderBy(s => s).ToList()
                        .Select(s => new KeyValuePair<int, string>(s, $"{s} người"))));

            TangOptions = new ObservableCollection<KeyValuePair<int, string>>(
                new[] { all }.Concat(
                    ctx.Phongs.Select(p => p.Tang).Distinct().OrderBy(t => t).ToList()
                        .Select(t => new KeyValuePair<int, string>(t, $"Tầng {t}"))));

            OnPropertyChanged(nameof(LoaiPhongOptions));
            OnPropertyChanged(nameof(SucChuaOptions));
            OnPropertyChanged(nameof(TangOptions));
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

                if (FilterMaLoaiPhong > 0)
                    query = query.Where(p => p.MaLoaiPhong == FilterMaLoaiPhong);

                if (FilterSucChua > 0)
                    query = query.Where(p => p.LoaiPhong != null && p.LoaiPhong.SucChua == FilterSucChua);

                if (FilterTang > 0)
                    query = query.Where(p => p.Tang == FilterTang);

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
            if (dlg.ShowDialog() == true) Refresh();
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
            if (dlg.ShowDialog() == true) Refresh();
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
                Refresh();
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
