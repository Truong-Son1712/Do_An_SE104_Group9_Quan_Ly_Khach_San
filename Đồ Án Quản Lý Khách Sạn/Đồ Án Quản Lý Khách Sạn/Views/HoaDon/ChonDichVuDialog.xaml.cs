using System.Windows;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.HoaDon
{
    public partial class ChonDichVuDialog : Window
    {
        public ChonDichVuDialog()
        {
            InitializeComponent();
        }

        private void BtnSelect_Click(object sender, RoutedEventArgs e) => Close();
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
    }
}
