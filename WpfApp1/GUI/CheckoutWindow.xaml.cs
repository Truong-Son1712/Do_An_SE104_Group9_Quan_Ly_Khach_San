using System;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.BLL;
using WpfApp1.DTO;
using WpfApp1.Helpers;

namespace WpfApp1.GUI
{
    public partial class CheckoutWindow : Window
    {
        private readonly CheckoutDetailDto _detail;
        private readonly InvoiceService _invoiceService = new();

        public CheckoutWindow(CheckoutDetailDto detail)
        {
            InitializeComponent();
            _detail = detail;

            // Load data to UI
            TxtCustomer.Text = _detail.BookerName;
            TxtRoom.Text = _detail.RoomNumber;
            TxtCheckIn.Text = _detail.CheckInDate.ToString("dd/MM/yyyy HH:mm");
            TxtTotalDays.Text = _detail.TotalDays.ToString();
            TxtSurchargeReason.Text = string.IsNullOrEmpty(_detail.SurchargeReason) ? "Không có" : _detail.SurchargeReason;

            TxtRoomCharge.Text = _detail.RoomCharge.ToString("N0") + " VNĐ";
            TxtSurcharge.Text = _detail.SurchargeAmount.ToString("N0") + " VNĐ";
            TxtTotal.Text = _detail.TotalAmount.ToString("N0") + " VNĐ";
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            string paymentMethod = (CboPaymentMethod.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Tiền mặt";

            var (success, message, createdInvoice) = _invoiceService.ProcessCheckout(_detail, paymentMethod);
            if (success && createdInvoice != null)
            {
                MessageBox.Show(message, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Mở hộp thoại chọn nơi lưu file PDF
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    DefaultExt = ".pdf",
                    FileName = $"HoaDon_{createdInvoice.InvoiceID}.pdf",
                    Title = "Chọn nơi lưu Hóa Đơn PDF"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        PdfExportHelper.ExportInvoiceToPdf(createdInvoice, _detail, saveFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi xuất PDF: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
