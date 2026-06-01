using System.Windows;
using System.Windows.Controls;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.KhachHang
{
    public partial class KhachHangView : UserControl
    {
        public KhachHangView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) => LoadFilterOptions();

        public void LoadFilterOptions()
        {
            try
            {
                using var ctx = new HotelDbContext();
                var types = ctx.LoaiKhachHangs.OrderBy(l => l.TenLoai).ToList();

                var currentTag = (CboLoaiFilter.SelectedItem as ComboBoxItem)?.Tag;
                int currentMaLKH = currentTag is int t ? t : 0;

                CboLoaiFilter.Items.Clear();
                CboLoaiFilter.Items.Add(new ComboBoxItem { Content = "Tất cả", Tag = 0 });
                foreach (var loai in types)
                    CboLoaiFilter.Items.Add(new ComboBoxItem { Content = loai.TenLoai, Tag = loai.MaLKH });

                foreach (ComboBoxItem item in CboLoaiFilter.Items)
                    if (item.Tag is int id && id == currentMaLKH) { item.IsSelected = true; break; }

                if (CboLoaiFilter.SelectedIndex < 0) CboLoaiFilter.SelectedIndex = 0;
            }
            catch { /* silent */ }
        }
    }
}
