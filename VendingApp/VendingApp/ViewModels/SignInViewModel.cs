using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingApp.ViewModels
{
    internal partial class SignInViewModel: ViewModelBase
    {
        [ObservableProperty] string login;

        [ObservableProperty] string password;

        [ObservableProperty] string message;

        public void Enter()
        {
            currentUser = db.Users.FirstOrDefault(x => x.Email == Login && x.Password == Password);

            if (currentUser == null)
            {
                Message = "Пользователь отсутсвует";
            }

            else
            {
                MainWindowViewModel.Instance.PageSwitcher = new MainViewModel();
            }
        }
    }
}
