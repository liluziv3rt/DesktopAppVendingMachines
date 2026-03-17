using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopAppVendingMachines.Models;
using Microsoft.EntityFrameworkCore;
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
        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string message;

        [RelayCommand]
        public void Enter()
        {
            // Загружаем пользователя вместе с ролью через правильное навигационное свойство
            var user = db.Users
                .Include(u => u.IdRoleNavigation)  // Важно! Используем IdRoleNavigation, а не Role
                .FirstOrDefault(x => x.Email == Email && x.Password == Password);

            if (user == null)
            {
                Message = "Неверный логин или пароль";
            }
            else
            {
                SessionManager.CurrentUser = user;
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
