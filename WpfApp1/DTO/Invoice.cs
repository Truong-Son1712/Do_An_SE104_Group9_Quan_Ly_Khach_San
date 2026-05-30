using System;

namespace WpfApp1.DTO
{
    public class Invoice
    {
        public string InvoiceID { get; set; } = string.Empty;
        public string BookingID { get; set; } = string.Empty;
        public string StaffID { get; set; } = string.Empty;
        public DateTime IssuedDate { get; set; }
        public decimal RoomCharge { get; set; }
        public decimal SurchargeAmount { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
