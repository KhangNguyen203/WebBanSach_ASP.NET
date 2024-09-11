* Đề tài: quản lý nhà sách
- Ngôn ngữ sử dụng: C# (sử dụng Entity Framework (EF))
- Database sử dụng: SQL server

* Chức năng của hệ thống
- Khách hàng
   + Đăng nhập, đăng xuất, đăng ký
   + Tìm kiếm sách
   + Lọc sản phẩm (Sắp xếp tăng giảm, tìm kiếm) theo giá
   + Xem thông tin đơn hàng
   + Đặt hàng 
   + Thống kê doanh thu
   + Thanh toán online bằng paypal
   + Đánh giá sản phẩm
- Quản trị viên
   + CRUD thể loại, sản phẩm,...
   + Thống kê (Tổng danh thu theo tháng, quý, năm)

* Cách chạy project:
  - Môi trường:
    + Database Manager: Sql server + SSMS 
    + IDE: Visual studio IDE
  - Qui trình:
    + Tạo csdl bằng file database.sql
    +  Vào VS tạo Model kết nối tới DB (dùng ADO.NET Entity Data Model).
    + Chạy local host (Chưa có dữ liệu, cần tạo Admin Account), đăng kí tài khoản mới.
    + Vào sql server sửa userRole của user vừa tạo sửa thành "Admin".
    + Dùng Email, passworld của user vừa sửa đăng nhập vào hệ thống => Admin View.
