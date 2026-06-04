using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels
{
    public class LoaiDichVuViewModel : BaseViewModel
    {
        private ObservableCollection<LoaiDichVu> _items = new();
        private LoaiDichVu? _selected;

        public ObservableCollection<LoaiDichVu> Items
        {
            get => _items;
            set => Set(ref _items, value);
        }

        public LoaiDichVu? Selected
        {
            get => _selected;
            set => Set(ref _selected, value);
        }

        public ICommand ThemCommand { get; }
        public ICommand SuaCommand  { get; }
        public ICommand XoaCommand  { get; }

        public LoaiDichVuViewModel()
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
                Items = new ObservableCollection<LoaiDichVu>(
                    ctx.LoaiDichVus.OrderBy(l => l.TenLoaiDV).ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Them()
        {
            var dlg = new Views.DichVu.LoaiDichVuDialog();
            if (dlg.ShowDialog() == true) LoadData();
        }

        private void Sua()
        {
            if (Selected == null) return;
            var dlg = new Views.DichVu.LoaiDichVuDialog(Selected.MaLoaiDV);
            if (dlg.ShowDialog() == true) LoadData();
        }

        private void Xoa()
        {
            if (Selected == null) return;
            try
            {
                using var ctx = new HotelDbContext();
                int soSD = ctx.DichVuPhongs.Count(d => d.MaLoaiDV == Selected.MaLoaiDV);
                if (soSD > 0)
                {
                    MessageBox.Show($"Không thể xóa loại dịch vụ \"{Selected.TenLoaiDV}\" vì đã có {soSD} lần sử dụng.",
                        "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (MessageBox.Show($"Xóa loại dịch vụ \"{Selected.TenLoaiDV}\"?",
                        "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

                var entity = ctx.LoaiDichVus.Find(Selected.MaLoaiDV);
                if (entity != null) ctx.LoaiDichVus.Remove(entity);
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
