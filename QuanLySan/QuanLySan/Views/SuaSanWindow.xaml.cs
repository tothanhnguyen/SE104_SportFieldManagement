using System.Windows;
using System.Windows.Input;
using QuanLySan.ViewModels;

namespace QuanLySan.Views
{
    public partial class SuaSanWindow : Window
    {
        public SuaSanWindow(string maSan)
        {
            InitializeComponent();
            DataContext = new SuaSanViewModel(maSan, this);
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DgGioSan_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            // Triggers property change for DataGrid when editing is finished
        }
    }
}
