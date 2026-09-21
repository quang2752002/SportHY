# Project Directives & Rules

## Autonomous Command & Build Execution
- **Tự động chạy lệnh không cần hỏi**: Khi cần thực thi bất kỳ lệnh CMD, PowerShell, terminal, script kiểm tra, kiểm tra database, tải package, hoặc lệnh build (`dotnet build`, `dotnet run`, `npm run build`, `npm run lint`,...), Agent luôn **chủ động tự động chạy ngay lập tức** mà **không cần hỏi ý kiến hay chờ người dùng xác nhận**.
- **Chủ động debug và sửa lỗi**: Khi một lệnh build hoặc script gặp lỗi, Agent tự động phân tích output, xác định nguyên nhân và tiến hành chỉnh sửa mã nguồn rồi chạy lại để kiểm chứng kết quả.
- **Báo cáo kết quả trực tiếp**: Sau khi thực hiện xong các bước và lệnh cần thiết, tóm tắt trực tiếp kết quả và giải pháp cho người dùng.

## Service Method Commenting Rule
- **Bắt buộc comment chức năng hàm trong Service**: Khi viết mới hoặc chỉnh sửa bất kỳ phương thức/hàm nào trong tầng Service (`Dms.Application/Services` và `Interfaces`), **luôn luôn viết comment XML doc (`/// <summary>...`) mô tả chi tiết chức năng**, tham số truyền vào và kết quả trả về. Không được để hàm trống comment.

## Thin Controller Rule (Tuyệt đối không viết logic vào Controller)
- **Không viết Business Logic trong Controller**: Toàn bộ nghiệp vụ tính toán, kiểm tra hợp lệ, xử lý logic, phân quyền chi tiết, và truy vấn dữ liệu từ `IUnitOfWork` / `DbContext` / `Repository` **TUYỆT ĐỐI KHÔNG** được viết trực tiếp trong Controller.
- **Vai trò của Controller**: Controller chỉ đóng vai trò tiếp nhận Request từ client, kiểm tra `ModelState` cơ bản, gọi Service tương ứng ở tầng `Dms.Application`, và trả về View / JSON / Redirect. Mọi logic nghiệp vụ phải được định nghĩa và thực thi tại tầng Service (`Dms.Application/Services` và `Interfaces`).

## Client-Side AJAX Rendering Rule
- **Hạn chế dùng ViewBag / Server-side Model binding trực tiếp trên View**: Tránh nhúng dữ liệu nặng qua `ViewBag` hay render tĩnh bằng Razor trên View.
- **Dùng AJAX tải dữ liệu**: Thiết kế View đóng vai trò làm khung giao diện (UI skeleton/shell). Dùng AJAX (`fetch` hoặc `$.ajax`) gọi các Action trả về `JsonResult` ở Controller để lấy dữ liệu động và render ra DOM.

## Soft Delete Directive (Chỉ xóa mềm trong toàn hệ thống)
- **Tuyệt đối không xóa cứng (Hard Delete)**: Mọi thao tác xóa dữ liệu (Giải đấu, Môn thể thao, VĐV, Đơn vị, Trận đấu, Phân công trọng tài, Hồ sơ đăng ký,...) trong toàn hệ thống **CHỈ ĐƯỢC XÓA MỀM (Soft Delete)** bằng cách gán `IsDeleted = true`, `LastModified = DateTime.UtcNow`. Không được xóa vật lý bản ghi khỏi database.
- **Bảo vệ tự động tại tầng DbContext & Repository**: Trong `ApplicationDbContext.SaveChangesAsync()` và `GenericRepository<T>`, mọi lệnh Delete/Remove đối với các entity kế thừa `BaseEntity` đều phải tự động chuyển thành cập nhật `IsDeleted = true`.
- **Truy vấn an toàn**: Mọi câu lệnh truy vấn dữ liệu phải luôn lọc loại bỏ bản ghi đã xóa mềm (`IsDeleted != true`).

