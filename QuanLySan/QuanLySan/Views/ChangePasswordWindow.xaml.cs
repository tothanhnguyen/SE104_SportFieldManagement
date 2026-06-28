using System;
using System.Windows;
using QuanLySan.ViewModels;

namespace QuanLySan.Views
{
    public partial class ChangePasswordWindow : Window
    {
        public ChangePasswordWindow()
        {
            InitializeComponent();
            var vm = new ChangePasswordViewModel();
            vm.CloseAction = () => this.Close();
            DataContext = vm;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PasswordBox_OldPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ChangePasswordViewModel vm)
            {
                vm.OldPassword = ((System.Windows.Controls.PasswordBox)sender).Password;
            }
        }

        private void PasswordBox_NewPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ChangePasswordViewModel vm)
            {
                vm.NewPassword = ((System.Windows.Controls.PasswordBox)sender).Password;
            }
        }

        private void PasswordBox_ConfirmNewPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ChangePasswordViewModel vm)
            {
                vm.ConfirmNewPassword = ((System.Windows.Controls.PasswordBox)sender).Password;
            }
        }
    }
}
