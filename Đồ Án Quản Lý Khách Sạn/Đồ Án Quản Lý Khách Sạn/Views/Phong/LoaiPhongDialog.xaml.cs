using System.Windows;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.Phong
{
    public partial class LoaiPhongDialog : Window
    {
        private readonly int? _maLP;

        public LoaiPhongDialog(int? maLoaiPhong = null)
        {
            InitializeComponent();
            _maLP = maLoaiPhong;
            if (maLoaiPhong.HasValue) LoadData(maLoaiPhong.Value);
        }

        private void LoadData(int id)
        {
            using var ctx = new HotelDbContext();
            var lp = ctx.LoaiPhongs.Find(id);
            if (lp == null) return;
            TxtTitle.Text = "Chỉnh Sửa Loại Phòng";
            TxtTenLoaiPhong.Text = lp.TenLoaiPhong;
            TxtGiaPhong.Text = lp.GiaPhong.ToString("N0");
            TxtSucChua.Text = lp.SucChua.ToString();
            TxtMoTa.Text = lp.MoTa;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            PnlError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(TxtTenLoaiPhong.Text))
            { ShowError("Vui lòng nhập tên loại phòng."); return; }

            string giaStr = TxtGiaPhong.Text.Replace(",", "").Replace(".", "").Trim();
            if (!decimal.TryParse(giaStr, out decimal gia) || gia <= 0)
            { ShowError("Giá phòng không hợp lệ."); return; }

            if (!int.TryParse(TxtSucChua.Text, out int sc) || sc < 1)
            { ShowError("Sức chứa phải là số nguyên dương."); return; }

            try
            {
                using var ctx = new HotelDbContext();
                if (_maLP.HasValue)
                {
                    var lp = ctx.LoaiPhongs.Find(_maLP.Value);
                    if (lp == null) return;
                    lp.TenLoaiPhong = TxtTenLoaiPhong.Text.Trim();
                    lp.GiaPhong = gia;
                    lp.SucChua = sc;
                    lp.MoTa = TxtMoTa.Text.Trim();
                }
                else
                {
                    ctx.LoaiPhongs.Add(new LoaiPhong
                    {
                        TenLoaiPhong = TxtTenLoaiPhong.Text.Trim(),
                        GiaPhong = gia,
                        SucChua = sc,
                        MoTa = TxtMoTa.Text.Trim()
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
