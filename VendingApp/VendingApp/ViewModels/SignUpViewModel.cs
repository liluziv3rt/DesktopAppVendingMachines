using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;
using VendingApp.Models;

namespace VendingApp.ViewModels
{
    internal partial class SignUpViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string? email;

        [ObservableProperty]
        private string? password;

        [ObservableProperty]
        private string? confirmPassword;

        [ObservableProperty]
        private string? message;

        public void Register()
        {
            // Валидация полей
            if (string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                Message = "Все поля должны быть заполнены";
                return;
            }

            if (Password != ConfirmPassword)
            {
                Message = "Пароли не совпадают";
                return;
            }

            // Проверка на существующего пользователя
            var existingUser = db.Users.FirstOrDefault(x => x.Email == Email);
            if (existingUser != null)
            {
                Message = "Пользователь с таким email уже существует";
                return;
            }

            // Создание нового пользователя
            var newUser = new User
            {
                Email = Email,
                Password = Password // В реальном проекте пароль должен хешироваться!
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            // Возврат на страницу авторизации после успешной регистрации
            MainWindowViewModel.Instance.PageSwitcher = new SignInViewModel();
        }

        public void GoToSignIn()
        {
            MainWindowViewModel.Instance.PageSwitcher = new SignInViewModel();
        }
    }
}
