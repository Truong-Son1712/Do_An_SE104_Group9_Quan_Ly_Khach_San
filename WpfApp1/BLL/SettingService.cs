using System;
using WpfApp1.DAL;
using WpfApp1.DTO;

namespace WpfApp1.BLL
{
    /// <summary>
    /// Tầng nghiệp vụ (BLL) xử lý logic cho phần Cài đặt quy định.
    /// Thực hiện các thao tác Validate (Kiểm tra dữ liệu hợp lệ) trước khi đẩy xuống DAL.
    /// </summary>
    public class SettingService
    {
        private readonly SettingAccess _settingDal;

        public SettingService()
        {
            _settingDal = new SettingAccess();
        }

        /// <summary>
        /// Lấy cấu hình quy định hiện hành.
        /// </summary>
        public SettingDTO GetSettings()
        {
            return _settingDal.GetSettings();
        }

        /// <summary>
        /// Cập nhật cấu hình quy định sau khi kiểm tra hợp lệ.
        /// </summary>
        public void UpdateSettings(SettingDTO settings)
        {
            // Kiểm tra: Số khách tối đa phải lớn hơn 0
            if (settings.SoKhachToiDa <= 0)
            {
                throw new Exception("Số khách tối đa mỗi phòng phải lớn hơn 0.");
            }

            // Kiểm tra: Tỷ lệ phụ thu phải >= 0 và < 100%
            if (settings.PhuThuKhachThu3 < 0 || settings.PhuThuKhachThu3 > 1)
            {
                throw new Exception("Tỷ lệ phụ thu khách thứ 3 phải từ 0% đến 100%.");
            }

            // Kiểm tra: Tỷ lệ tiền cọc phải >= 0 và <= 100%
            if (settings.TyLeTienCoc < 0 || settings.TyLeTienCoc > 1)
            {
                throw new Exception("Tỷ lệ tiền cọc tối thiểu phải từ 0% đến 100%.");
            }

            // Kiểm tra: Giờ nhận phòng và trả phòng không được trùng nhau
            if (settings.GioNhanPhong == settings.GioTraPhong)
            {
                throw new Exception("Giờ nhận phòng và trả phòng không được trùng nhau.");
            }

            // Nếu mọi dữ liệu hợp lệ, chuyển xuống tầng DAL để lưu
            bool isSuccess = _settingDal.UpdateSettings(settings);
            
            if (!isSuccess)
            {
                throw new Exception("Lỗi hệ thống: Không thể cập nhật quy định vào cơ sở dữ liệu.");
            }
        }
    }
}
