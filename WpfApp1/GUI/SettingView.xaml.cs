using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfApp1.GUI
{
    public partial class SettingView : UserControl
    {
        public SettingView()
        {
            InitializeComponent();
            this.DataContext = new WpfApp1.ViewModels.SettingViewModel();
        }
    }

    /// <summary>
    /// Converter nhỏ giúp đổi màu TextBlock thông báo (Lỗi -> Đỏ, Thành công -> Xanh)
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isError && isError)
            {
                return new SolidColorBrush(Colors.Red);
            }
            return new SolidColorBrush(Colors.Green);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
