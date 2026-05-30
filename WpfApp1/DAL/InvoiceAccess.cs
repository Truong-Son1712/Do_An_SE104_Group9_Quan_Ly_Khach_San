using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using WpfApp1.DTO;

namespace WpfApp1.DAL
{
    public class InvoiceAccess
    {
        public List<CheckoutDetailDto> GetBookingsForCheckout()
        {
            var list = new List<CheckoutDetailDto>();
            using var conn = DatabaseConnection.GetConnection();
            using var cmd = new SqlCommand("sp_GetBookingsForCheckout", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new CheckoutDetailDto
                {
                    BookingID = reader["BookingID"].ToString()!,
                    RoomID = reader["RoomID"].ToString()!,
                    RoomNumber = reader["RoomNumber"].ToString()!,
                    BookerName = reader["BookerName"].ToString()!,
                    CheckInDate = Convert.ToDateTime(reader["CheckInDate"]),
                    NumberOfGuests = Convert.ToInt32(reader["NumberOfGuests"]),
                    PricePerDay = Convert.ToDecimal(reader["PricePerDay"])
                });
            }
            return list;
        }

        // Dùng Dictionary hoặc danh sách tuple để lấy thông tin khách. 
        // Ở đây trả về danh sách Nationality để tính phụ thu.
        public List<string> GetGuestNationalities(string bookingId)
        {
            var list = new List<string>();
            using var conn = DatabaseConnection.GetConnection();
            using var cmd = new SqlCommand("sp_GetBookingGuests", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@BookingID", bookingId);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(reader["Nationality"].ToString()!);
            }
            return list;
        }

        public void CreateInvoice(Invoice invoice)
        {
            using var conn = DatabaseConnection.GetConnection();
            using var cmd = new SqlCommand("sp_CreateInvoice", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@InvoiceID", invoice.InvoiceID);
            cmd.Parameters.AddWithValue("@BookingID", invoice.BookingID);
            cmd.Parameters.AddWithValue("@StaffID", invoice.StaffID);
            cmd.Parameters.AddWithValue("@RoomCharge", invoice.RoomCharge);
            cmd.Parameters.AddWithValue("@SurchargeAmount", invoice.SurchargeAmount);
            cmd.Parameters.AddWithValue("@ServiceCharge", invoice.ServiceCharge);
            cmd.Parameters.AddWithValue("@TotalAmount", invoice.TotalAmount);
            cmd.Parameters.AddWithValue("@PaymentMethod", invoice.PaymentMethod);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public List<InvoiceHistoryDto> GetInvoiceHistory()
        {
            var list = new List<InvoiceHistoryDto>();
            using var conn = DatabaseConnection.GetConnection();
            using var cmd = new SqlCommand("sp_GetAllInvoices", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new InvoiceHistoryDto
                {
                    InvoiceID = reader["InvoiceID"].ToString()!,
                    BookingID = reader["BookingID"].ToString()!,
                    BookerName = reader["BookerName"].ToString()!,
                    RoomNumber = reader["RoomNumber"].ToString()!,
                    IssuedDate = Convert.ToDateTime(reader["IssuedDate"]),
                    TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                    PaymentMethod = reader["PaymentMethod"].ToString()!
                });
            }
            return list;
        }
    }
}
