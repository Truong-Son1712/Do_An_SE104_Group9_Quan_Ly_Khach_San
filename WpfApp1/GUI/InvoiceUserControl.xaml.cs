using System.Windows.Controls;
using WpfApp1.ViewModels;

namespace WpfApp1.GUI
{
    public partial class InvoiceUserControl : UserControl
    {
        public InvoiceUserControl()
        {
            InitializeComponent();
            DataContext = new InvoiceViewModel();
        }
    }
}
