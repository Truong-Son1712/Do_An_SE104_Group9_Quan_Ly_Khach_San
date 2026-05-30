using System;

namespace WpfApp1.DTO
{
    public class InvoiceHistoryDto
    {
        public string InvoiceID { get; set; } = string.Empty;
        public string BookingID { get; set; } = string.Empty;
        public string BookerName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime IssuedDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
