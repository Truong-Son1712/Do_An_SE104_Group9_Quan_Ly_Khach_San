using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels
{
    public class KhachHangViewModel : BaseViewModel
    {
        private ObservableCollection<KhachHang> _khachHangs = new();
        private KhachHang? _selected;
        private string _searchText = string.Empty;
        private string _filterLoai = "TatCa";

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

        public string FilterLoai
        {
            get => _filterLoai;
            set { Set(ref _filterLoai, value); LoadData(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ThemCommand    { get; }
        public ICommand SuaCommand     { get; }
        public ICommand XoaCommand     { get; }

        public KhachHangViewModel()
        {
            RefreshCommand = new RelayCommand(_ => LoadData());
            ThemCommand    = new RelayCommand(_ => Them());
            SuaCommand     = new RelayCommand(_ => Sua(), _ => Selected != null);
            XoaCommand     = new RelayCommand(_ => Xoa(), _ => Selected != null);
            LoadData();
        }

        public void LoadData()
        {
            try
            {
                using var ctx = new HotelDbContext();
                var q = ctx.KhachHangs.AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                    q = q.Where(k => k.HoTen.Contains(SearchText) ||
                                     k.CMND.Contains(SearchText) ||
                                     (k.SDT != null && k.SDT.Contains(SearchText)));

                if (FilterLoai != "TatCa")
                    q = q.Where(k => k.LoaiKhach == FilterLoai);

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
            if (dlg.ShowDialog() == true) LoadData();
        }

        private void Sua()
        {
            if (Selected == null) return;
            var dlg = new Views.KhachHang.KhachHangDialog(Selected.MaKH);
            if (dlg.ShowDialog() == true) LoadData();
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
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
