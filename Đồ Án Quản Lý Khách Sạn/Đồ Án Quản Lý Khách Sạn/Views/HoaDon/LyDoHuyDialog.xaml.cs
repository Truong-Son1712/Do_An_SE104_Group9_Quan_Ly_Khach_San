using System.Windows;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.HoaDon
{
    public partial class LyDoHuyDialog : Window
    {
        public string LyDo { get; private set; } = "";

        public LyDoHuyDialog(bool daThanhToan = false)
        {
            InitializeComponent();
            if (daThanhToan)
            {
                TxtWarning.Text = "Hóa đơn này ĐÃ ĐƯỢC THANH TOÁN. Hủy sẽ xóa hóa đơn và rollback doanh thu.";
                PnlWarning.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEE"));
                TxtWarning.Foreground = System.Windows.Media.Brushes.Crimson;
            }
            Loaded += (_, _) => TxtLyDo.Focus();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            string ly = TxtLyDo.Text.Trim();
            if (string.IsNullOrEmpty(ly))
            {
                TxtError.Text      = "Vui lòng nhập lý do hủy.";
                PnlError.Visibility = Visibility.Visible;
                TxtLyDo.Focus();
                return;
            }
            LyDo         = ly;
            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
    }
}
