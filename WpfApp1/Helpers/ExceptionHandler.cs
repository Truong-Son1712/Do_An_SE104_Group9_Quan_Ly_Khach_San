using Microsoft.Data.SqlClient;
using System.IO;
using System.Windows;

namespace WpfApp1.Helpers
{
    /// <summary>
    /// Xử lý exception tập trung cho toàn bộ ứng dụng.
    /// </summary>
    public static class ExceptionHandler
    {
        // -------------------------------------------------------
        // Custom Exceptions
        // -------------------------------------------------------

        public class AuthenticationException : Exception
        {
            public AuthenticationException(string message) : base(message) { }
        }

        public class ValidationException : Exception
        {
            public ValidationException(string message) : base(message) { }
        }

        public class NotFoundException : Exception
        {
            public NotFoundException(string message) : base(message) { }
        }

        public class DuplicateException : Exception
        {
            public DuplicateException(string message) : base(message) { }
        }

        // -------------------------------------------------------
        // Xử lý và trả về message thân thiện
        // -------------------------------------------------------

        public static string Handle(Exception ex, string context = "")
        {
            Log(ex, context);

            return ex switch
            {
                AuthenticationException ae => ae.Message,
                ValidationException     ve => ve.Message,
                NotFoundException       ne => ne.Message,
                DuplicateException      de => de.Message,
                SqlException            se => HandleSql(se),
                _                          => $"Lỗi hệ thống: {ex.Message}"
            };
        }

        /// <summary>Hiển thị MessageBox lỗi.</summary>
        public static void ShowError(Exception ex, string context = "")
        {
            string msg = Handle(ex, context);
            MessageBox.Show(msg, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // -------------------------------------------------------
        private static string HandleSql(SqlException ex) => ex.Number switch
        {
            547  => "Vi phạm ràng buộc dữ liệu.",
            2627 => "Dữ liệu đã tồn tại (trùng khóa).",
            2601 => "Giá trị bị trùng lặp.",
            -1   => "Mất kết nối đến SQL Server.",
            _    => $"Lỗi SQL ({ex.Number}): {ex.Message}"
        };

        private static void Log(Exception ex, string context)
        {
            try
            {
                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{context}] {ex.GetType().Name}: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(line);

                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                File.AppendAllText(logPath, line + Environment.NewLine);
            }
            catch { /* không throw nếu log thất bại */ }
        }
    }
}