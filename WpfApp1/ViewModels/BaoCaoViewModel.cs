using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WpfApp1.BLL;
using WpfApp1.DTO;

namespace WpfApp1.ViewModels
{
    public class BaoCaoViewModel : INotifyPropertyChanged
    {
        private readonly ReportService _service;
        
        private int _selectedYear;
        private decimal _tongDoanhThuNam;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int SelectedYear
        {
            get => _selectedYear;
            set { _selectedYear = value; OnPropertyChanged(); LoadData(); }
        }

        public decimal TongDoanhThuNam
        {
            get => _tongDoanhThuNam;
            set { _tongDoanhThuNam = value; OnPropertyChanged(); }
        }

        // --- CÁC THUỘC TÍNH MỚI BỔ SUNG CHO UI ---
        private int _tongHoaDon;
        public int TongHoaDon { get => _tongHoaDon; set { _tongHoaDon = value; OnPropertyChanged(); } }

        private int _luotKhach;
        public int LuotKhach { get => _luotKhach; set { _luotKhach = value; OnPropertyChanged(); } }

        private double _congSuatPhong;
        public double CongSuatPhong { get => _congSuatPhong; set { _congSuatPhong = value; OnPropertyChanged(); } }

        private int _khachNoiDia;
        public int KhachNoiDia { get => _khachNoiDia; set { _khachNoiDia = value; OnPropertyChanged(); } }

        private int _khachNuocNgoai;
        public int KhachNuocNgoai { get => _khachNuocNgoai; set { _khachNuocNgoai = value; OnPropertyChanged(); } }
        // ------------------------------------------

        public ObservableCollection<RevenueReportDTO> DoanhThuTheoThang { get; set; }
        public ObservableCollection<int> DanhSachNam { get; set; }

        public BaoCaoViewModel()
        {
            _service = new ReportService();
            DoanhThuTheoThang = new ObservableCollection<RevenueReportDTO>();
            DanhSachNam = new ObservableCollection<int> { 2024, 2025, 2026 };
            
            SelectedYear = DateTime.Now.Year; // Sẽ tự động trigger LoadData
        }

        private void LoadData()
        {
            var data = _service.GetDoanhThuTheoNam(SelectedYear);
            
            DoanhThuTheoThang.Clear();
            foreach(var item in data)
            {
                DoanhThuTheoThang.Add(item);
            }

            TongDoanhThuNam = data.Sum(x => x.DoanhThu);

            // MOCK DATA MỚI
            TongHoaDon = 120;
            LuotKhach = 350;
            CongSuatPhong = 65.5;
            KhachNoiDia = 250;
            KhachNuocNgoai = 100;
        }
    }
}
