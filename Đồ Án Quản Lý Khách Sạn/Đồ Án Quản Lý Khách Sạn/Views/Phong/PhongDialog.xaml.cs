using System.Windows;
using System.Windows.Controls;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;
using Microsoft.EntityFrameworkCore;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.Phong
{
    public partial class PhongDialog : Window
    {
        private readonly int? _maPhong;

        public PhongDialog(int? maPhong = null)
        {
            InitializeComponent();
            _maPhong = maPhong;
            LoadLoaiPhong();
            if (maPhong.HasValue) LoadPhong(maPhong.Value);
        }

        private void LoadLoaiPhong()
        {
            using var ctx = new HotelDbContext();
            CboLoaiPhong.ItemsSource = ctx.LoaiPhongs.OrderBy(l => l.TenLoaiPhong).ToList();
        }

        private void LoadPhong(int maPhong)
        {
            using var ctx = new HotelDbContext();
            var p = ctx.Phongs.Find(maPhong);
            if (p == null) return;

            TxtTitle.Text = "Chỉnh Sửa Phòng";
            TxtSoPhong.Text = p.SoPhong;
            TxtTang.Text = p.Tang.ToString();
            CboLoaiPhong.SelectedValue = p.MaLoaiPhong;
            TxtMoTa.Text = p.MoTa;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            PnlError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(TxtSoPhong.Text))
            { ShowError("Vui lòng nhập số phòng."); return; }
            if (!int.TryParse(TxtTang.Text, out int tang) || tang < 1)
            { ShowError("Tầng phải là số nguyên dương."); return; }
            if (CboLoaiPhong.SelectedValue is not int maLoai)
            { ShowError("Vui lòng chọn loại phòng."); return; }

            try
            {
                using var ctx = new HotelDbContext();
                if (_maPhong.HasValue)
                {
                    var p = ctx.Phongs.Find(_maPhong.Value);
                    if (p == null) return;
                    p.SoPhong = TxtSoPhong.Text.Trim();
                    p.Tang = tang;
                    p.MaLoaiPhong = maLoai;
                    p.MoTa = TxtMoTa.Text.Trim();
                }
                else
                {
                    if (ctx.Phongs.Any(x => x.SoPhong == TxtSoPhong.Text.Trim()))
                    { ShowError($"Phòng số \"{TxtSoPhong.Text.Trim()}\" đã tồn tại."); return; }

                    ctx.Phongs.Add(new Models.Phong
                    {
                        SoPhong = TxtSoPhong.Text.Trim(),
                        Tang = tang,
                        MaLoaiPhong = maLoai,
                        TrangThai = TrangThaiPhong.TrongSach,
                        MoTa = TxtMoTa.Text.Trim()
                    });
                }
                ctx.SaveChanges();
                DialogResult = true;
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi lưu dữ liệu: {ex.Message}");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();

        private void ShowError(string msg)
        {
            TxtError.Text = msg;
            PnlError.Visibility = Visibility.Visible;
        }
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();

    }
}
