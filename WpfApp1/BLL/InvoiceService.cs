using System;
using System.Collections.Generic;
using System.Linq;
using WpfApp1.DAL;
using WpfApp1.DTO;

namespace WpfApp1.BLL
{
    public class InvoiceService
    {
        private readonly InvoiceAccess _invoiceAccess = new();

        public List<CheckoutDetailDto> GetPendingCheckouts()
        {
            return _invoiceAccess.GetBookingsForCheckout();
        }

        public List<InvoiceHistoryDto> GetInvoiceHistory()
        {
            return _invoiceAccess.GetInvoiceHistory();
        }

        public CheckoutDetailDto CalculateCheckoutDetails(CheckoutDetailDto dto)
        {
            // 1. Tính số ngày ở
            // Nếu trả phòng cùng ngày thì tính 1 ngày, nếu khác ngày thì làm tròn lên
            TimeSpan duration = DateTime.Now - dto.CheckInDate;
            int totalDays = (int)Math.Ceiling(duration.TotalDays);
            if (totalDays <= 0) totalDays = 1;

            dto.TotalDays = totalDays;

            // 2. Tiền phòng cơ bản
            dto.RoomCharge = totalDays * dto.PricePerDay;

            // 3. Tính phụ thu theo Quy định 4
            // Lấy danh sách quốc tịch khách trong phòng
            var nationalities = _invoiceAccess.GetGuestNationalities(dto.BookingID);
            
            decimal surchargeRatio = 1.0m;
            List<string> reasons = new();

            // Khách thứ 3 trở lên -> Phụ thu 25% (Hệ số = Hệ số + 0.25)
            // Trong đề bài nói "khách thứ 3", giả sử phòng chuẩn 2 người, khách thứ 3 phụ thu 25%.
            if (dto.NumberOfGuests >= 3)
            {
                surchargeRatio += 0.25m;
                reasons.Add("Phụ thu 25% (khách thứ 3)");
            }

            // Có khách nước ngoài -> Hệ số 1.5
            // "Khách nước ngoài" có thể là so sánh chuỗi Nationality != "Việt Nam"
            bool hasForeigner = nationalities.Any(n => !n.Equals("Việt Nam", StringComparison.OrdinalIgnoreCase));
            if (hasForeigner)
            {
                surchargeRatio *= 1.5m;
                reasons.Add("Hệ số x1.5 (khách nước ngoài)");
            }

            // Số tiền sau khi nhân hệ số
            decimal roomAfterSurcharge = dto.RoomCharge * surchargeRatio;
            
            // SurchargeAmount = Tổng tiền sau phụ thu - Tiền gốc
            dto.SurchargeAmount = roomAfterSurcharge - dto.RoomCharge;

            dto.TotalAmount = dto.RoomCharge + dto.SurchargeAmount;
            dto.SurchargeReason = string.Join(" | ", reasons);

            return dto;
        }

        public (bool Success, string Message, Invoice? CreatedInvoice) ProcessCheckout(CheckoutDetailDto dto, string paymentMethod)
        {
            try
            {
                // Generate InvoiceID (thực tế có thể dùng auto increment hoặc GUID)
                string invoiceId = "INV" + DateTime.Now.ToString("yyyyMMddHHmmss");

                var invoice = new Invoice
                {
                    InvoiceID = invoiceId,
                    BookingID = dto.BookingID,
                    StaffID = "NV001", // Default Admin
                    IssuedDate = DateTime.Now,
                    RoomCharge = dto.RoomCharge,
                    SurchargeAmount = dto.SurchargeAmount,
                    ServiceCharge = 0, // Dịch vụ khác
                    TotalAmount = dto.TotalAmount,
                    PaymentMethod = paymentMethod
                };

                _invoiceAccess.CreateInvoice(invoice);

                return (true, "Thanh toán thành công!", invoice);
            }
            catch (Exception ex)
            {
                return (false, "Lỗi thanh toán: " + ex.Message, null);
            }
        }
    }
}
