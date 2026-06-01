using System.Windows;
using System.Windows.Controls;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;

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

                // Lưu lại Tag đang chọn để restore sau khi reload
                var currentTag = (CboLoaiFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "TatCa";

                CboLoaiFilter.Items.Clear();
                CboLoaiFilter.Items.Add(new ComboBoxItem { Content = "Tất cả", Tag = "TatCa" });
                foreach (var t in types)
                    CboLoaiFilter.Items.Add(new ComboBoxItem { Content = t.TenLoai, Tag = t.MaCode });

                // Restore selection
                foreach (ComboBoxItem item in CboLoaiFilter.Items)
                    if (item.Tag?.ToString() == currentTag) { item.IsSelected = true; break; }

                if (CboLoaiFilter.SelectedIndex < 0)
                    CboLoaiFilter.SelectedIndex = 0;
            }
            catch { /* silent */ }
        }
    }
}
