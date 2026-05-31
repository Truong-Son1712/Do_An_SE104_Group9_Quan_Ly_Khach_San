using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;
using Microsoft.EntityFrameworkCore;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.Phong
{
    public partial class DoiTrangThaiPhongDialog : Window
    {
        private readonly int _maPhong;
        private TrangThaiPhong _trangThaiHienTai;

        public TrangThaiPhong TrangThaiMoi { get; private set; }

        public DoiTrangThaiPhongDialog(int maPhong)
        {
            InitializeComponent();
            _maPhong = maPhong;
            LoadInfo();
        }

        private void LoadInfo()
        {
            using var ctx = new HotelDbContext();
            var phong = ctx.Phongs.Include(p => p.LoaiPhong).FirstOrDefault(p => p.MaPhong == _maPhong);
            if (phong == null) return;

            _trangThaiHienTai = phong.TrangThai;

            TxtSoPhong.Text         = $"Phòng {phong.SoPhong}";
            TxtLoaiPhong.Text       = phong.LoaiPhong?.TenLoaiPhong ?? "";
            TxtTrangThaiHienTai.Text = ConvertTrangThai(_trangThaiHienTai);
            BadgeCurrent.Background  = new SolidColorBrush(GetColor(_trangThaiHienTai));

            // Chọn sẵn trạng thái hiện tại trong ComboBox
            foreach (ComboBoxItem item in CboTrangThai.Items)
                if (item.Tag?.ToString() == _trangThaiHienTai.ToString())
                { item.IsSelected = true; break; }
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (CboTrangThai.SelectedItem is not ComboBoxItem selected) return;
            if (!Enum.TryParse<TrangThaiPhong>(selected.Tag?.ToString(), out var tt)) return;

            if (tt == _trangThaiHienTai)
            {
                MessageBox.Show("Trạng thái không thay đổi.", "Thông Báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                using var ctx = new HotelDbContext();
                var phong = ctx.Phongs.Find(_maPhong);
                if (phong == null) return;
                phong.TrangThai = tt;
                ctx.SaveChanges();

                TrangThaiMoi = tt;
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();

        private static string ConvertTrangThai(TrangThaiPhong tt) => tt switch
        {
            TrangThaiPhong.TrongSach   => "Trống (Sạch)",
            TrangThaiPhong.DangSuDung  => "Đang Sử Dụng",
            TrangThaiPhong.CanDonDep   => "Cần Dọn Dẹp",
            TrangThaiPhong.BaoDuong    => "Bảo Dưỡng",
            TrangThaiPhong.DaDat       => "Đã Đặt",
            _                          => tt.ToString()
        };

        private static Color GetColor(TrangThaiPhong tt) => tt switch
        {
            TrangThaiPhong.TrongSach  => (Color)ColorConverter.ConvertFromString("#2ECC71"),
            TrangThaiPhong.DangSuDung => (Color)ColorConverter.ConvertFromString("#E74C3C"),
            TrangThaiPhong.CanDonDep  => (Color)ColorConverter.ConvertFromString("#F39C12"),
            TrangThaiPhong.BaoDuong   => (Color)ColorConverter.ConvertFromString("#95A5A6"),
            TrangThaiPhong.DaDat      => (Color)ColorConverter.ConvertFromString("#3498DB"),
            _                         => Colors.Gray
        };
    }
}
