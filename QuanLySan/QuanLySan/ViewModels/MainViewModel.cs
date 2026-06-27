#nullable enable
using System.Windows.Input;
using QuanLySan.Services;
using QuanLySan.ViewModels.Base;

namespace QuanLySan.ViewModels
{
    // Menu chính: phát lệnh mở biểu mẫu theo khóa (CommandParameter từ View).
    public class MainViewModel : BaseViewModel
    {
        private readonly INavigationService _nav;

        public ICommand MoBieuMauCommand { get; }

        public MainViewModel() : this(new NavigationService()) { }

        public MainViewModel(INavigationService nav)
        {
            _nav = nav;
            MoBieuMauCommand = new RelayCommand(p =>
            {
                if (p is string key) _nav.MoBieuMau(key);
            });
        }
    }
}
