using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels
{
    public class LoaiPhongViewModel : BaseViewModel
    {
        private ObservableCollection<LoaiPhong> _loaiPhongs = new();
        private LoaiPhong? _selected;

        public ObservableCollection<LoaiPhong> LoaiPhongs
        {
            get => _loaiPhongs;
            set => Set(ref _loaiPhongs, value);
        }

        public LoaiPhong? Selected
        {
            get => _selected;
            set => Set(ref _selected, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand ThemCommand    { get; }
        public ICommand SuaCommand     { get; }
        public ICommand XoaCommand     { get; }

        public LoaiPhongViewModel()
        {
            RefreshCommand = new RelayCommand(_ => LoadData());
            ThemCommand    = new RelayCommand(_ => ThemLoaiPhong());
            SuaCommand     = new RelayCommand(_ => SuaLoaiPhong(), _ => Selected != null);
            XoaCommand     = new RelayCommand(_ => XoaLoaiPhong(), _ => Selected != null);
            LoadData();
        }

        public void LoadData()
        {
            try
            {
                using var ctx = new HotelDbContext();
                LoaiPhongs = new ObservableCollection<LoaiPhong>(
                    ctx.LoaiPhongs.OrderBy(l => l.TenLoaiPhong).ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThemLoaiPhong()
        {
            var dlg = new Views.Phong.LoaiPhongDialog();
            if (dlg.ShowDialog() == true) LoadData();
        }

        private void SuaLoaiPhong()
        {
            if (Selected == null) return;
            var dlg = new Views.Phong.LoaiPhongDialog(Selected.MaLoaiPhong);
            if (dlg.ShowDialog() == true) LoadData();
        }

        private void XoaLoaiPhong()
        {
            if (Selected == null) return;
            if (MessageBox.Show($"Xóa loại phòng \"{Selected.TenLoaiPhong}\"?",
                "Xác Nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

            try
            {
                using var ctx = new HotelDbContext();
                if (ctx.Phongs.Any(p => p.MaLoaiPhong == Selected.MaLoaiPhong))
                {
                    MessageBox.Show("Không thể xóa vì đang có phòng thuộc loại này.", "Cảnh Báo",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var lp = ctx.LoaiPhongs.Find(Selected.MaLoaiPhong);
                if (lp != null) { ctx.LoaiPhongs.Remove(lp); ctx.SaveChanges(); }
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
