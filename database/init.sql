-- ============================================================
--  Quản Lý Khách Sạn – Grand Hotel
--  SQLite init script (schema + dữ liệu mẫu)
--
--  Cách dùng:
--    sqlite3 hotel.db < init.sql
--
--  Hoặc chỉ cần chạy ứng dụng – app tự tạo DB tại:
--    %LOCALAPPDATA%\QuanLyKhachSan\hotel.db
--
--  Tài khoản mặc định:
--    admin   / admin123   (Admin)
--    quanly  / quanly123  (Quản Lý)
--    letan   / letan123   (Lễ Tân)
--
--  Enum TrangThaiPhong  : 0=TrongSach 1=DangSuDung 2=CanDonDep 3=BaoDuong 4=DaDat
--  Enum TrangThaiDatPhong: 0=DaDat 1=DaNhanPhong 2=DaTraPhong 3=HuyDat
-- ============================================================

PRAGMA foreign_keys = OFF;

-- ── Xóa bảng cũ (thứ tự: FK trước) ──────────────────────────────────────────
DROP TABLE IF EXISTS HoaDons;
DROP TABLE IF EXISTS DatPhongKhachHangs;
DROP TABLE IF EXISTS DatPhongs;
DROP TABLE IF EXISTS KhachHangs;
DROP TABLE IF EXISTS Phongs;
DROP TABLE IF EXISTS LoaiPhongs;
DROP TABLE IF EXISTS NhanViens;
DROP TABLE IF EXISTS CauHinhs;

-- ── Schema ────────────────────────────────────────────────────────────────────

CREATE TABLE NhanViens (
    MaNV       INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    HoTen      TEXT    NOT NULL,
    TaiKhoan   TEXT    NOT NULL UNIQUE,
    MatKhau    TEXT    NOT NULL,
    VaiTro     TEXT    NOT NULL DEFAULT 'LeTan',   -- Admin | QuanLy | LeTan
    Email      TEXT,
    SDT        TEXT,
    DiaChi     TEXT,
    NgayTao    TEXT    NOT NULL DEFAULT (datetime('now','localtime')),
    IsActive   INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE LoaiPhongs (
    MaLoaiPhong  INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    TenLoaiPhong TEXT    NOT NULL,
    GiaPhong     TEXT    NOT NULL DEFAULT '0',
    SucChua      INTEGER NOT NULL DEFAULT 2,
    MoTa         TEXT
);

CREATE TABLE Phongs (
    MaPhong     INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    SoPhong     TEXT    NOT NULL,
    MaLoaiPhong INTEGER NOT NULL,
    Tang        INTEGER NOT NULL DEFAULT 1,
    TrangThai   INTEGER NOT NULL DEFAULT 0,
    MoTa        TEXT,
    FOREIGN KEY (MaLoaiPhong) REFERENCES LoaiPhongs(MaLoaiPhong) ON DELETE RESTRICT
);

CREATE TABLE KhachHangs (
    MaKH      INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    HoTen     TEXT    NOT NULL,
    CMND      TEXT    NOT NULL,
    SDT       TEXT,
    Email     TEXT,
    DiaChi    TEXT,
    QuocTich  TEXT    NOT NULL DEFAULT 'Việt Nam',
    LoaiKhach TEXT    NOT NULL DEFAULT 'NoiDia',   -- NoiDia | NuocNgoai
    NgaySinh  TEXT,
    GioiTinh  TEXT    NOT NULL DEFAULT 'Nam',
    NgayTao   TEXT    NOT NULL DEFAULT (datetime('now','localtime'))
);

CREATE TABLE DatPhongs (
    MaDatPhong    INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    MaKH          INTEGER NOT NULL,
    MaPhong       INTEGER NOT NULL,
    NgayDat       TEXT    NOT NULL DEFAULT (datetime('now','localtime')),
    NgayNhanPhong TEXT    NOT NULL,
    NgayTraPhong  TEXT    NOT NULL,
    TrangThai     INTEGER NOT NULL DEFAULT 0,       -- TrangThaiDatPhong enum
    TienCoc       TEXT    NOT NULL DEFAULT '0',
    GhiChu        TEXT,
    SoKhach       INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY (MaKH)    REFERENCES KhachHangs(MaKH)  ON DELETE RESTRICT,
    FOREIGN KEY (MaPhong) REFERENCES Phongs(MaPhong)    ON DELETE RESTRICT
);

CREATE TABLE DatPhongKhachHangs (
    MaDatPhong INTEGER NOT NULL,
    MaKH       INTEGER NOT NULL,
    PRIMARY KEY (MaDatPhong, MaKH),
    FOREIGN KEY (MaDatPhong) REFERENCES DatPhongs(MaDatPhong)   ON DELETE CASCADE,
    FOREIGN KEY (MaKH)       REFERENCES KhachHangs(MaKH)        ON DELETE RESTRICT
);

CREATE TABLE HoaDons (
    MaHD          INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    MaDatPhong    INTEGER NOT NULL UNIQUE,
    MaNV          INTEGER NOT NULL,
    NgayLap       TEXT    NOT NULL DEFAULT (datetime('now','localtime')),
    TienPhong     TEXT    NOT NULL DEFAULT '0',
    TienCoc       TEXT    NOT NULL DEFAULT '0',
    TongTien      TEXT    NOT NULL DEFAULT '0',
    PhuongThucTT  TEXT    NOT NULL DEFAULT 'TienMat',    -- TienMat | ChuyenKhoan | The
    TrangThai     TEXT    NOT NULL DEFAULT 'ChuaThanhToan', -- ChuaThanhToan | DaThanhToan
    NgayThanhToan TEXT,
    GhiChu        TEXT,
    FOREIGN KEY (MaDatPhong) REFERENCES DatPhongs(MaDatPhong) ON DELETE CASCADE,
    FOREIGN KEY (MaNV)       REFERENCES NhanViens(MaNV)       ON DELETE RESTRICT
);

CREATE TABLE CauHinhs (
    Khoa   TEXT NOT NULL PRIMARY KEY,
    GiaTri TEXT NOT NULL DEFAULT ''
);

-- ── Dữ liệu mẫu ──────────────────────────────────────────────────────────────

-- NhanViens (mật khẩu BCrypt, cost=11)
-- admin / admin123  |  quanly / quanly123  |  letan / letan123
INSERT INTO NhanViens (MaNV,HoTen,TaiKhoan,MatKhau,VaiTro,Email,SDT,DiaChi,NgayTao,IsActive) VALUES
(1,'Nguyễn Văn Nam','admin','$2a$11$oZyF2ESXb93yz/sAxfADQ.y8lY41GbJdxIIZr0VSD5y6Ad4sqfGva','Admin','admin@hotel.com','0901234560','','2026-05-25 15:30:24',1),
(2,'Trần Thị Tâm','quanly','$2a$11$tzwSrDi27V1nmndwgVZnKOoz/bG4ikNF6yuX2fxen6GgcmYdAt7y6','QuanLy','quanly@hotel.com','0901234561','','2026-05-25 15:30:24',1),
(3,'Lê Văn Nguyên','letan','$2a$11$3/IzprxXOgLMJFOl/sB8huLnqyRiQ.j63fNQulTEL47vNo2d/7PQa','LeTan','letan@hotel.com','0901234562','','2026-05-25 15:30:24',1);

-- LoaiPhongs
INSERT INTO LoaiPhongs (MaLoaiPhong,TenLoaiPhong,GiaPhong,SucChua,MoTa) VALUES
(1,'Phòng Đơn Standard','500000',1,'Phòng đơn tiêu chuẩn, 1 giường đơn'),
(2,'Phòng Đôi Standard','800000',2,'Phòng đôi tiêu chuẩn, 1 giường đôi'),
(3,'Phòng Gia Đình','1200000',4,'Phòng gia đình, 2 giường đôi'),
(4,'Phòng Deluxe','1500000',2,'Phòng Deluxe, view biển'),
(5,'Suite VIP','3000000',2,'Suite cao cấp, phòng khách riêng');

-- Phongs  (TrangThai=0 → TrongSach)
INSERT INTO Phongs (MaPhong,SoPhong,MaLoaiPhong,Tang,TrangThai,MoTa) VALUES
(1,'101',1,1,0,NULL),(2,'102',1,1,0,NULL),(3,'103',1,1,0,NULL),
(4,'104',1,1,0,NULL),(5,'105',1,1,0,NULL),(6,'106',1,1,0,NULL),
(7,'201',2,2,0,NULL),(8,'202',2,2,0,NULL),(9,'203',2,2,0,NULL),
(10,'204',2,2,0,NULL),(11,'205',2,2,0,NULL),(12,'206',2,2,0,NULL),
(13,'301',3,3,0,NULL),(14,'302',3,3,0,NULL),
(15,'303',4,3,0,NULL),(16,'304',4,3,0,NULL),
(17,'401',5,4,0,NULL),(18,'402',5,4,0,NULL);

-- KhachHangs
INSERT INTO KhachHangs (MaKH,HoTen,CMND,SDT,Email,DiaChi,QuocTich,LoaiKhach,NgaySinh,GioiTinh,NgayTao) VALUES
(1,'Nguyễn Văn An','001085012345','0901111001','an.nguyen@gmail.com','12 Lý Thường Kiệt, Hà Nội','Việt Nam','NoiDia','1990-03-15','Nam','2026-05-25 15:30:25'),
(2,'Trần Thị Bình','079085067890','0902222002','binh.tran@gmail.com','45 Nguyễn Huệ, TP.HCM','Việt Nam','NoiDia','1995-07-22','Nu','2026-05-25 15:30:25'),
(3,'Lê Minh Châu','048085034567','0903333003','chau.le@gmail.com','78 Trần Phú, Đà Nẵng','Việt Nam','NoiDia','1988-11-05','Nam','2026-05-25 15:30:25'),
(4,'Phạm Thu Hà','036085089012','0904444004','ha.pham@gmail.com','23 Hoàng Diệu, Huế','Việt Nam','NoiDia','1993-05-18','Nu','2026-05-25 15:30:25'),
(5,'Võ Quốc Hùng','092085056789','0905555005','hung.vo@gmail.com','56 Pasteur, Cần Thơ','Việt Nam','NoiDia','1985-09-30','Nam','2026-05-25 15:30:25'),
(6,'John Smith','A12345678','+1-202-555-0101','john.smith@gmail.com','New York, USA','Hoa Kỳ','NuocNgoai','1982-04-12','Nam','2026-05-25 15:30:25'),
(7,'Wang Fang','G87654321','+86-138-0000-1234','wang.fang@qq.com','Beijing, China','Trung Quốc','NuocNgoai','1991-08-20','Nu','2026-05-25 15:30:25'),
(8,'Nguyễn Thị Lan','001090023456','0908888008','lan.nguyen@yahoo.com','99 Đinh Tiên Hoàng, Hà Nội','Việt Nam','NoiDia','1997-01-08','Nu','2026-05-25 15:30:25'),
(9,'Đặng Văn Đức','025090045678','0909999009','duc.dang@gmail.com','34 Lê Lợi, Hải Phòng','Việt Nam','NoiDia','1989-06-25','Nam','2026-05-25 15:30:25'),
(10,'Tanaka Yuki','TK9876543','+81-90-1234-5678','tanaka.y@mail.jp','Tokyo, Japan','Nhật Bản','NuocNgoai','1994-03-03','Nu','2026-05-25 15:30:25');

-- CauHinhs (cấu hình hệ thống)
INSERT INTO CauHinhs (Khoa,GiaTri) VALUES
('HeSoNuocNgoai','1.2'),    -- Hệ số giá khách nước ngoài (1.2 = +20%)
('SucChuaToiDa','4'),       -- Số khách tối đa tuyệt đối được phép đặt 1 phòng
('TiLePhuThu','0.25');      -- Tỷ lệ phụ thu khi vượt SucChua của loại phòng (0.25 = 25%)

PRAGMA foreign_keys = ON;
