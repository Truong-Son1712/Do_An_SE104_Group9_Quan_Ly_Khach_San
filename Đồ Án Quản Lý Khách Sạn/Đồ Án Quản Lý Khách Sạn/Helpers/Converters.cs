using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Helpers
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value switch
            {
                bool b => b,
                string s => !string.IsNullOrEmpty(s),
                _ => false
            };
            if (parameter is string p && p == "Inverse") flag = !flag;
            return flag ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNull = value == null;
            if (parameter is string s && s == "Inverse") isNull = !isNull;
            return isNull ? Visibility.Collapsed : Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class CurrencyConverter : IValueConverter
    {
        private static readonly CultureInfo Vi = new("vi-VN");
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value switch
            {
                decimal d => d.ToString("N0", Vi) + " ₫",
                double dbl => dbl.ToString("N0", Vi) + " ₫",
                _ => "0 ₫"
            };
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class TrangThaiPhongTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is TrangThaiPhong t ? t switch
            {
                TrangThaiPhong.TrongSach   => "Trống - Sạch",
                TrangThaiPhong.DangSuDung  => "Đang Sử Dụng",
                TrangThaiPhong.CanDonDep   => "Cần Dọn Dẹp",
                TrangThaiPhong.BaoDuong    => "Bảo Dưỡng",
                TrangThaiPhong.DaDat       => "Đã Đặt",
                _                          => t.ToString()
            } : value?.ToString() ?? "";
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class TrangThaiPhongColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string hex = value is TrangThaiPhong t ? t switch
            {
                TrangThaiPhong.TrongSach  => "#2ECC71",
                TrangThaiPhong.DangSuDung => "#E74C3C",
                TrangThaiPhong.CanDonDep  => "#F39C12",
                TrangThaiPhong.BaoDuong   => "#95A5A6",
                TrangThaiPhong.DaDat      => "#3498DB",
                _                         => "#BDC3C7"
            } : "#BDC3C7";
            return new BrushConverter().ConvertFromString(hex) as SolidColorBrush
                   ?? Brushes.Gray;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class TrangThaiDatPhongTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is TrangThaiDatPhong t ? t switch
            {
                TrangThaiDatPhong.DaDat       => "Đã Đặt",
                TrangThaiDatPhong.DaNhanPhong  => "Đã Nhận Phòng",
                TrangThaiDatPhong.DaTraPhong   => "Đã Trả Phòng",
                TrangThaiDatPhong.HuyDat       => "Đã Hủy",
                _                              => t.ToString()
            } : value?.ToString() ?? "";
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class TrangThaiDatPhongColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string hex = value is TrangThaiDatPhong t ? t switch
            {
                TrangThaiDatPhong.DaDat       => "#3498DB",
                TrangThaiDatPhong.DaNhanPhong  => "#27AE60",
                TrangThaiDatPhong.DaTraPhong   => "#7F8C8D",
                TrangThaiDatPhong.HuyDat       => "#E74C3C",
                _                              => "#BDC3C7"
            } : "#BDC3C7";
            return new BrushConverter().ConvertFromString(hex) as SolidColorBrush ?? Brushes.Gray;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class VaiTroTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value?.ToString() switch
            {
                "Admin"  => "Quản Trị Viên",
                "QuanLy" => "Quản Lý",
                "LeTan"  => "Lễ Tân",
                _        => value?.ToString() ?? ""
            };
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class LoaiKhachTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value?.ToString() switch
            {
                "NoiDia"    => "Nội Địa",
                "NuocNgoai" => "Nước Ngoài",
                _           => value?.ToString() ?? ""
            };
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class GioiTinhTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value?.ToString() switch
            {
                "Nam" => "Nam",
                "Nu"  => "Nữ",
                _     => value?.ToString() ?? ""
            };
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class HoaDonTrangThaiColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string hex = value?.ToString() switch
            {
                "DaThanhToan"    => "#27AE60",
                "ChuaThanhToan"  => "#E74C3C",
                _                => "#BDC3C7"
            };
            return new BrushConverter().ConvertFromString(hex) as SolidColorBrush ?? Brushes.Gray;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class HoaDonTrangThaiTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value?.ToString() switch
            {
                "DaThanhToan"   => "Đã Thanh Toán",
                "ChuaThanhToan" => "Chưa Thanh Toán",
                _               => value?.ToString() ?? ""
            };
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class PhuongThucTTTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value?.ToString() switch
            {
                "TienMat"     => "Tiền Mặt",
                "ChuyenKhoan" => "Chuyển Khoản",
                "The"         => "Thẻ Ngân Hàng",
                _             => value?.ToString() ?? ""
            };
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class SoNgayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int days)
                return days == 1 ? "1 ngày" : $"{days} ngày";
            return "0 ngày";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
