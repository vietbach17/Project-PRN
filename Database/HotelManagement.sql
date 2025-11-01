create database HotelManagement
use HotelManagement;
GO
create table Users(
    UserId           INT IDENTITY(1,1) CONSTRAINT PK_Users PRIMARY KEY,
    Username         NVARCHAR(100) NOT NULL CONSTRAINT UQ_Users_Username UNIQUE,
    PasswordHash     NVARCHAR(256) NOT NULL,
    Role             NVARCHAR(50)  NOT NULL,
    IsActive         BIT NOT NULL DEFAULT(1),
    CreatedAt        DATETIME2(0) NOT NULL DEFAULT(SYSDATETIME()),
    UpdatedAt        DATETIME2(0) NULL
);
GO

create table Customers(

    CustomerId       INT IDENTITY(1,1) CONSTRAINT PK_Customers PRIMARY KEY,
    FullName         NVARCHAR(200) NOT NULL,
    Phone            NVARCHAR(30)  NULL,
    Email            NVARCHAR(256) NULL,
    IDNumber         NVARCHAR(50)  NULL,
    Address          NVARCHAR(300) NULL,
    CreatedAt        DATETIME2(0) NOT NULL DEFAULT(SYSDATETIME())

);
GO

create table RoomTypes(
    RoomTypeId       INT IDENTITY(1,1) CONSTRAINT PK_RoomTypes PRIMARY KEY,
    Name             NVARCHAR(100) NOT NULL CONSTRAINT UQ_RoomTypes_Name UNIQUE,
    Capacity         INT NOT NULL CHECK (Capacity > 0),
    BasePrice        DECIMAL(18,2) NOT NULL CHECK (BasePrice >= 0),
    Description      NVARCHAR(300) NULL,
    CreatedAt        DATETIME2(0) NOT NULL DEFAULT(SYSDATETIME())
);
GO

CREATE TABLE Rooms (
    RoomId           INT IDENTITY(1,1) CONSTRAINT PK_Rooms PRIMARY KEY,
    RoomNumber       NVARCHAR(20) NOT NULL CONSTRAINT UQ_Rooms_RoomNumber UNIQUE,
    Floor            INT NULL,
    RoomTypeId       INT NOT NULL CONSTRAINT FK_Rooms_RoomTypes REFERENCES  RoomTypes(RoomTypeId),
    Status           NVARCHAR(30) NOT NULL DEFAULT('Available')
        CHECK (Status IN ('Available','Occupied','Cleaning','Maintenance')),
    PricePerNight    DECIMAL(18,2) NOT NULL CHECK (PricePerNight >= 0),
    CreatedAt        DATETIME2(0) NOT NULL DEFAULT(SYSDATETIME())
);
GO

CREATE TABLE Bookings (
    BookingId        INT IDENTITY(1,1) CONSTRAINT PK_Bookings PRIMARY KEY,
    CustomerId       INT NOT NULL CONSTRAINT FK_Bookings_Customers REFERENCES  Customers(CustomerId),
    CheckInDate      DATE NOT NULL,
    CheckOutDate     DATE NOT NULL,
    Status           NVARCHAR(30) NOT NULL DEFAULT('Reserved')
        CHECK (Status IN ('Reserved','CheckedIn','CheckedOut','Cancelled')),
    Notes            NVARCHAR(500) NULL,
    CreatedAt        DATETIME2(0) NOT NULL DEFAULT(SYSDATETIME()),
    CONSTRAINT CK_Bookings_Dates CHECK (CheckOutDate > CheckInDate)
);
GO

CREATE TABLE BookingRooms (
    BookingRoomId    INT IDENTITY(1,1) CONSTRAINT PK_BookingRooms PRIMARY KEY,
    BookingId        INT NOT NULL CONSTRAINT FK_BookingRooms_Bookings REFERENCES  Bookings(BookingId) ON DELETE CASCADE,
    RoomId           INT NOT NULL CONSTRAINT FK_BookingRooms_Rooms REFERENCES  Rooms(RoomId),
    Guests           INT NOT NULL DEFAULT(1) CHECK (Guests > 0),
    RatePerNight     DECIMAL(18,2) NOT NULL CHECK (RatePerNight >= 0)
);
GO

CREATE TABLE Services (
    ServiceId        INT IDENTITY(1,1) CONSTRAINT PK_Services PRIMARY KEY,
    Name             NVARCHAR(150) NOT NULL CONSTRAINT UQ_Services_Name UNIQUE,
    Price            DECIMAL(18,2) NOT NULL CHECK (Price >= 0),
    Unit             NVARCHAR(50) NOT NULL DEFAULT('unit')
);
GO

CREATE TABLE BookingServices (
    BookingServiceId INT IDENTITY(1,1) CONSTRAINT PK_BookingServices PRIMARY KEY,
    BookingId        INT NOT NULL CONSTRAINT FK_BookingServices_Bookings REFERENCES  Bookings(BookingId) ON DELETE CASCADE,
    ServiceId        INT NOT NULL CONSTRAINT FK_BookingServices_Services REFERENCES  Services(ServiceId),
    Quantity         INT NOT NULL DEFAULT(1) CHECK (Quantity > 0),
    UnitPrice        DECIMAL(18,2) NOT NULL CHECK (UnitPrice >= 0)
);
GO

CREATE TABLE Payments (
    PaymentId        INT IDENTITY(1,1) CONSTRAINT PK_Payments PRIMARY KEY,
    BookingId        INT NOT NULL CONSTRAINT FK_Payments_Bookings REFERENCES  Bookings(BookingId) ON DELETE CASCADE,
    Amount           DECIMAL(18,2) NOT NULL CHECK (Amount >= 0),
    Method           NVARCHAR(50) NOT NULL,
    PaidAt           DATETIME2(0) NOT NULL DEFAULT(SYSDATETIME()),
    Reference        NVARCHAR(100) NULL
);
GO

CREATE TABLE Housekeeping (
    TaskId           INT IDENTITY(1,1) CONSTRAINT PK_Housekeeping PRIMARY KEY,
    RoomId           INT NOT NULL CONSTRAINT FK_Housekeeping_Rooms REFERENCES  Rooms(RoomId),
    TaskDate         DATE NOT NULL DEFAULT(CAST(GETDATE() AS DATE)),
    StaffName        NVARCHAR(150) NULL,
    Status           NVARCHAR(30) NOT NULL DEFAULT('Pending') CHECK (Status IN ('Pending','InProgress','Done')),
    Notes            NVARCHAR(300) NULL
);
GO

CREATE TABLE Invoices (
    InvoiceId        INT IDENTITY(1,1) CONSTRAINT PK_Invoices PRIMARY KEY,
    BookingId        INT NOT NULL CONSTRAINT FK_Invoices_Bookings REFERENCES  Bookings(BookingId) ON DELETE CASCADE,
    SubtotalRoom     DECIMAL(18,2) NOT NULL CHECK (SubtotalRoom >= 0),
    SubtotalService  DECIMAL(18,2) NOT NULL CHECK (SubtotalService >= 0),
    Tax              DECIMAL(18,2) NOT NULL CHECK (Tax >= 0),
    Discount         DECIMAL(18,2) NOT NULL DEFAULT(0) CHECK (Discount >= 0),
    Total            AS (SubtotalRoom + SubtotalService + Tax - Discount) PERSISTED,
    IssuedAt         DATETIME2(0) NOT NULL DEFAULT(SYSDATETIME())
);
GO

IF NOT EXISTS (SELECT 1 FROM Users)
BEGIN
    INSERT INTO Users (Username, PasswordHash, Role, IsActive)
    VALUES
    (N'admin',   N'hashed_password_here', N'Admin',   1),
    (N'manager', N'hashed_password_here', N'Manager', 1),
    (N'staff1',  N'hashed_password_here', N'Staff',   1);
END
GO

IF NOT EXISTS (SELECT 1 FROM RoomTypes)
BEGIN
    INSERT INTO RoomTypes (Name, Capacity, BasePrice, Description)
    VALUES
    (N'Standard', 2,  500000,  N'Phòng tiêu chuẩn'),
    (N'Deluxe',   3,  800000,  N'Phòng cao cấp'),
    (N'Suite',    4, 1200000,  N'Phòng hạng sang');
END
GO

IF NOT EXISTS (SELECT 1 FROM Rooms)
BEGIN
    INSERT INTO Rooms (RoomNumber, Floor, RoomTypeId, Status, PricePerNight)
    SELECT N'101', 1, rt.RoomTypeId, N'Available', rt.BasePrice FROM RoomTypes rt WHERE rt.Name = N'Standard'
    UNION ALL SELECT N'102', 1, rt.RoomTypeId, N'Available', rt.BasePrice FROM RoomTypes rt WHERE rt.Name = N'Standard'
    UNION ALL SELECT N'201', 2, rt.RoomTypeId, N'Available', rt.BasePrice FROM RoomTypes rt WHERE rt.Name = N'Deluxe'
    UNION ALL SELECT N'202', 2, rt.RoomTypeId, N'Available', rt.BasePrice FROM RoomTypes rt WHERE rt.Name = N'Deluxe'
    UNION ALL SELECT N'301', 3, rt.RoomTypeId, N'Available', rt.BasePrice FROM RoomTypes rt WHERE rt.Name = N'Suite';
END
GO

IF NOT EXISTS (SELECT 1 FROM Services)
BEGIN
    INSERT INTO Services (Name, Price, Unit)
    VALUES
    (N'Giặt ủi',           50000,  N'lần'),
    (N'Ăn sáng',           100000, N'suất'),
    (N'Đưa đón sân bay',   300000, N'lượt'),
    (N'Nước suối',         20000,  N'chai');
END
GO

IF NOT EXISTS (SELECT 1 FROM Customers)
BEGIN
    INSERT INTO Customers (FullName, Phone, Email, IDNumber, Address)
    VALUES
    (N'Nguyễn Văn A', N'0901234567', N'a@example.com', N'012345678', N'Hồ Chí Minh'),
    (N'Trần Thị B',   N'0912345678', N'b@example.com', N'123456789', N'Hà Nội'),
    (N'Lê Văn C',     N'0987654321', N'c@example.com', N'987654321', N'Đà Nẵng');
END
GO

DECLARE @CustomerId INT = (SELECT TOP 1 CustomerId FROM Customers WHERE FullName = N'Nguyễn Văn A');
DECLARE @RoomId INT = (SELECT RoomId FROM Rooms WHERE RoomNumber = N'101');
DECLARE @CheckIn DATE = '2025-11-01';
DECLARE @CheckOut DATE = '2025-11-05';
DECLARE @RatePerNight DECIMAL(18,2) = (SELECT PricePerNight FROM Rooms WHERE RoomId = @RoomId);
DECLARE @BookingId INT;

IF @CustomerId IS NOT NULL AND @RoomId IS NOT NULL
BEGIN
    INSERT INTO Bookings (CustomerId, CheckInDate, CheckOutDate, Status, Notes)
    VALUES (@CustomerId, @CheckIn, @CheckOut, N'Reserved', N'Đặt qua website');
    SET @BookingId = SCOPE_IDENTITY();

    INSERT INTO BookingRooms (BookingId, RoomId, Guests, RatePerNight)
    VALUES (@BookingId, @RoomId, 2, @RatePerNight);

    DECLARE @SvcLaundry INT = (SELECT ServiceId FROM Services WHERE Name = N'Giặt ủi');
    DECLARE @SvcBreakfast INT = (SELECT ServiceId FROM Services WHERE Name = N'Ăn sáng');

    IF @SvcLaundry IS NOT NULL
        INSERT INTO BookingServices (BookingId, ServiceId, Quantity, UnitPrice)
        VALUES (@BookingId, @SvcLaundry, 2, (SELECT Price FROM Services WHERE ServiceId = @SvcLaundry));

    IF @SvcBreakfast IS NOT NULL
        INSERT INTO BookingServices (BookingId, ServiceId, Quantity, UnitPrice)
        VALUES (@BookingId, @SvcBreakfast, 4, (SELECT Price FROM Services WHERE ServiceId = @SvcBreakfast));

    DECLARE @Nights INT = DATEDIFF(DAY, @CheckIn, @CheckOut);
    IF (@Nights < 1) SET @Nights = 1;

    DECLARE @SubtotalRoom DECIMAL(18,2) = (SELECT SUM(@Nights * br.RatePerNight) FROM BookingRooms br WHERE br.BookingId = @BookingId);
    DECLARE @SubtotalService DECIMAL(18,2) = (SELECT COALESCE(SUM(bs.Quantity * bs.UnitPrice),0) FROM BookingServices bs WHERE bs.BookingId = @BookingId);
    DECLARE @Tax DECIMAL(18,2) = ROUND((COALESCE(@SubtotalRoom,0) + COALESCE(@SubtotalService,0)) * 0.10, 0);

    INSERT INTO Invoices (BookingId, SubtotalRoom, SubtotalService, Tax, Discount)
    VALUES (@BookingId, COALESCE(@SubtotalRoom,0), COALESCE(@SubtotalService,0), COALESCE(@Tax,0), 0);

    INSERT INTO Payments (BookingId, Amount, Method, Reference)
    VALUES (@BookingId, 1000000, N'Cash', N'POS-0001');

    INSERT INTO Housekeeping (RoomId, TaskDate, StaffName, Status, Notes)
    VALUES (@RoomId, @CheckOut, N'Ngô Minh', N'Pending', N'Dọn phòng sau khách trả phòng');
END
GO

DECLARE @CustomerId2 INT = (SELECT TOP 1 CustomerId FROM Customers WHERE FullName = N'Trần Thị B');
DECLARE @RoomId2 INT = (SELECT RoomId FROM Rooms WHERE RoomNumber = N'201');
DECLARE @CheckIn2 DATE = '2025-11-10';
DECLARE @CheckOut2 DATE = '2025-11-12';
DECLARE @RatePerNight2 DECIMAL(18,2) = (SELECT PricePerNight FROM Rooms WHERE RoomId = @RoomId2);
DECLARE @BookingId2 INT;

IF @CustomerId2 IS NOT NULL AND @RoomId2 IS NOT NULL
BEGIN
    INSERT INTO Bookings (CustomerId, CheckInDate, CheckOutDate, Status)
    VALUES (@CustomerId2, @CheckIn2, @CheckOut2, N'Reserved');
    SET @BookingId2 = SCOPE_IDENTITY();

    INSERT INTO BookingRooms (BookingId, RoomId, Guests, RatePerNight)
    VALUES (@BookingId2, @RoomId2, 1, @RatePerNight2);

    DECLARE @Nights2 INT = DATEDIFF(DAY, @CheckIn2, @CheckOut2);
    IF (@Nights2 < 1) SET @Nights2 = 1;

    DECLARE @SubtotalRoom2 DECIMAL(18,2) = (SELECT SUM(@Nights2 * br.RatePerNight) FROM BookingRooms br WHERE br.BookingId = @BookingId2);
    DECLARE @Tax2 DECIMAL(18,2) = ROUND(COALESCE(@SubtotalRoom2,0) * 0.10, 0);

    INSERT INTO Invoices (BookingId, SubtotalRoom, SubtotalService, Tax, Discount)
    VALUES (@BookingId2, COALESCE(@SubtotalRoom2,0), 0, COALESCE(@Tax2,0), 0);

    INSERT INTO Payments (BookingId, Amount, Method, Reference)
    VALUES (@BookingId2, COALESCE(@SubtotalRoom2,0) + COALESCE(@Tax2,0), N'Transfer', N'TXN-2025-0002');
END
GO
