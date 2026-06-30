# QUẢN LÝ SÂN THỂ THAO

## Giới Thiệu
Dự án Quản Lý Sân Thể Thao là một ứng dụng Desktop được phát triển nhằm hỗ trợ các trung tâm thể thao quản lý sân bãi, lịch đặt sân, thông tin hội viên và doanh thu một cách chuyên nghiệp, chính xác và hiệu quả. Ứng dụng được thiết kế với giao diện trực quan, dễ sử dụng, đáp ứng đầy đủ các quy trình nghiệp vụ thực tế của một hệ thống cho thuê sân.

## Công Nghệ Sử Dụng
- Ngôn ngữ lập trình: C#
- Nền tảng: .NET 8.0 (WPF - Windows Presentation Foundation)
- Kiến trúc phần mềm: MVVM (Model - View - ViewModel)
- Hệ quản trị cơ sở dữ liệu: SQL Server
- Giao tiếp cơ sở dữ liệu: ADO.NET (Microsoft.Data.SqlClient)
- Giao diện (UI): WPF-UI, FontAwesome.Sharp

## Các Tính Năng Chính
1. Quản Lý Hệ Thống
- Đăng nhập hệ thống phân quyền an toàn.
- Đổi mật khẩu bảo mật tài khoản.

2. Quản Lý Sân Bãi
- Xem tổng quan trạng thái các sân trên Dashboard (Tổng sân, Đang hoạt động, Đang bảo trì).
- Tiếp nhận (thêm mới) sân thể thao vào hệ thống.
- Cập nhật thông tin sân hoặc xóa sân khi cần thiết.
- Phân loại sân theo từng loại hình và trạng thái hoạt động.

3. Quản Lý Đặt Sân
- Đặt sân theo từng khung giờ và loại ngày cụ thể.
- Tự động kiểm tra và ngăn chặn việc đặt trùng khung giờ hoặc đặt sân đang trong trạng thái bảo trì.
- Hỗ trợ chọn nhanh khung giờ trống dựa trên mã chi tiết.
- Thanh toán hóa đơn đặt sân nhanh chóng, tự động tính toán tổng tiền theo thời gian và đơn giá.

4. Quản Lý Hội Viên
- Đăng ký hồ sơ hội viên mới (thông tin cá nhân, số điện thoại, email).
- Tra cứu thông tin hội viên nhanh chóng để hỗ trợ việc đặt sân.

5. Báo Cáo Thống Kê
- Cung cấp số liệu thống kê tổng quan ngay trên màn hình chính (Doanh thu tháng, Số lượng đặt sân hôm nay/tháng này, Biểu đồ đặt sân 7 ngày).
- Lập báo cáo doanh thu chi tiết theo từng sân thể thao.
- Lập báo cáo doanh thu chi tiết theo từng khách hàng / hội viên.

6. Thiết Lập Quy Định
- Cho phép người quản lý thay đổi các quy định chung của hệ thống (ví dụ: các loại sân, loại ngày, đơn giá, tình trạng sân...).

## Cấu Trúc Mã Nguồn
- /Models: Chứa các lớp thực thể (Entity) đại diện cho các bảng trong cơ sở dữ liệu và lớp DatabaseConfig lưu trữ cấu hình chuỗi kết nối.
- /ViewModels: Chứa logic nghiệp vụ xử lý dữ liệu và điều khiển giao diện theo chuẩn MVVM, kết nối giữa View và dữ liệu (Repository).
- /Views: Chứa mã XAML định nghĩa giao diện người dùng (Các Window, Page, UserControl).
- /Services & /Data: Chứa các lớp Repository chuyên thực hiện các truy vấn SQL trực tiếp xuống cơ sở dữ liệu, đảm bảo việc thao tác dữ liệu được tách biệt rõ ràng.
- /Utils: Các tiện ích hỗ trợ, quản lý phiên làm việc của người dùng (AppSession).

## Hướng Dẫn Cài Đặt Và Chạy Ứng Dụng
1. Yêu cầu hệ thống:
- Máy tính cài đặt hệ điều hành Windows.
- Cài đặt .NET 8.0 SDK hoặc Runtime.
- Cài đặt Visual Studio 2022 (khuyến nghị).
- Cài đặt SQL Server.

2. Cài đặt Cơ Sở Dữ Liệu:
- Phục hồi (Restore) hoặc chạy script khởi tạo cơ sở dữ liệu "QLSanTheThao" trên SQL Server của bạn.

3. Cấu hình kết nối:
- Mở thư mục dự án, tìm đến file /Models/DatabaseConfig.cs.
- Thay đổi biến ConnectionString sao cho phù hợp với thông tin kết nối SQL Server tại máy cục bộ (Data Source, Initial Catalog, cấu hình bảo mật...).

4. Khởi chạy:
- Mở file QuanLySan.sln bằng Visual Studio.
- Nhấn Rebuild Solution để hệ thống tải xuống các thư viện cần thiết (NuGet packages).
- Nhấn Start (F5) để bắt đầu chạy ứng dụng.
