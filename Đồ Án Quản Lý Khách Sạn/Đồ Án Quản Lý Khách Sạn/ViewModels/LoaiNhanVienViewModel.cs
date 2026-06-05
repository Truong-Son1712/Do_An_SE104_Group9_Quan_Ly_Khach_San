using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;
using Microsoft.EntityFrameworkCore;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels
{
    public class LoaiNhanVienViewModel : BaseViewModel
    {
        private ObservableCollection<LoaiNhanVien> _items = new();
        private LoaiNhanVien? _selected;

        public ObservableCollection<LoaiNhanVien> Items
        {
            get => _items;
            set => Set(ref _items, value);
        }

        public LoaiNhanVien? Selected
        {
            get => _selected;
            set => Set(ref _selected, value);
        }

        public ICommand ThemCommand       { get; }
        public ICommand SuaQuyenCommand   { get; }
        public ICommand XoaCommand        { get; }
        public ICommand RefreshCommand    { get; }

        public LoaiNhanVienViewModel()
        {
            ThemCommand     = new RelayCommand(_ => Them());
            SuaQuyenCommand = new RelayCommand(_ => SuaQuyen(),
                _ => Selected != null && Selected.VaiTroCode != "Admin");
            XoaCommand      = new RelayCommand(_ => Xoa(),
                _ => Selected != null && Selected.VaiTroCode != "Admin");
            RefreshCommand  = new RelayCommand(_ => LoadData());
            LoadData();
        }

        public void LoadData()
        {
            using var ctx = new HotelDbContext();
            var loais = ctx.LoaiNhanViens
                .Include(l => l.Quyens)
                .OrderBy(l => l.IsBuiltIn ? 0 : 1)
                .ThenBy(l => l.MaLoaiNV)
                .ToList();

            // Tính số nhân viên cho từng loại qua FK MaLoaiNV
            var nhanVienCounts = ctx.NhanViens
                .Where(n => n.MaLoaiNV != null)
                .GroupBy(n => n.MaLoaiNV)
                .ToDictionary(g => g.Key!.Value, g => g.Count());

            foreach (var l in loais)
                l.SoNhanVien = nhanVienCounts.TryGetValue(l.MaLoaiNV, out var c) ? c : 0;

            Items = new ObservableCollection<LoaiNhanVien>(loais);
        }

        private void Them()
        {
            var dlg = new Views.NhanVien.LoaiNhanVienDialog();
            if (dlg.ShowDialog() == true) LoadData();
        }

        private void SuaQuyen()
        {
            if (Selected == null) return;
            var dlg = new Views.NhanVien.LoaiNhanVienDialog(Selected.MaLoaiNV);
            if (dlg.ShowDialog() == true) LoadData();
        }

        private void Xoa()
        {
            if (Selected == null || Selected.VaiTroCode == "Admin") return;

            using var ctx = new HotelDbContext();
            int soNV = ctx.NhanViens.Count(n => n.MaLoaiNV == Selected.MaLoaiNV);
            if (soNV > 0)
            {
                MessageBox.Show(
                    $"Không thể xóa loại nhân viên \"{Selected.TenLoai}\" vì còn {soNV} nhân viên thuộc loại này.",
                    "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show(
                    $"Xóa loại nhân viên \"{Selected.TenLoai}\"?",
                    "Xác Nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                    MessageBoxResult.No) != MessageBoxResult.Yes) return;

            var loai = ctx.LoaiNhanViens.Find(Selected.MaLoaiNV);
            if (loai != null) { ctx.LoaiNhanViens.Remove(loai); ctx.SaveChanges(); }
            LoadData();
        }
    }
}
