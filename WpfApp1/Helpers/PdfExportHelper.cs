using System;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WpfApp1.DTO;

namespace WpfApp1.Helpers
{
    public static class PdfExportHelper
    {
        public static void ExportInvoiceToPdf(Invoice invoice, CheckoutDetailDto detail, string filePath)
        {
            // Thiết lập License của QuestPDF (Bắt buộc từ version 2022.12)
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial")); // Tự động dùng font Windows

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(x => ComposeContent(x, invoice, detail));
                    page.Footer().Element(ComposeFooter);
                });
            })
            .GeneratePdf(filePath);

            // Tự động mở file PDF sau khi xuất
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception) { /* Bỏ qua nếu máy tính không có trình đọc PDF */ }
        }

        private static void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("HÓA ĐƠN THANH TOÁN")
                        .FontSize(24).SemiBold().FontColor(Colors.Blue.Darken2);
                    
                    col.Item().Text(text =>
                    {
                        text.Span("Ngày in: ").SemiBold();
                        text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                    });
                });
            });
        }

        private static void ComposeContent(IContainer container, Invoice invoice, CheckoutDetailDto detail)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Spacing(20);

                // Thông tin khách hàng & phòng
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Cell().Text($"Khách hàng: {detail.BookerName}");
                    table.Cell().Text($"Phòng: {detail.RoomNumber}");
                    table.Cell().Text($"Mã hóa đơn: {invoice.InvoiceID}");
                    table.Cell().Text($"Ngày nhận phòng: {detail.CheckInDate:dd/MM/yyyy HH:mm}");
                    table.Cell().Text($"Hình thức TT: {invoice.PaymentMethod}");
                    table.Cell().Text($"Số ngày ở: {detail.TotalDays}");
                });

                // Chi tiết tính tiền
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(50);
                        columns.RelativeColumn();
                        columns.ConstantColumn(120);
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Text("#").SemiBold();
                        header.Cell().Text("Khoản mục").SemiBold();
                        header.Cell().AlignRight().Text("Thành tiền").SemiBold();
                        header.Cell().ColumnSpan(3).PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    // Rows
                    table.Cell().Text("1");
                    table.Cell().Text("Tiền phòng cơ bản");
                    table.Cell().AlignRight().Text($"{invoice.RoomCharge:N0} đ");

                    table.Cell().Text("2");
                    table.Cell().Text($"Phụ thu\n(Lý do: {detail.SurchargeReason})");
                    table.Cell().AlignRight().Text($"{invoice.SurchargeAmount:N0} đ");

                    table.Cell().ColumnSpan(3).PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    
                    // Total
                    table.Cell().ColumnSpan(2).AlignRight().Text("TỔNG CỘNG:").SemiBold().FontSize(16);
                    table.Cell().AlignRight().Text($"{invoice.TotalAmount:N0} đ").SemiBold().FontSize(16).FontColor(Colors.Red.Medium);
                });
            });
        }

        private static void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Xin cảm ơn và hẹn gặp lại quý khách! ").FontSize(10);
            });
        }
    }
}
