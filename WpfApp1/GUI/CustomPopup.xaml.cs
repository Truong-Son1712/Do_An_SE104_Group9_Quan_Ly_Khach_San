using System.Windows;

namespace WpfApp1.GUI
{
    public partial class CustomPopup : Window
    {
        public CustomPopup()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
