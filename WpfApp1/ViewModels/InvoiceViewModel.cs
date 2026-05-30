using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfApp1.BLL;
using WpfApp1.DTO;
using WpfApp1.GUI;
using WpfApp1.Helpers;

namespace WpfApp1.ViewModels
{
    public class InvoiceViewModel : BaseViewModel
    {
        private readonly InvoiceService _invoiceService = new();
        private List<CheckoutDetailDto> _allPendingBookings = new();

        public ObservableCollection<CheckoutDetailDto> PendingBookings { get; set; } = new();
        public ObservableCollection<InvoiceHistoryDto> InvoiceHistory { get; set; } = new();

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetField(ref _searchText, value))
                {
                    FilterData();
                }
            }
        }

        private CheckoutDetailDto? _selectedBooking;
        public CheckoutDetailDto? SelectedBooking
        {
            get => _selectedBooking;
            set { SetField(ref _selectedBooking, value); OnPropertyChanged(nameof(CanCheckout)); }
        }

        public bool CanCheckout => SelectedBooking != null;

        public ICommand LoadDataCommand { get; }
        public ICommand CheckoutCommand { get; }

        public InvoiceViewModel()
        {
            LoadDataCommand = new RelayCommand(LoadData);
            CheckoutCommand = new RelayCommand(ExecuteCheckout, () => CanCheckout);
            
            LoadData(); // Tự động load dữ liệu khi khởi tạo
        }

        public void LoadData()
        {
            try
            {
                _allPendingBookings = _invoiceService.GetPendingCheckouts();
                FilterData();

                InvoiceHistory.Clear();
                var history = _invoiceService.GetInvoiceHistory();
                foreach (var item in history)
                {
                    InvoiceHistory.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách: " + ex.Message);
            }
        }

        private void FilterData()
        {
            PendingBookings.Clear();
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var item in _allPendingBookings) PendingBookings.Add(item);
            }
            else
            {
                string lowerSearch = SearchText.ToLower();
                var filtered = _allPendingBookings.Where(x => 
                    x.RoomNumber.ToLower().Contains(lowerSearch) || 
                    x.BookerName.ToLower().Contains(lowerSearch) ||
                    x.BookingID.ToLower().Contains(lowerSearch));
                
                foreach (var item in filtered) PendingBookings.Add(item);
            }
        }

        private void ExecuteCheckout()
        {
            if (SelectedBooking == null) return;

            // Tính toán trước chi tiết
            var calculatedDetail = _invoiceService.CalculateCheckoutDetails(SelectedBooking);

            // Mở cửa sổ Checkout
            var checkoutWin = new CheckoutWindow(calculatedDetail);
            if (checkoutWin.ShowDialog() == true)
            {
                // Nếu thanh toán thành công, reload lại danh sách
                LoadData();
            }
        }
    }
}
