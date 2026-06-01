using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Helpers
{
    public static class AppConfig
    {
        private const string KEY_HE_SO         = "HeSoNuocNgoai";
        private const string KEY_SUC_CHUA_MAX  = "SucChuaToiDa";
        private const string KEY_TI_LE_PHU_THU = "TiLePhuThu";

        // ── Hệ số giá khách nước ngoài (legacy – vẫn giữ để tương thích) ──
        public static decimal GetHeSoNuocNgoai()
        {
            using var ctx = new HotelDbContext();
            var val = ctx.CauHinhs.Find(KEY_HE_SO)?.GiaTri;
            return decimal.TryParse(val, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 1.5m;
        }

        public static void SetHeSoNuocNgoai(decimal value)
        {
            using var ctx = new HotelDbContext();
            var cfg = ctx.CauHinhs.Find(KEY_HE_SO);
            if (cfg == null)
                ctx.CauHinhs.Add(new CauHinh { Khoa = KEY_HE_SO, GiaTri = value.ToString(System.Globalization.CultureInfo.InvariantCulture) });
            else
                cfg.GiaTri = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            ctx.SaveChanges();
        }

        // ── Hệ số giá theo mã loại khách ────────────────────────────────────
        public static decimal GetHeSoByCode(string maCode)
        {
            if (string.IsNullOrEmpty(maCode)) return 1m;
            using var ctx = new HotelDbContext();
            return ctx.LoaiKhachHangs.Find(maCode)?.HeSoGia ?? 1m;
        }

        // Nhân tất cả hệ số của các loại khách trong booking lại với nhau.
        // Ví dụ: NuocNgoai(×1.2) và KhachDuLich(×1.3) → ×1.56
        public static decimal GetCombinedHeSo(IEnumerable<string> maCodes)
        {
            var codes = maCodes.Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
            if (!codes.Any()) return 1m;
            using var ctx = new HotelDbContext();
            var heSos = ctx.LoaiKhachHangs
                .Where(l => codes.Contains(l.MaCode))
                .Select(l => l.HeSoGia)
                .ToList();
            return heSos.Any() ? heSos.Aggregate(1m, (acc, h) => acc * h) : 1m;
        }

        // Kept for backward compatibility
        public static decimal GetMaxHeSo(IEnumerable<string> maCodes) => GetCombinedHeSo(maCodes);

        // ── Sức chứa tối đa ────────────────────────────────────────────────
        public static int GetSucChuaToiDa()
        {
            using var ctx = new HotelDbContext();
            var val = ctx.CauHinhs.Find(KEY_SUC_CHUA_MAX)?.GiaTri;
            return int.TryParse(val, out var d) ? d : 4;
        }

        public static void SetSucChuaToiDa(int value)
        {
            using var ctx = new HotelDbContext();
            var cfg = ctx.CauHinhs.Find(KEY_SUC_CHUA_MAX);
            if (cfg == null)
                ctx.CauHinhs.Add(new CauHinh { Khoa = KEY_SUC_CHUA_MAX, GiaTri = value.ToString() });
            else
                cfg.GiaTri = value.ToString();
            ctx.SaveChanges();
        }

        // ── Tỷ lệ phụ thu khi vượt sức chứa phòng ─────────────────────────
        public static decimal GetTiLePhuThu()
        {
            using var ctx = new HotelDbContext();
            var val = ctx.CauHinhs.Find(KEY_TI_LE_PHU_THU)?.GiaTri;
            return decimal.TryParse(val, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0.25m;
        }

        public static void SetTiLePhuThu(decimal value)
        {
            using var ctx = new HotelDbContext();
            var cfg = ctx.CauHinhs.Find(KEY_TI_LE_PHU_THU);
            if (cfg == null)
                ctx.CauHinhs.Add(new CauHinh { Khoa = KEY_TI_LE_PHU_THU, GiaTri = value.ToString(System.Globalization.CultureInfo.InvariantCulture) });
            else
                cfg.GiaTri = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            ctx.SaveChanges();
        }
    }
}
