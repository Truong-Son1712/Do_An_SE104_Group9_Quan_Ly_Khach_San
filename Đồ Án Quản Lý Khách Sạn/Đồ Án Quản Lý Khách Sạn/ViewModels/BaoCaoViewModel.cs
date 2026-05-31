using System.Collections.ObjectModel;
using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;
using Microsoft.EntityFrameworkCore;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels
{
    public class BaoCaoDoanhThuItem
    {
        public string Thang    { get; set; } = string.Empty;
        public decimal DoanhThu { get; set; }
        public int SoHoaDon    { get; set; }
        public decimal TyLe    { get; set; }
    }

    public class BaoCaoLoaiPhongItem
    {
        public string TenLoaiPhong { get; set; } = string.Empty;
        public int SoLuotDat       { get; set; }
        public decimal DoanhThu    { get; set; }
        public double TyLe         { get; set; }
        public string TyLeText     => $"{TyLe:0.#}%";
    }

    public class BaoCaoViewModel : BaseViewModel
    {
        private int _selectedYear = DateTime.Today.Year;
        private decimal _tongDoanhThuNam;
        private int _tongLuotKhach;
        private int _tongHoaDon;
        private double _congSuatPhong;
        private string _loaiKhachNoiDia = "0";
        private string _loaiKhachNuocNgoai = "0";

        public int SelectedYear
        {
            get => _selectedYear;
            set { Set(ref _selectedYear, value); LoadData(); }
        }

        public decimal TongDoanhThuNam    { get => _tongDoanhThuNam;     set => Set(ref _tongDoanhThuNam, value); }
        public int TongLuotKhach           { get => _tongLuotKhach;       set => Set(ref _tongLuotKhach, value); }
        public int TongHoaDon              { get => _tongHoaDon;          set => Set(ref _tongHoaDon, value); }
        public double CongSuatPhong        { get => _congSuatPhong;       set => Set(ref _congSuatPhong, value); }
        public string LoaiKhachNoiDia      { get => _loaiKhachNoiDia;     set => Set(ref _loaiKhachNoiDia, value); }
        public string LoaiKhachNuocNgoai   { get => _loaiKhachNuocNgoai;  set => Set(ref _loaiKhachNuocNgoai, value); }

        public ObservableCollection<BaoCaoDoanhThuItem> DoanhThuTheoThang { get; } = new();
        public ObservableCollection<BaoCaoLoaiPhongItem> DoanhThuLoaiPhong { get; } = new();

        public List<int> DanhSachNam { get; } = Enumerable.Range(2020, DateTime.Today.Year - 2019).Reverse().ToList();

        public ICommand RefreshCommand { get; }

        public BaoCaoViewModel()
        {
            RefreshCommand = new RelayCommand(_ => LoadData());
            LoadData();
        }

        public void LoadData()
        {
            try
            {
                using var ctx = new HotelDbContext();

                var hdsNam = ctx.HoaDons
                    .Include(h => h.DatPhong).ThenInclude(d => d!.KhachHang)
                    .Include(h => h.DatPhong).ThenInclude(d => d!.Phong).ThenInclude(p => p!.LoaiPhong)
                    .Where(h => h.NgayLap.Year == SelectedYear && h.TrangThai == "DaThanhToan")
                    .ToList();

                TongDoanhThuNam = hdsNam.Sum(h => h.TongTien);
                TongHoaDon = hdsNam.Count;
                TongLuotKhach = ctx.DatPhongs.Count(d =>
                    d.NgayNhanPhong.Year == SelectedYear &&
                    d.TrangThai != TrangThaiDatPhong.HuyDat);

                int tongPhong = ctx.Phongs.Count();
                int ngayTrongNam = DateTime.IsLeapYear(SelectedYear) ? 366 : 365;
                int tongLuotDat = ctx.DatPhongs
                    .Where(d => d.NgayNhanPhong.Year == SelectedYear && d.TrangThai != TrangThaiDatPhong.HuyDat)
                    .ToList()
                    .Sum(d => Math.Max(1, (d.NgayTraPhong - d.NgayNhanPhong).Days));
                CongSuatPhong = tongPhong > 0
                    ? Math.Round((double)tongLuotDat / (tongPhong * ngayTrongNam) * 100, 1)
                    : 0;

                var allKhach = ctx.DatPhongs
                    .Include(d => d.KhachHang)
                    .Where(d => d.NgayNhanPhong.Year == SelectedYear && d.TrangThai != TrangThaiDatPhong.HuyDat)
                    .Select(d => d.KhachHang)
                    .ToList();
                int nd  = allKhach.Count(k => k?.LoaiKhach == "NoiDia");
                int nn  = allKhach.Count(k => k?.LoaiKhach == "NuocNgoai");
                int all = nd + nn;
                LoaiKhachNoiDia    = all > 0 ? $"{nd} ({nd * 100 / all}%)" : "0";
                LoaiKhachNuocNgoai = all > 0 ? $"{nn} ({nn * 100 / all}%)" : "0";

                // Doanh thu theo tháng
                DoanhThuTheoThang.Clear();
                decimal maxDt = hdsNam.Any() ? hdsNam.GroupBy(h => h.NgayLap.Month).Max(g => g.Sum(h => h.TongTien)) : 1;
                for (int thang = 1; thang <= 12; thang++)
                {
                    var ds = hdsNam.Where(h => h.NgayLap.Month == thang).ToList();
                    decimal dt = ds.Sum(h => h.TongTien);
                    DoanhThuTheoThang.Add(new BaoCaoDoanhThuItem
                    {
                        Thang = $"T{thang}",
                        DoanhThu = dt,
                        SoHoaDon = ds.Count,
                        TyLe = maxDt > 0 ? Math.Round(dt / maxDt * 100, 1) : 0
                    });
                }

                // Doanh thu theo loại phòng
                DoanhThuLoaiPhong.Clear();
                var byLoaiRaw = hdsNam
                    .Where(h => h.DatPhong?.Phong?.LoaiPhong != null)
                    .GroupBy(h => h.DatPhong!.Phong!.LoaiPhong!.TenLoaiPhong)
                    .Select(g => new BaoCaoLoaiPhongItem
                    {
                        TenLoaiPhong = g.Key,
                        SoLuotDat   = g.Count(),
                        DoanhThu    = g.Sum(h => h.TongTien)
                    })
                    .OrderByDescending(x => x.DoanhThu)
                    .ToList();
                decimal tongLoai = byLoaiRaw.Sum(x => x.DoanhThu);
                foreach (var item in byLoaiRaw)
                {
                    item.TyLe = tongLoai > 0 ? Math.Round((double)(item.DoanhThu / tongLoai * 100), 1) : 0;
                    DoanhThuLoaiPhong.Add(item);
                }
            }
            catch { /* silent */ }
        }
    }
}
