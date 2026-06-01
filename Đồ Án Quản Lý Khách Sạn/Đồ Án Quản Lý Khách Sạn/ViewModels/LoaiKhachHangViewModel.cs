using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels
{
    public class LoaiKhachHangViewModel : BaseViewModel
    {
        private ObservableCollection<LoaiKhachHang> _items = new();
        private LoaiKhachHang? _selected;

        public ObservableCollection<LoaiKhachHang> Items
        {
            get => _items;
            set => Set(ref _items, value);
        }

        public LoaiKhachHang? Selected
        {
            get => _selected;
            set => Set(ref _selected, value);
        }

        public ICommand ThemCommand { get; }
        public ICommand SuaCommand  { get; }
        public ICommand XoaCommand  { get; }

        public LoaiKhachHangViewModel()
        {
            ThemCommand = new RelayCommand(_ => Them());
            SuaCommand  = new RelayCommand(_ => Sua(),  _ => Selected != null);
            XoaCommand  = new RelayCommand(_ => Xoa(),  _ => Selected != null);
            LoadData();
        }

        public void LoadData()
        {
            try
            {
                using var ctx = new HotelDbContext();
                Items = new ObservableCollection<LoaiKhachHang>(
                    ctx.LoaiKhachHangs.OrderBy(l => l.TenLoai).ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Them()
        {
            var dlg = new Views.KhachHang.LoaiKhachHangDialog();
            if (dlg.ShowDialog() == true) LoadData();
        }

        private void Sua()
        {
            if (Selected == null) return;
            var dlg = new Views.KhachHang.LoaiKhachHangDialog(Selected.MaLKH);
            if (dlg.ShowDialog() == true) LoadData();
        }

        private void Xoa()
        {
            if (Selected == null) return;

            try
            {
                using var ctx = new HotelDbContext();

                if (ctx.LoaiKhachHangs.Count() <= 1)
                {
                    MessageBox.Show("Phải có ít nhất 1 loại khách hàng. Không thể xóa.",
                        "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int soKhach = ctx.KhachHangs.Count(k => k.LoaiKhach == Selected.MaCode);
                if (soKhach > 0)
                {
                    MessageBox.Show($"Không thể xóa loại khách \"{Selected.TenLoai}\" vì có {soKhach} khách hàng đang sử dụng.",
                        "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var confirm = MessageBox.Show(
                    $"Xóa loại khách \"{Selected.TenLoai}\"?",
                    "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm != MessageBoxResult.Yes) return;

                var entity = ctx.LoaiKhachHangs.Find(Selected.MaLKH);
                if (entity != null) ctx.LoaiKhachHangs.Remove(entity);
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
