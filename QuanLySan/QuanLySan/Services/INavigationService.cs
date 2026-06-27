namespace QuanLySan.Services
{
    // Trừu tượng hóa việc mở biểu mẫu theo khóa định danh, để ViewModel không phụ thuộc lớp View.
    public interface INavigationService
    {
        void MoBieuMau(string key);
    }
}
