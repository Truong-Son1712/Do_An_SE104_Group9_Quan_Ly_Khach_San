using System.Windows;
using System.Windows.Controls;
using Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.BaoCao
{
    /// Lớp code-behind cho màn hình Báo cáo doanh thu và Thống kê công suất phòng
    public partial class BaoCaoView : UserControl
    {
        /// Khởi tạo các thành phần giao diện của màn hình Báo cáo
        public BaoCaoView() => InitializeComponent();

        /// Mở hộp thoại Thống kê doanh thu theo Loại phòng của năm đang chọn
        private void BtnThongKeLoaiPhong_Click(object sender, RoutedEventArgs e)
        {
            var vm = (BaoCaoViewModel)DataContext;
            var dlg = new ThongKeLoaiPhongDialog(vm.SelectedYear) { Owner = Window.GetWindow(this) };
            dlg.ShowDialog();
        }

        /// Mở hộp thoại Thống kê số lượt thuê/công suất sử dụng từng phòng cụ thể
        private void BtnThongKePhong_Click(object sender, RoutedEventArgs e)
        {
            var vm = (BaoCaoViewModel)DataContext;
            var dlg = new ThongKePhongDialog(vm.SelectedYear) { Owner = Window.GetWindow(this) };
            dlg.ShowDialog();
        }
    }
}
