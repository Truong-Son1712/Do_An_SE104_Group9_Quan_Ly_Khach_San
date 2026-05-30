using System.Windows.Controls;

namespace WpfApp1.GUI
{
    public partial class BaoCaoView : UserControl
    {
        public BaoCaoView()
        {
            InitializeComponent();
            this.DataContext = new WpfApp1.ViewModels.BaoCaoViewModel();
        }

        private void CardLoaiPhong_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var popup = new CustomPopup();
            popup.Owner = System.Windows.Window.GetWindow(this);
            popup.ShowDialog();
        }
    }
}
