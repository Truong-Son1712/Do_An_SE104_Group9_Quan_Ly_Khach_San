using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WpfApp1.BLL;
using WpfApp1.DTO;

namespace WpfApp1.ViewModels
{
    /// <summary>
    /// ViewModel hỗ trợ binding dữ liệu từ BLL lên giao diện SettingView.xaml
    /// Kế thừa BaseViewModel nếu có sẵn, ở đây tự implement INotifyPropertyChanged để chắc chắn tương thích.
    /// </summary>
    public class SettingViewModel : INotifyPropertyChanged
    {
        private readonly SettingService _service;
        
        // Các biến lưu trữ dữ liệu binding
        private string _phuThuKhachThu3Str;
        private string _soKhachToiDaStr;
        private string _tyLeTienCocStr;
        private string _gioNhanPhongStr;
        private string _gioTraPhongStr;
        
        private string _message;
        private bool _isError;

        // Sự kiện Notify cho XAML biết khi dữ liệu thay đổi
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string PhuThuKhachThu3Str
        {
            get => _phuThuKhachThu3Str;
            set { _phuThuKhachThu3Str = value; OnPropertyChanged(); }
        }

        public string SoKhachToiDaStr
        {
            get => _soKhachToiDaStr;
            set { _soKhachToiDaStr = value; OnPropertyChanged(); }
        }

        public string TyLeTienCocStr
        {
            get => _tyLeTienCocStr;
            set { _tyLeTienCocStr = value; OnPropertyChanged(); }
        }

        public string GioNhanPhongStr
        {
            get => _gioNhanPhongStr;
            set { _gioNhanPhongStr = value; OnPropertyChanged(); }
        }

        public string GioTraPhongStr
        {
            get => _gioTraPhongStr;
            set { _gioTraPhongStr = value; OnPropertyChanged(); }
        }

        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        public bool IsError
        {
            get => _isError;
            set { _isError = value; OnPropertyChanged(); }
        }

        // Lệnh lưu cấu hình
        public ICommand SaveCommand { get; }

        public SettingViewModel()
        {
            _service = new SettingService();
            SaveCommand = new RelayCommandParam(ExecuteSave);
            LoadData();
        }

        /// <summary>
        /// Tải dữ liệu quy định từ BLL lên màn hình.
        /// </summary>
        private void LoadData()
        {
            try
            {
                var settings = _service.GetSettings();
                // Chuyển format số thập phân thành % để người dùng dễ nhìn (0.25 -> 25)
                PhuThuKhachThu3Str = (settings.PhuThuKhachThu3 * 100).ToString("0.##");
                TyLeTienCocStr = (settings.TyLeTienCoc * 100).ToString("0.##");
                
                SoKhachToiDaStr = settings.SoKhachToiDa.ToString();
                
                // Format giờ hh:mm
                GioNhanPhongStr = settings.GioNhanPhong.ToString(@"hh\:mm");
                GioTraPhongStr = settings.GioTraPhong.ToString(@"hh\:mm");
            }
            catch (Exception ex)
            {
                ShowMessage($"Lỗi tải dữ liệu: {ex.Message}", true);
            }
        }

        /// <summary>
        /// Thực thi lưu dữ liệu xuống BLL
        /// </summary>
        private void ExecuteSave(object obj)
        {
            try
            {
                // Parse dữ liệu từ TextBox do người dùng nhập
                if (!decimal.TryParse(PhuThuKhachThu3Str, out decimal phuThu))
                    throw new Exception("Tỷ lệ phụ thu không hợp lệ.");
                if (!decimal.TryParse(TyLeTienCocStr, out decimal tienCoc))
                    throw new Exception("Tỷ lệ tiền cọc không hợp lệ.");
                if (!int.TryParse(SoKhachToiDaStr, out int soKhach))
                    throw new Exception("Số khách tối đa không hợp lệ.");
                if (!TimeSpan.TryParse(GioNhanPhongStr, out TimeSpan gioNhan))
                    throw new Exception("Giờ nhận phòng không hợp lệ (Định dạng hh:mm).");
                if (!TimeSpan.TryParse(GioTraPhongStr, out TimeSpan gioTra))
                    throw new Exception("Giờ trả phòng không hợp lệ (Định dạng hh:mm).");

                var newSettings = new SettingDTO
                {
                    PhuThuKhachThu3 = phuThu / 100, // Đổi % về số thập phân (25 -> 0.25)
                    TyLeTienCoc = tienCoc / 100,
                    SoKhachToiDa = soKhach,
                    GioNhanPhong = gioNhan,
                    GioTraPhong = gioTra
                };

                // Gọi BLL để validate và lưu
                _service.UpdateSettings(newSettings);
                
                ShowMessage("Lưu quy định thành công!", false);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, true);
            }
        }

        private void ShowMessage(string msg, bool isError)
        {
            Message = msg;
            IsError = isError;
        }
    }

    /// <summary>
    /// Class hỗ trợ binding sự kiện Click của Button vào ViewModel (ICommand)
    /// </summary>
    public class RelayCommandParam : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommandParam(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
