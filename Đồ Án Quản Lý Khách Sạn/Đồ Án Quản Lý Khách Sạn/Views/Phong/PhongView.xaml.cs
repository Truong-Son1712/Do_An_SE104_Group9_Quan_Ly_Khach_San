using System.Windows;
using System.Windows.Controls;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.Phong
{
    public partial class PhongView : UserControl
    {
        public PhongView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            decimal heSo = AppConfig.GetHeSoNuocNgoai();
            TxtHeSo.Text = heSo.ToString("0.##");

            bool isAdmin = SessionManager.IsAdmin;
            TxtHeSo.IsReadOnly = !isAdmin;
            BtnLuuHeSo.IsEnabled = isAdmin;
            BtnLuuHeSo.Opacity = isAdmin ? 1.0 : 0.4;
            TxtHeSoNote.Text = isAdmin
                ? $"Giá khách nước ngoài = Giá phòng × {heSo:0.##}  (chỉ Admin mới chỉnh được)"
                : $"Giá khách nước ngoài = Giá phòng × {heSo:0.##}  (chỉ Admin mới chỉnh được)";
        }

        private void BtnLuuHeSo_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionManager.IsAdmin)
            {
                MessageBox.Show("Chỉ Admin mới có quyền thay đổi hệ số.", "Không có quyền",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtHeSo.Text.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal heSo)
                || heSo <= 0)
            {
                MessageBox.Show("Hệ số phải là số dương (ví dụ: 1.5).", "Giá trị không hợp lệ",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AppConfig.SetHeSoNuocNgoai(heSo);
            TxtHeSoNote.Text = $"Giá khách nước ngoài = Giá phòng × {heSo:0.##}  (chỉ Admin mới chỉnh được)";
            MessageBox.Show($"Đã lưu hệ số khách nước ngoài: {heSo:0.##}x",
                "Lưu Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
