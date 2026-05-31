using System.Windows;
using System.Windows.Controls;
using Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.BaoCao
{
    public partial class BaoCaoView : UserControl
    {
        public BaoCaoView() => InitializeComponent();

        private void BtnThongKeLoaiPhong_Click(object sender, RoutedEventArgs e)
        {
            var vm = (BaoCaoViewModel)DataContext;
            var dlg = new ThongKeLoaiPhongDialog(vm.SelectedYear) { Owner = Window.GetWindow(this) };
            dlg.ShowDialog();
        }

        private void BtnThongKePhong_Click(object sender, RoutedEventArgs e)
        {
            var vm = (BaoCaoViewModel)DataContext;
            var dlg = new ThongKePhongDialog(vm.SelectedYear) { Owner = Window.GetWindow(this) };
            dlg.ShowDialog();
        }
    }
}
