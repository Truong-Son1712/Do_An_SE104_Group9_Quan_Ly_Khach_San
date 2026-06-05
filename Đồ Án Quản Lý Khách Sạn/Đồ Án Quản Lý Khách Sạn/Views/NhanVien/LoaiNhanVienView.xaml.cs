using System.Windows.Controls;
using Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.NhanVien
{
    public partial class LoaiNhanVienView : UserControl
    {
        public LoaiNhanVienView()
        {
            InitializeComponent();
            DataContext = new LoaiNhanVienViewModel();
        }
    }
}
