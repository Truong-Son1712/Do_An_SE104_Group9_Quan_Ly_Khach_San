using System.Windows;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.DichVu
{
    public partial class LoaiDichVuDialog : Window
    {
        private readonly int? _maLoaiDV;

        public LoaiDichVuDialog(int? maLoaiDV = null)
        {
            InitializeComponent();
            _maLoaiDV = maLoaiDV;
            if (maLoaiDV.HasValue)
            {
                TxtTitle.Text = "Chỉnh Sửa Loại Dịch Vụ";
                LoadData(maLoaiDV.Value);
            }
        }

        private void LoadData(int id)
        {
            using var ctx = new HotelDbContext();
            var item = ctx.LoaiDichVus.Find(id);
            if (item == null) return;
            TxtTenLoaiDV.Text  = item.TenLoaiDV;
            TxtDonGia.Text     = item.DonGia.ToString("N0");
            TxtDonViTinh.Text  = item.DonViTinh;
            TxtMoTa.Text       = item.MoTa;
            ChkIsActive.IsChecked = item.IsActive;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            PnlError.Visibility = Visibility.Collapsed;

            string ten = TxtTenLoaiDV.Text.Trim();
            if (string.IsNullOrEmpty(ten))
            { ShowError("Vui lòng nhập tên dịch vụ."); return; }

            string donViTinh = TxtDonViTinh.Text.Trim();
            if (string.IsNullOrEmpty(donViTinh))
            { ShowError("Vui lòng nhập đơn vị tính."); return; }

            string giaStr = TxtDonGia.Text.Replace(",", "").Replace(".", "").Trim();
            if (!decimal.TryParse(giaStr, out decimal donGia) || donGia < 0)
            { ShowError("Đơn giá phải là số không âm."); return; }

            try
            {
                using var ctx = new HotelDbContext();
                if (_maLoaiDV.HasValue)
                {
                    var item = ctx.LoaiDichVus.Find(_maLoaiDV.Value);
                    if (item == null) return;
                    item.TenLoaiDV  = ten;
                    item.DonGia     = donGia;
                    item.DonViTinh  = donViTinh;
                    item.MoTa       = TxtMoTa.Text.Trim();
                    item.IsActive   = ChkIsActive.IsChecked ?? true;
                }
                else
                {
                    ctx.LoaiDichVus.Add(new LoaiDichVu
                    {
                        TenLoaiDV = ten,
                        DonGia    = donGia,
                        DonViTinh = donViTinh,
                        MoTa      = TxtMoTa.Text.Trim(),
                        IsActive  = ChkIsActive.IsChecked ?? true
                    });
                }
                ctx.SaveChanges();
                DialogResult = true;
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
        private void ShowError(string msg) { TxtError.Text = msg; PnlError.Visibility = Visibility.Visible; }
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
    }
}
