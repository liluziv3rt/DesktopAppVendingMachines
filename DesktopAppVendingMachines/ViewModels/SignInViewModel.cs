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
            currentLogin = db.Users.FirstOrDefault(x => x.Email == email && x.Password == password);

            if (currentLogin == null)

            {
                Message = "Пользователь отсутсвует";
            }

            else
            {
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
