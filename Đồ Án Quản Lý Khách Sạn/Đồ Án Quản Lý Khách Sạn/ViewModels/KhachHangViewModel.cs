using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;
using Microsoft.EntityFrameworkCore;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels
{
    public class KhachHangViewModel : BaseViewModel
    {
        private ObservableCollection<KhachHang> _khachHangs = new();
        private KhachHang? _selected;
        private string _searchText   = string.Empty;
        private int    _filterLoai   = 0;   // 0 = tất cả
        private int _filterGioiTinhIndex; // 0=Tất cả, 1=Nam, 2=Nữ

        public ObservableCollection<KhachHang> KhachHangs
        {
            get => _khachHangs;
            set => Set(ref _khachHangs, value);
        }

        public KhachHang? Selected
        {
            get => _selected;
            set => Set(ref _selected, value);
        }

        public string SearchText
        {
            get => _searchText;
            set { Set(ref _searchText, value); LoadData(); }
        }

        public int FilterLoai
        {
            get => _filterLoai;
            set { Set(ref _filterLoai, value); LoadData(); }
        }

        public int FilterGioiTinhIndex
        {
            get => _filterGioiTinhIndex;
            set { Set(ref _filterGioiTinhIndex, value); LoadData(); }
        }

        public ObservableCollection<KeyValuePair<int, string>> LoaiKhachOptions { get; private set; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand ThemCommand    { get; }
        public ICommand SuaCommand     { get; }
        public ICommand XoaCommand     { get; }

        public KhachHangViewModel()
        {
            RefreshCommand = new RelayCommand(_ => Refresh());
            ThemCommand    = new RelayCommand(_ => Them());
            SuaCommand     = new RelayCommand(_ => Sua(), _ => Selected != null);
            XoaCommand     = new RelayCommand(_ => Xoa(), _ => Selected != null);
            Refresh();
        }

        private void Refresh() { LoadFilterOptions(); LoadData(); }

        private void LoadFilterOptions()
        {
            using var ctx = new HotelDbContext();
            var all = new KeyValuePair<int, string>(0, "Tất cả");

            LoaiKhachOptions = new ObservableCollection<KeyValuePair<int, string>>(
                new[] { all }.Concat(
                    ctx.LoaiKhachHangs.OrderBy(l => l.TenLoai)
                        .Select(l => new KeyValuePair<int, string>(l.MaLKH, l.TenLoai))
                        .ToList()));

            OnPropertyChanged(nameof(LoaiKhachOptions));
        }

        public void LoadData()
        {
            try
            {
                using var ctx = new HotelDbContext();
                var q = ctx.KhachHangs
                    .Include(k => k.LoaiKhachHang)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                    q = q.Where(k => k.HoTen.Contains(SearchText) ||
                                     k.CMND.Contains(SearchText) ||
                                     (k.SDT != null && k.SDT.Contains(SearchText)));

                if (FilterLoai > 0)
                    q = q.Where(k => k.MaLoaiKH == FilterLoai);

                if (FilterGioiTinhIndex == 1) q = q.Where(k => k.GioiTinh == "Nam");
                else if (FilterGioiTinhIndex == 2) q = q.Where(k => k.GioiTinh == "Nu");

                KhachHangs = new ObservableCollection<KhachHang>(
                    q.OrderByDescending(k => k.NgayTao).ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Them()
        {
            var dlg = new Views.KhachHang.KhachHangDialog();
            if (dlg.ShowDialog() == true) Refresh();
        }

        private void Sua()
        {
            if (Selected == null) return;
            var dlg = new Views.KhachHang.KhachHangDialog(Selected.MaKH);
            if (dlg.ShowDialog() == true) Refresh();
        }

        private void Xoa()
        {
            if (Selected == null) return;
            if (MessageBox.Show($"Xóa khách hàng \"{Selected.HoTen}\"?",
                "Xác Nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            try
            {
                using var ctx = new HotelDbContext();
                if (ctx.DatPhongs.Any(d => d.MaKH == Selected.MaKH))
                {
                    MessageBox.Show("Không thể xóa vì khách hàng đã có lịch sử đặt phòng.", "Cảnh Báo",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var kh = ctx.KhachHangs.Find(Selected.MaKH);
                if (kh != null) { ctx.KhachHangs.Remove(kh); ctx.SaveChanges(); }
                Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
