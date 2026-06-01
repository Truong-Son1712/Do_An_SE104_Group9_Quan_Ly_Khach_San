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
        public string  Thang     { get; set; } = string.Empty;
        public decimal DoanhThu  { get; set; }
        public int     SoHoaDon  { get; set; }
        public decimal TyLe      { get; set; }
    }

    public class BaoCaoLoaiPhongItem
    {
        public string  TenLoaiPhong { get; set; } = string.Empty;
        public int     SoLuotDat    { get; set; }
        public decimal DoanhThu     { get; set; }
        public double  TyLe         { get; set; }
        public string  TyLeText     => $"{TyLe:0.#}%";
    }

    public class BaoCaoViewModel : BaseViewModel
    {
        private int     _selectedYear = DateTime.Today.Year;
        private decimal _tongDoanhThuNam;
        private int     _tongLuotKhach;
        private int     _tongHoaDon;
        private double  _congSuatPhong;
        private string  _loaiKhachNoiDia    = "0";
        private string  _loaiKhachNuocNgoai = "0";

        public int SelectedYear
        {
            get => _selectedYear;
            set { Set(ref _selectedYear, value); LoadData(); }
        }

        public decimal TongDoanhThuNam    { get => _tongDoanhThuNam;     set => Set(ref _tongDoanhThuNam, value); }
        public int     TongLuotKhach       { get => _tongLuotKhach;       set => Set(ref _tongLuotKhach, value); }
        public int     TongHoaDon          { get => _tongHoaDon;          set => Set(ref _tongHoaDon, value); }
        public double  CongSuatPhong       { get => _congSuatPhong;       set => Set(ref _congSuatPhong, value); }
        public string  LoaiKhachNoiDia     { get => _loaiKhachNoiDia;     set => Set(ref _loaiKhachNoiDia, value); }
        public string  LoaiKhachNuocNgoai  { get => _loaiKhachNuocNgoai;  set => Set(ref _loaiKhachNuocNgoai, value); }

        public ObservableCollection<BaoCaoDoanhThuItem>  DoanhThuTheoThang { get; } = new();
        public ObservableCollection<BaoCaoLoaiPhongItem> DoanhThuLoaiPhong { get; } = new();

        public List<int> DanhSachNam { get; } =
            Enumerable.Range(2020, DateTime.Today.Year - 2019).Reverse().ToList();

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

                // ── Hóa đơn đã thanh toán trong năm ────────────────────────
                var hdsNam = ctx.HoaDons
                    .Include(h => h.DatPhong).ThenInclude(d => d!.KhachHang)
                    .Include(h => h.DatPhong).ThenInclude(d => d!.Phong).ThenInclude(p => p!.LoaiPhong)
                    .Where(h => h.NgayLap.Year == SelectedYear && h.TrangThai == "DaThanhToan")
                    .ToList();

                // ── Tiền cọc nhận trong năm, chưa có HoaDon DaThanhToan ────
                // Cọc được tính vào doanh thu ngay khi xác nhận đặt phòng
                var paidIds = ctx.HoaDons
                    .Where(h => h.TrangThai == "DaThanhToan")
                    .Select(h => h.MaDatPhong)
                    .ToHashSet();

                var cocBookings = ctx.DatPhongs
                    .Include(d => d.Phong).ThenInclude(p => p!.LoaiPhong)
                    .Where(d => d.NgayDat.Year == SelectedYear
                             && d.TrangThai != TrangThaiDatPhong.HuyDat)
                    .ToList()
                    .Where(d => d.TienCoc > 0 && !paidIds.Contains(d.MaDatPhong))
                    .ToList();

                decimal tongCoc = cocBookings.Sum(d => d.TienCoc);

                // ── Tổng doanh thu & thống kê chung ────────────────────────
                TongDoanhThuNam = hdsNam.Sum(h => h.TongTien) + tongCoc;
                TongHoaDon      = hdsNam.Count;
                TongLuotKhach   = ctx.DatPhongs.Count(d =>
                    d.NgayNhanPhong.Year == SelectedYear &&
                    d.TrangThai != TrangThaiDatPhong.HuyDat);

                int tongPhong      = ctx.Phongs.Count();
                int ngayTrongNam   = DateTime.IsLeapYear(SelectedYear) ? 366 : 365;
                int tongLuotDat    = ctx.DatPhongs
                    .Where(d => d.NgayNhanPhong.Year == SelectedYear && d.TrangThai != TrangThaiDatPhong.HuyDat)
                    .ToList()
                    .Sum(d => Math.Max(1, (d.NgayTraPhong - d.NgayNhanPhong).Days));
                CongSuatPhong = tongPhong > 0
                    ? Math.Round((double)tongLuotDat / (tongPhong * ngayTrongNam) * 100, 1)
                    : 0;

                var allKhach = ctx.DatPhongs
                    .Include(d => d.KhachHang).ThenInclude(k => k!.LoaiKhachHang)
                    .Where(d => d.NgayNhanPhong.Year == SelectedYear && d.TrangThai != TrangThaiDatPhong.HuyDat)
                    .Select(d => d.KhachHang)
                    .ToList();
                // Phân loại: HeSoGia == 1.0 = nội địa, > 1.0 = khách có phụ thu
                int nd  = allKhach.Count(k => k?.LoaiKhachHang?.HeSoGia == 1m);
                int nn  = allKhach.Count(k => k?.LoaiKhachHang?.HeSoGia > 1m);
                int all = nd + nn;
                LoaiKhachNoiDia    = all > 0 ? $"{nd} ({nd * 100 / all}%)" : "0";
                LoaiKhachNuocNgoai = all > 0 ? $"{nn} ({nn * 100 / all}%)" : "0";

                // ── Doanh thu theo tháng (HoaDon + tiền cọc tháng đó) ──────
                DoanhThuTheoThang.Clear();

                // Tính maxDT bao gồm cả cọc để scale biểu đồ đúng
                decimal maxDt = 1;
                for (int t = 1; t <= 12; t++)
                {
                    decimal dt = hdsNam.Where(h => h.NgayLap.Month == t).Sum(h => h.TongTien)
                               + cocBookings.Where(d => d.NgayDat.Month == t).Sum(d => d.TienCoc);
                    if (dt > maxDt) maxDt = dt;
                }

                for (int thang = 1; thang <= 12; thang++)
                {
                    var dsHd = hdsNam.Where(h => h.NgayLap.Month == thang).ToList();
                    decimal dtHd  = dsHd.Sum(h => h.TongTien);
                    decimal dtCoc = cocBookings.Where(d => d.NgayDat.Month == thang).Sum(d => d.TienCoc);
                    decimal dt    = dtHd + dtCoc;
                    DoanhThuTheoThang.Add(new BaoCaoDoanhThuItem
                    {
                        Thang    = $"T{thang}",
                        DoanhThu = dt,
                        SoHoaDon = dsHd.Count,
                        TyLe     = maxDt > 0 ? Math.Round(dt / maxDt * 100, 1) : 0
                    });
                }

                // ── Doanh thu theo loại phòng (HoaDon + tiền cọc) ──────────
                DoanhThuLoaiPhong.Clear();

                // Gộp HoaDon theo loại phòng
                var byLoaiHd = hdsNam
                    .Where(h => h.DatPhong?.Phong?.LoaiPhong != null)
                    .GroupBy(h => h.DatPhong!.Phong!.LoaiPhong!.TenLoaiPhong)
                    .ToDictionary(g => g.Key, g => (soLuot: g.Count(), doanhThu: g.Sum(h => h.TongTien)));

                // Gộp tiền cọc theo loại phòng
                var byLoaiCoc = cocBookings
                    .Where(d => d.Phong?.LoaiPhong != null)
                    .GroupBy(d => d.Phong!.LoaiPhong!.TenLoaiPhong)
                    .ToDictionary(g => g.Key, g => g.Sum(d => d.TienCoc));

                var allLoai = byLoaiHd.Keys.Union(byLoaiCoc.Keys).ToList();
                var byLoaiRaw = allLoai.Select(loai => new BaoCaoLoaiPhongItem
                {
                    TenLoaiPhong = loai,
                    SoLuotDat   = byLoaiHd.TryGetValue(loai, out var hd) ? hd.soLuot : 0,
                    DoanhThu    = (byLoaiHd.TryGetValue(loai, out var hd2) ? hd2.doanhThu : 0)
                                + (byLoaiCoc.TryGetValue(loai, out var coc) ? coc : 0)
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
