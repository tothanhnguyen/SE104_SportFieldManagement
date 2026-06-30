using System.Windows;
using QuanLySan.ViewModels;

namespace QuanLySan.Views
{
    public partial class ThayDoiQuyDinhWindow : Window
    {
        public ThayDoiQuyDinhWindow()
        {
            InitializeComponent();
            var vm = new ThayDoiQuyDinhViewModel();
            DataContext = vm;
            
            // Theo dõi khi DanhSachLoaiHoiVien có item mới để scroll và focus
            vm.DanhSachLoaiHoiVien.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems != null && e.NewItems.Count > 0)
                {
                    var newItem = e.NewItems[0];
                    
                    // Dispatcher để đảm bảo UI đã render xong row mới
                    Dispatcher.InvokeAsync(() =>
                    {
                        dgLoaiHoiVien.SelectedItem = newItem;
                        dgLoaiHoiVien.ScrollIntoView(newItem);
                        
                        // Focus vào ô đầu tiên có thể edit (Tên hạng)
                        dgLoaiHoiVien.CurrentCell = new System.Windows.Controls.DataGridCellInfo(newItem, dgLoaiHoiVien.Columns[1]);
                        dgLoaiHoiVien.Focus();
                        dgLoaiHoiVien.BeginEdit();
                    }, System.Windows.Threading.DispatcherPriority.Background);
                }
            };
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
