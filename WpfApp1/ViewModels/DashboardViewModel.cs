using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WpfApp1.BLL;
using WpfApp1.DTO;

namespace WpfApp1.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly ReportService _service;
        private DashboardDTO _data;
        private string _thangHienTai;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DashboardDTO Data
        {
            get => _data;
            set { _data = value; OnPropertyChanged(); }
        }

        public string ThangHienTai
        {
            get => _thangHienTai;
            set { _thangHienTai = value; OnPropertyChanged(); }
        }

        // Dành cho biểu đồ hình tròn mini (Ty le)
        public double TyLePhongTrong => Data != null && Data.TongPhong > 0 ? (double)Data.PhongTrong / Data.TongPhong * 100 : 0;
        public double TyLePhongSuDung => Data != null && Data.TongPhong > 0 ? (double)Data.PhongDangSuDung / Data.TongPhong * 100 : 0;

        public ICommand RefreshCommand { get; }

        public DashboardViewModel()
        {
            _service = new ReportService();
            RefreshCommand = new RelayCommandParam(_ => LoadData());
            LoadData();
        }

        private void LoadData()
        {
            ThangHienTai = $"Tháng {DateTime.Now.Month}/{DateTime.Now.Year}";
            Data = _service.GetDashboardData();
            
            OnPropertyChanged(nameof(TyLePhongTrong));
            OnPropertyChanged(nameof(TyLePhongSuDung));
        }
    }
}
