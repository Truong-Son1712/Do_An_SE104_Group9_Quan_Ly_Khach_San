using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;
using Microsoft.EntityFrameworkCore;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels
{
    public class HoaDonViewModel : BaseViewModel
    {
        private ObservableCollection<HoaDon> _hoaDons = new();
        private HoaDon? _selected;
        private string _searchText = string.Empty;
        private string _filterTrangThai = "TatCa";
        private DateTime _tuNgay = DateTime.Today.AddDays(-30);
        private DateTime _denNgay = DateTime.Today;

        public ObservableCollection<HoaDon> HoaDons
        {
            get => _hoaDons;
            set => Set(ref _hoaDons, value);
        }

        public HoaDon? Selected
        {
            get => _selected;
            set => Set(ref _selected, value);
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

        public DateTime TuNgay
        {
            get => _tuNgay;
            set { Set(ref _tuNgay, value); LoadData(); }
        }

        public DateTime DenNgay
        {
            get => _denNgay;
            set { Set(ref _denNgay, value); LoadData(); }
        }

        private decimal _tongDoanhThu;
        public decimal TongDoanhThu { get => _tongDoanhThu; set => Set(ref _tongDoanhThu, value); }

        public ICommand RefreshCommand    { get; }
        public ICommand XemChiTietCommand { get; }
        public ICommand ThanhToanCommand  { get; }

        public HoaDonViewModel()
        {
            RefreshCommand    = new RelayCommand(_ => LoadData());
            XemChiTietCommand = new RelayCommand(_ => XemChiTiet(), _ => Selected != null);
            ThanhToanCommand  = new RelayCommand(_ => ThanhToan(), _ => Selected?.TrangThai == "ChuaThanhToan");
            LoadData();
        }

        public void LoadData()
        {
            try
            {
                using var ctx = new HotelDbContext();
                var q = ctx.HoaDons
                    .Include(h => h.DatPhong).ThenInclude(d => d!.KhachHang)
                    .Include(h => h.DatPhong).ThenInclude(d => d!.Phong)
                    .Include(h => h.NhanVien)
                    .Where(h => h.NgayLap.Date >= TuNgay.Date && h.NgayLap.Date <= DenNgay.Date)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                    q = q.Where(h =>
                        (h.DatPhong != null && h.DatPhong.KhachHang != null &&
                         h.DatPhong.KhachHang.HoTen.Contains(SearchText)) ||
                        (h.DatPhong != null && h.DatPhong.Phong != null &&
                         h.DatPhong.Phong.SoPhong.Contains(SearchText)) ||
                        h.MaHD.ToString().Contains(SearchText));

                if (FilterTrangThai != "TatCa")
                    q = q.Where(h => h.TrangThai == FilterTrangThai);

                var list = q.OrderByDescending(h => h.NgayLap).ToList();
                HoaDons = new ObservableCollection<HoaDon>(list);
                TongDoanhThu = list.Where(h => h.TrangThai == "DaThanhToan").Sum(h => h.TongTien);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void XemChiTiet()
        {
            if (Selected == null) return;
            var dlg = new Views.HoaDon.XemHoaDonDialog(Selected.MaHD);
            dlg.ShowDialog();
        }

        private void ThanhToan()
        {
            if (Selected == null) return;
            var dlg = new Views.HoaDon.ThanhToanDialog(Selected.MaHD);
            if (dlg.ShowDialog() == true)
                LoadData();
        }
    }
}
