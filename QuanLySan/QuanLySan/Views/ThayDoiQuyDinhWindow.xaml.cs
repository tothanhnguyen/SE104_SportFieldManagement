using System.Windows;
using QuanLySan.ViewModels;

namespace QuanLySan.Views
{
    public partial class ThayDoiQuyDinhWindow : Window
    {
        public ThayDoiQuyDinhWindow()
        {
            InitializeComponent();
            DataContext = new ThayDoiQuyDinhViewModel();
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
