-----------------------------Tables-----------------------------
CREATE TABLE [User] (
  [UserID] int PRIMARY KEY IDENTITY(1, 1),
  [FullName] nvarchar(50),
  [Email] nvarchar(50),
  [PhoneNumber] nvarchar(50),
  [UserName] nvarchar(50),
  [Password] nvarchar(50),
  [UserRole] nvarchar(10)
)
GO

CREATE TABLE [Category] (
  [CategoryID] int PRIMARY KEY IDENTITY(1, 1),
  [CategoryName] nvarchar(50)
)
GO

CREATE TABLE [Product] (
  [ProductID] int PRIMARY KEY IDENTITY(1, 1),
  [ProductName] nvarchar(100),
  [Price] float,
  [Image] nvarchar(500),
  [Description] nvarchar(500),
  [CategoryID] int,
  [Author] nvarchar(100)
)
GO

CREATE TABLE [Orders] (
  [OrderID] int PRIMARY KEY IDENTITY(1, 1),
  [OrderDate] date,
  [UserID] int,
  [Status] nvarchar(20)
)
GO

CREATE TABLE [OrderDetails] (
  [ID] int PRIMARY KEY IDENTITY(1, 1),
  [OrderID] int,
  [ProductID] int,
  [UnitPrice] float,
  [Quantity] int
)
GO

CREATE TABLE [Comments] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [Content] nvarchar(500),
  [CreatedAt] datetime,
  [UserId] int,
  [ProductId] int
)
GO

ALTER TABLE [Comments] ADD FOREIGN KEY ([UserId]) REFERENCES [User] ([UserID])
GO

ALTER TABLE [Comments] ADD FOREIGN KEY ([ProductId]) REFERENCES [Product] ([ProductID])
GO

ALTER TABLE [Orders] ADD FOREIGN KEY ([UserID]) REFERENCES [User] ([UserID])
GO

ALTER TABLE [Product] ADD FOREIGN KEY ([CategoryID]) REFERENCES [Category] ([CategoryID])
GO

ALTER TABLE [OrderDetails] ADD FOREIGN KEY ([OrderID]) REFERENCES [Orders] ([OrderID])
GO

ALTER TABLE [OrderDetails] ADD FOREIGN KEY ([ProductID]) REFERENCES [Product] ([ProductID])
GO
-----------------------------End Tables-------------------------------


-----------------------------Stored Procedure-------------------------
--ThongKeSoLuongSanPhamTheoThang
CREATE PROCEDURE [dbo].[spThongKeSoLuongSanPhamTheoThang]
    @thang INT = NULL
AS
BEGIN
    SELECT p.ProductName, SUM(od.Quantity) AS 'soLuong'
    FROM Orders o
    JOIN OrderDetails od ON o.OrderID = od.OrderID
    JOIN Product p ON p.ProductID = od.ProductID
    WHERE (@thang IS NULL OR MONTH(o.OrderDate) = @thang)
    GROUP BY p.ProductName
END

--ThongKeDoanhThuTheoNam
CREATE PROCEDURE [dbo].[spThongKeDoanhThuTheoNam]
    @year INT = NULL
AS
BEGIN
    SELECT p.ProductName, SUM(od.Quantity * od.UnitPrice) AS 'TongTien'
    FROM Orders o
    JOIN OrderDetails od ON o.OrderID = od.OrderID
    JOIN Product p ON p.ProductID = od.ProductID
    WHERE (@year IS NULL OR YEAR(o.OrderDate) = @year)
    GROUP BY p.ProductName
END

--ThongKeDoanhThuTheoThang
CREATE PROCEDURE [dbo].[ThongKeDoanhThuTheoThang]
AS
BEGIN
    SELECT MONTH(o.OrderDate) AS 'thang', 
           SUM(od.UnitPrice * od.Quantity) AS 'TongTien'
    FROM Orders o
    JOIN OrderDetails od ON o.OrderID = od.OrderID
    GROUP BY MONTH(o.OrderDate)
END

--ThongKeDoanhThuTheLoaiTheoNam
create PROCEDURE [dbo].[spThongKeDoanhThuTheLoaiTheoNam] @year int = NULL
as begin 
SELECT c.CategoryName as 'CategoryName', SUM(od.Quantity*od.UnitPrice) as 'TongTien'
    FROM Orders o
    JOIN OrderDetails od ON o.OrderID = od.OrderID
    JOIN Product p ON p.ProductID = od.ProductID
	join category c on c.CategoryID = p.CategoryID
    WHERE (@year IS NULL OR YEAR(o.OrderDate) = @year)
	GROUP BY c.CategoryID,c.CategoryName
end

--ThongKeSoLuongSachTheoTheLoai
create proc [dbo].[ThongKeSoLuongSachTheoTheLoai]
as begin
select count(p.CategoryID) 
	from category c join product p 
	on c.CategoryID = p.CategoryID
	group by p.CategoryID
end
-----------------------------End Stored Procedure-------------------------
