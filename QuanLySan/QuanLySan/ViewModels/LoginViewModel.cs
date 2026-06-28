using System;
using System.Windows.Input;
using System.Windows;
using QuanLySan.ViewModels.Base;
using QuanLySan.Data;
using QuanLySan.Utils;

namespace QuanLySan.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthRepository _repo = new AuthRepository();

        public Action? CloseAction { get; set; }

        private bool _isLoginMode = true;
        public bool IsLoginMode
        {
            get => _isLoginMode;
            set { _isLoginMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsRegisterMode)); }
        }

        public bool IsRegisterMode => !IsLoginMode;

        private string _username = "";
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _password = "";
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        private string _email = "";
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand ToggleModeCommand { get; }
        public ICommand SubmitCommand { get; }

        public LoginViewModel()
        {
            ToggleModeCommand = new RelayCommand(_ =>
            {
                IsLoginMode = !IsLoginMode;
                ErrorMessage = "";
            });

            SubmitCommand = new RelayCommand(_ =>
            {
                ErrorMessage = "";
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
                    return;
                }

                if (IsLoginMode)
                {
                    var result = _repo.Login(Username, Password);
                    if (result != null)
                    {
                        AppSession.CurrentAccountId = result.Value.AccountId;
                        AppSession.CurrentUsername = result.Value.Username;
                        AppSession.CurrentEmail = result.Value.Email;
                        CloseAction?.Invoke();
                    }
                    else
                    {
                        ErrorMessage = "Sai tên đăng nhập hoặc mật khẩu.";
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(Email))
                    {
                        ErrorMessage = "Vui lòng nhập email.";
                        return;
                    }

                    bool ok = _repo.Register(Username, Password, Email);
                    if (ok)
                    {
                        MessageBox.Show("Đăng ký thành công! Vui lòng đăng nhập.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        IsLoginMode = true;
                        Password = "";
                    }
                    else
                    {
                        ErrorMessage = "Tên đăng nhập đã tồn tại!";
                    }
                }
            });
        }
    }
}
