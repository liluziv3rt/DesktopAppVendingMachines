using CommunityToolkit.Mvvm.ComponentModel;
using DesktopAppVendingMachines.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace DesktopAppVendingMachines.ViewModels
{
    internal partial class SignUpViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string confirmPassword;

        [ObservableProperty]
        private string message;

        [ObservableProperty]
        private string surname;

        [ObservableProperty]
        private string firstname;

        [ObservableProperty]
        private string patronymic;


        public void Register()
        {
            if (string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(Surname) ||
                string.IsNullOrWhiteSpace(Firstname) ||
                string.IsNullOrWhiteSpace(Patronymic))
            {
                Message = "Все поля должны быть заполнены";
                return;
            }

            if (Password != ConfirmPassword)
            {
                Message = "Пароли не совпадают";
                return;
            }

            if (!Email.Contains('@'))
            {
                Message = "Введите корректный email (должен содержать '@')";
                return;
            }

            if (Password.Length < 3)
            {
                Message = "Пароль должен содержать минимум 3 символа";
                return;
            }

            var existingUser = db.Users.FirstOrDefault(x => x.Email == Email);
            if (existingUser != null)
            {
                Message = "Пользователь с таким email уже существует";
                return;
            }

            var newUser = new User
            {
                Email = Email,
                Password = Password,
                Name = Firstname,
                Family = Surname,
                Patronymic = Patronymic,
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            MainWindowViewModel.Instance.PageSwitcher = new SignInViewModel();
        }

        public void GoToSignIn()
        {
            MainWindowViewModel.Instance.PageSwitcher = new SignInViewModel();
        }
    }
}
