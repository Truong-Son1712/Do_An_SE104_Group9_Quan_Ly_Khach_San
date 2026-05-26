using System.Windows;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;
using Microsoft.EntityFrameworkCore;
using KhachHangModel = Đồ_Án_Quản_Lý_Khách_Sạn.Models.KhachHang;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.DatPhong
{
    public partial class ChiTietDatPhongDialog : Window
    {
        private class KhachDisplay
        {
            public string TieuDe   { get; set; } = "";
            public string MaKH     { get; set; } = "";
            public string CMND     { get; set; } = "";
            public string GioiTinh { get; set; } = "";
            public string NgaySinh { get; set; } = "";
            public string SDT      { get; set; } = "";
            public string QuocTich { get; set; } = "";
            public string LoaiKhach{ get; set; } = "";
            public string DiaChi   { get; set; } = "";
        }

        public ChiTietDatPhongDialog(int maDatPhong)
        {
            InitializeComponent();
            LoadData(maDatPhong);
        }

        private void LoadData(int id)
        {
            using var ctx = new HotelDbContext();
            var dp = ctx.DatPhongs
                .Include(d => d.Phong).ThenInclude(p => p!.LoaiPhong)
                .Include(d => d.KhachHang)
                .Include(d => d.DatPhongKhachHangs).ThenInclude(x => x.KhachHang)
                .FirstOrDefault(d => d.MaDatPhong == id);
            if (dp == null) return;

            TxtTitle.Text = $"Chi Tiết Đặt Phòng #{dp.MaDatPhong}";
            TxtMaDP.Text  = $"#{dp.MaDatPhong}";
            TxtPhong.Text = $"Phòng {dp.Phong?.SoPhong} – {dp.Phong?.LoaiPhong?.TenLoaiPhong}";
            TxtNgayDat.Text = dp.NgayDat.ToString("dd/MM/yyyy HH:mm:ss");

            bool daCheckIn  = dp.TrangThai == TrangThaiDatPhong.DaNhanPhong
                           || dp.TrangThai == TrangThaiDatPhong.DaTraPhong;
            bool daCheckOut = dp.TrangThai == TrangThaiDatPhong.DaTraPhong;

            LblNhanPhong.Text = daCheckIn  ? "Nhận phòng (thực tế):" : "Nhận phòng (dự kiến):";
            LblTraPhong.Text  = daCheckOut ? "Trả phòng (thực tế):"  : "Trả phòng (dự kiến):";
            TxtNgayNhan.Text  = dp.NgayNhanPhong.ToString(daCheckIn  ? "dd/MM/yyyy HH:mm:ss" : "dd/MM/yyyy");
            TxtNgayTra.Text   = dp.NgayTraPhong.ToString(daCheckOut  ? "dd/MM/yyyy HH:mm:ss" : "dd/MM/yyyy");

            TxtTrangThai.Text = dp.TrangThai switch
            {
                TrangThaiDatPhong.DaDat       => "Đã Đặt – chờ nhận phòng",
                TrangThaiDatPhong.DaNhanPhong => "Đã Nhận Phòng",
                TrangThaiDatPhong.DaTraPhong  => "Đã Trả Phòng",
                TrangThaiDatPhong.HuyDat      => "Đã Hủy",
                _                             => dp.TrangThai.ToString()
            };
            TxtTienCoc.Text = dp.TienCoc > 0 ? $"{dp.TienCoc:N0} ₫" : "Không có";
            TxtGhiChu.Text  = string.IsNullOrWhiteSpace(dp.GhiChu) ? "—" : dp.GhiChu;

            var khachList = dp.DatPhongKhachHangs.Any()
                ? dp.DatPhongKhachHangs.Select(x => x.KhachHang).Where(k => k != null).Cast<KhachHangModel>().ToList()
                : dp.KhachHang != null ? new List<KhachHangModel> { dp.KhachHang } : new List<KhachHangModel>();

            IcKhachHang.ItemsSource = khachList.Select((k, i) => new KhachDisplay
            {
                TieuDe    = $"Khách {i + 1}:  {k.HoTen}",
                MaKH      = $"KH{k.MaKH:D4}",
                CMND      = string.IsNullOrWhiteSpace(k.CMND) ? "—" : k.CMND,
                GioiTinh  = k.GioiTinh == "Nu" ? "Nữ" : "Nam",
                NgaySinh  = k.NgaySinh.HasValue ? k.NgaySinh.Value.ToString("dd/MM/yyyy") : "—",
                SDT       = k.SDT ?? "—",
                QuocTich  = k.QuocTich,
                LoaiKhach = k.LoaiKhach == "NuocNgoai" ? "Nước ngoài" : "Nội địa",
                DiaChi    = k.DiaChi ?? "—"
            }).ToList();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
    }
}
