using System.Windows.Controls;

namespace WpfApp1.GUI
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            this.DataContext = new WpfApp1.ViewModels.DashboardViewModel();
        }
    }
}
