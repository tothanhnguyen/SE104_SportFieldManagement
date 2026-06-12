#nullable enable
using System.Windows;

namespace QuanLySan.Services
{
    // Triển khai IDialogService bằng MessageBox của WPF (lớp duy nhất được phép gọi MessageBox).
    public class DialogService : IDialogService
    {
        public void ThongBao(string message, string title = "Thông báo")
            => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

        public void CanhBao(string message, string title = "Cảnh báo")
            => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

        public void Loi(string message, string title = "Lỗi")
            => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
