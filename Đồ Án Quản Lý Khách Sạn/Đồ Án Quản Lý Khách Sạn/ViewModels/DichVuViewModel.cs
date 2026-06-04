using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;
using Microsoft.EntityFrameworkCore;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels
{
    // Row hiển thị trong danh sách phòng đang có khách
    public class PhongDichVuRow
    {
        public int     MaDatPhong   { get; set; }
        public string  SoPhong      { get; set; } = "";
        public string  LoaiPhong    { get; set; } = "";
        public string  KhachHang    { get; set; } = "";
        public DateTime NgayNhan    { get; set; }
        public DateTime NgayTraDuKien { get; set; }
        public int     SoDichVu     { get; set; }
        public decimal TongTienDV   { get; set; }
    }

    public class DichVuViewModel : BaseViewModel
    {
        private ObservableCollection<PhongDichVuRow> _rows = new();
        private PhongDichVuRow? _selected;

        public ObservableCollection<PhongDichVuRow> Rows
        {
            get => _rows;
            set => Set(ref _rows, value);
        }

        public PhongDichVuRow? Selected
        {
            get => _selected;
            set => Set(ref _selected, value);
        }

        public ICommand RefreshCommand    { get; }
        public ICommand QuanLyDVCommand   { get; }

        public DichVuViewModel()
        {
            RefreshCommand  = new RelayCommand(_ => LoadData());
            QuanLyDVCommand = new RelayCommand(_ => QuanLyDichVu(), _ => Selected != null);
            LoadData();
        }

        public void LoadData()
        {
            try
            {
                using var ctx = new HotelDbContext();
                var danhSach = ctx.DatPhongs
                    .Include(d => d.Phong).ThenInclude(p => p!.LoaiPhong)
                    .Include(d => d.KhachHang)
                    .Include(d => d.DatPhongKhachHangs).ThenInclude(x => x.KhachHang)
                    .Include(d => d.DichVuPhongs)
                    .Where(d => d.TrangThai == TrangThaiDatPhong.DaNhanPhong)
                    .ToList();

                Rows = new ObservableCollection<PhongDichVuRow>(
                    danhSach.Select(d => new PhongDichVuRow
                    {
                        MaDatPhong    = d.MaDatPhong,
                        SoPhong       = d.Phong?.SoPhong ?? "",
                        LoaiPhong     = d.Phong?.LoaiPhong?.TenLoaiPhong ?? "",
                        KhachHang     = d.DanhSachKhachText,
                        NgayNhan      = d.NgayNhanPhong,
                        NgayTraDuKien = d.NgayTraPhong,
                        SoDichVu      = d.DichVuPhongs.Sum(dv => dv.SoLuong),
                        TongTienDV    = d.DichVuPhongs.Sum(dv => dv.SoLuong * dv.DonGia)
                    }).OrderBy(r => r.SoPhong));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void QuanLyDichVu()
        {
            if (Selected == null) return;
            var dlg = new Views.DichVu.QuanLyDichVuPhongDialog(Selected.MaDatPhong,
                Selected.SoPhong, Selected.LoaiPhong);
            if (dlg.ShowDialog() == true) LoadData();
        }
    }
}
