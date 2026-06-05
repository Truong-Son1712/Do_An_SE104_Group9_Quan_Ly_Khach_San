using System.Windows.Controls;
using Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.CaiDat
{
    public partial class CaiDatView : UserControl
    {
        public CaiDatView()
        {
            InitializeComponent();
            IsVisibleChanged += (_, e) =>
            {
                if ((bool)e.NewValue && DataContext is CaiDatViewModel vm)
                    vm.Refresh();
            };
        }
    }
}
