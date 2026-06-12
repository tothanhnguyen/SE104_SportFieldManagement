#nullable enable
namespace QuanLySan.Services
{
    // Trừu tượng hóa hộp thoại để ViewModel không phụ thuộc trực tiếp vào UI (MessageBox).
    // Nhờ vậy ViewModel chỉ chứa logic trình bày, đúng nguyên tắc MVVM.
    public interface IDialogService
    {
        void ThongBao(string message, string title = "Thông báo");
        void CanhBao(string message, string title = "Cảnh báo");
        void Loi(string message, string title = "Lỗi");
    }
}
