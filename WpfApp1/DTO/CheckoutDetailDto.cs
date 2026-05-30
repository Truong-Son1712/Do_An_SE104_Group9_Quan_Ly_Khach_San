using System;

namespace WpfApp1.DTO
{
    public class CheckoutDetailDto
    {
        public string BookingID { get; set; } = string.Empty;
        public string RoomID { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string BookerName { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal PricePerDay { get; set; }
        
        // Fields tính toán thêm
        public int TotalDays { get; set; }
        public decimal RoomCharge { get; set; }
        public decimal SurchargeAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string SurchargeReason { get; set; } = string.Empty;
    }
}
