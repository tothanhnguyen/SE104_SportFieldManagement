using System;
using System.Windows.Input;
using System.Windows;
using QuanLySan.ViewModels.Base;
using QuanLySan.Data;
using QuanLySan.Utils;

namespace QuanLySan.ViewModels
{
    public class ChangePasswordViewModel : BaseViewModel
    {
        private readonly AuthRepository _repo = new AuthRepository();

        public Action? CloseAction { get; set; }

        private string _oldPassword = "";
        public string OldPassword
        {
            get => _oldPassword;
            set { _oldPassword = value; OnPropertyChanged(); }
        }

        private string _newPassword = "";
        public string NewPassword
        {
            get => _newPassword;
            set { _newPassword = value; OnPropertyChanged(); }
        }

        private string _confirmNewPassword = "";
        public string ConfirmNewPassword
        {
            get => _confirmNewPassword;
            set { _confirmNewPassword = value; OnPropertyChanged(); }
        }

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand SubmitCommand { get; }

        public ChangePasswordViewModel()
        {
            SubmitCommand = new RelayCommand(_ =>
            {
                ErrorMessage = "";
                if (string.IsNullOrWhiteSpace(OldPassword) || string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmNewPassword))
                {
                    ErrorMessage = "Vui lòng nhập đầy đủ thông tin.";
                    return;
                }

                if (NewPassword != ConfirmNewPassword)
                {
                    ErrorMessage = "Mật khẩu xác nhận không khớp.";
                    return;
                }

                if (NewPassword.Length < 4)
                {
                    ErrorMessage = "Mật khẩu mới phải có ít nhất 4 ký tự.";
                    return;
                }

                bool success = _repo.ChangePassword(AppSession.CurrentUsername, OldPassword, NewPassword);
                if (success)
                {
                    MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseAction?.Invoke();
                }
                else
                {
                    ErrorMessage = "Mật khẩu cũ không chính xác.";
                }
            });
        }
    }
}
