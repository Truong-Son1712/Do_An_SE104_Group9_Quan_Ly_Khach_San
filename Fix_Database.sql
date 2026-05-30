USE QuanLyKhachSan;
GO

-- Thêm các cột bị thiếu do trong file Word không có ghi
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('USER_ACCOUNT') AND name = 'IsActive')
BEGIN
    ALTER TABLE USER_ACCOUNT ADD IsActive BIT NOT NULL DEFAULT 1;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('USER_ACCOUNT') AND name = 'LastLogin')
BEGIN
    ALTER TABLE USER_ACCOUNT ADD LastLogin DATETIME NULL;
END
GO

-- Tạo các Stored Procedures
CREATE OR ALTER PROCEDURE sp_GetStaffWithoutAccount
AS
BEGIN
    SELECT s.StaffID, s.FullName, s.Position
    FROM STAFF s
    LEFT JOIN USER_ACCOUNT u ON s.StaffID = u.StaffID
    WHERE u.AccountID IS NULL;
END
GO

CREATE OR ALTER PROCEDURE sp_GetAllRoles
AS
BEGIN
    SELECT RoleID, RoleName FROM USER_ROLE;
END
GO

CREATE OR ALTER PROCEDURE sp_GetUserByUsername
    @Username VARCHAR(50)
AS
BEGIN
    SELECT u.AccountID, u.Username, u.PasswordHash, u.StaffID, u.RoleID, u.AvatarURL, u.IsActive, u.LastLogin,
           s.FullName, s.Position, r.RoleName
    FROM USER_ACCOUNT u
    JOIN STAFF s ON u.StaffID = s.StaffID
    JOIN USER_ROLE r ON u.RoleID = r.RoleID
    WHERE u.Username = @Username;
END
GO

CREATE OR ALTER PROCEDURE sp_GetAllUsers
AS
BEGIN
    SELECT u.AccountID, u.Username, u.PasswordHash, u.StaffID, u.RoleID, u.AvatarURL, u.IsActive, u.LastLogin,
           s.FullName, s.Position, r.RoleName
    FROM USER_ACCOUNT u
    JOIN STAFF s ON u.StaffID = s.StaffID
    JOIN USER_ROLE r ON u.RoleID = r.RoleID;
END
GO

CREATE OR ALTER PROCEDURE sp_CreateUser
    @AccountID VARCHAR(20),
    @Username VARCHAR(50),
    @PasswordHash VARCHAR(255),
    @StaffID VARCHAR(20),
    @RoleID INT
AS
BEGIN
    INSERT INTO USER_ACCOUNT (AccountID, Username, PasswordHash, StaffID, RoleID, IsActive)
    VALUES (@AccountID, @Username, @PasswordHash, @StaffID, @RoleID, 1);
END
GO

CREATE OR ALTER PROCEDURE sp_UpdateUser
    @AccountID VARCHAR(20),
    @Username VARCHAR(50),
    @RoleID INT,
    @IsActive BIT
AS
BEGIN
    UPDATE USER_ACCOUNT
    SET Username = @Username, RoleID = @RoleID, IsActive = @IsActive
    WHERE AccountID = @AccountID;
END
GO

CREATE OR ALTER PROCEDURE sp_ChangePassword
    @AccountID VARCHAR(20),
    @NewPasswordHash VARCHAR(255)
AS
BEGIN
    UPDATE USER_ACCOUNT
    SET PasswordHash = @NewPasswordHash
    WHERE AccountID = @AccountID;
END
GO

CREATE OR ALTER PROCEDURE sp_UpdateLastLogin
    @AccountID VARCHAR(20)
AS
BEGIN
    UPDATE USER_ACCOUNT
    SET LastLogin = GETDATE()
    WHERE AccountID = @AccountID;
END
GO

CREATE OR ALTER PROCEDURE sp_DeactivateUser
    @AccountID VARCHAR(20)
AS
BEGIN
    UPDATE USER_ACCOUNT
    SET IsActive = 0
    WHERE AccountID = @AccountID;
END
GO
