using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace DesktopAppVendingMachines.ViewModels
{
    internal partial class SignInViewModel : ViewModelBase
    {
        [ObservableProperty] string email;
        [ObservableProperty] string password;
        [ObservableProperty] string message;

        [RelayCommand]
        public void Enter()
        {
            var user = db.Users.FirstOrDefault(x => x.Email == Email && x.Password == Password);
            if (user == null)
            {
                Message = "Неверный логин или пароль";
            }
            else
            {
                currentLogin = user; // это ключевая строка!
                MainWindowViewModel.Instance.PageSwitcher = new MainViewModel();
            }
        }

        [RelayCommand]
        public void GoToSignUp()
        {
            MainWindowViewModel.Instance.PageSwitcher = new SignUpViewModel();
        }

    }
}
