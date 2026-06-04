using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;
using Microsoft.EntityFrameworkCore;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels
{
    public class MaGiamGiaViewModel : BaseViewModel
    {
        private ObservableCollection<MaGiamGia> _items = new();
        private MaGiamGia? _selected;

        public ObservableCollection<MaGiamGia> Items
        {
            get => _items;
            set => Set(ref _items, value);
        }

        public MaGiamGia? Selected
        {
            get => _selected;
            set => Set(ref _selected, value);
        }

        public bool IsAdmin => SessionManager.IsAdmin;
        public bool IsQuanLy => SessionManager.IsQuanLy;

        public ICommand ThemCommand          { get; }
        public ICommand SuaCommand           { get; }
        public ICommand XoaCommand           { get; }
        public ICommand ToggleActiveCommand  { get; }

        public MaGiamGiaViewModel()
        {
            ThemCommand         = new RelayCommand(_ => Them(), _ => IsQuanLy);
            SuaCommand          = new RelayCommand(_ => Sua(),  _ => Selected != null && IsQuanLy);
            XoaCommand          = new RelayCommand(_ => Xoa(),  _ => Selected != null && IsQuanLy);
            ToggleActiveCommand = new RelayCommand(_ => ToggleActive(), _ => Selected != null && IsQuanLy);
            LoadData();
        }

        public void LoadData()
        {
            try
            {
                using var ctx = new HotelDbContext();
                var list = ctx.MaGiamGias
                    .Include(m => m.LichSuDungs)
                    .Include(m => m.ChiTiets)
                    .OrderByDescending(m => m.NgayTao)
                    .ToList();
                Items = new ObservableCollection<MaGiamGia>(list);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Them()
        {
            var dlg = new Views.MaGiamGia.ThemSuaMaGiamGiaDialog();
            if (dlg.ShowDialog() == true) LoadData();
        }

        private void Sua()
        {
            if (Selected == null) return;
            var dlg = new Views.MaGiamGia.ThemSuaMaGiamGiaDialog(Selected.MaGG);
            if (dlg.ShowDialog() == true) LoadData();
        }

        private void Xoa()
        {
            if (Selected == null) return;
            try
            {
                using var ctx = new HotelDbContext();
                bool daCoNguoiDung = ctx.LichSuDungMaGiams.Any(l => l.MaGG == Selected.MaGG);
                if (daCoNguoiDung)
                {
                    MessageBox.Show(
                        $"Không thể xóa mã \"{Selected.TenMa}\" vì đã có hóa đơn sử dụng mã này.",
                        "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (MessageBox.Show($"Xóa mã giảm giá \"{Selected.TenMa}\"?",
                        "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

                var entity = ctx.MaGiamGias.Find(Selected.MaGG);
                if (entity != null) ctx.MaGiamGias.Remove(entity);
                ctx.SaveChanges();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleActive()
        {
            if (Selected == null) return;
            try
            {
                using var ctx = new HotelDbContext();
                var entity = ctx.MaGiamGias.Find(Selected.MaGG);
                if (entity == null) return;

                if (entity.TrangThai == "Active")
                {
                    // Tắt: luôn cho phép
                    entity.TrangThai = "Inactive";
                }
                else
                {
                    // Bật lại: kiểm tra có mã cùng tên đang hoạt động không
                    var now = DateTime.Now;
                    bool trungActive = ctx.MaGiamGias.Any(m =>
                        m.TenMa     == entity.TenMa
                        && m.MaGG   != entity.MaGG
                        && m.TrangThai == "Active"
                        && m.NgayBatDau  <= now
                        && m.NgayKetThuc >= now);

                    if (trungActive)
                    {
                        MessageBox.Show(
                            $"Không thể bật mã \"{entity.TenMa}\" vì đang có mã khác cùng tên đang hoạt động.",
                            "Không thể bật", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    entity.TrangThai = "Active";
                }

                ctx.SaveChanges();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
