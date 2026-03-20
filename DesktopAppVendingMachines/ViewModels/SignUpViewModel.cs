using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopAppVendingMachines.Models;
using DesktopAppVendingMachines.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DesktopAppVendingMachines.ViewModels
{
    public partial class SignUpViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string confirmPassword;

        [ObservableProperty]
        private string verificationCode;

        [ObservableProperty]
        private string franchiseCode;

        [ObservableProperty]
        private string message;

        [ObservableProperty]
        private string surname;

        [ObservableProperty]
        private string firstname;

        [ObservableProperty]
        private string patronymic;

        [ObservableProperty]
        private string captchaQuestion;

        [ObservableProperty]
        private string captchaAnswer;

        [ObservableProperty]
        private bool showPassword;

        [ObservableProperty]
        private string generatedCode;

        [ObservableProperty]
        private bool showVerificationField;

        [ObservableProperty]
        private bool isVerificationCodeSent;

        [ObservableProperty]
        private bool isCaptchaValid;

        private Random _random = new Random();

        public SignUpViewModel()
        {
            GenerateCaptcha();
            ShowVerificationField = false;
            IsVerificationCodeSent = false;
            IsCaptchaValid = false;
        }

        partial void OnCaptchaAnswerChanged(string value)
        {
            // Проверяем CAPTCHA при каждом изменении поля
            IsCaptchaValid = ValidateCaptcha(value);
        }

        [RelayCommand]
        private void ToggleShowPassword()
        {
            ShowPassword = !ShowPassword;
        }

        [RelayCommand]
        private async Task SendVerificationCode()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                Message = "Введите email для отправки кода подтверждения";
                return;
            }

            if (!IsValidEmail(Email))
            {
                Message = "Введите корректный email";
                return;
            }

            // Проверка CAPTCHA перед отправкой кода
            if (!IsCaptchaValid)
            {
                Message = "Сначала решите математический пример";
                return;
            }

            // Генерируем 6-значный код
            GeneratedCode = _random.Next(100000, 999999).ToString();

            // Показываем код в модальном окне
            await ShowMessageDialog("Код подтверждения", $"Ваш код подтверждения: {GeneratedCode}\n\nВведите этот код в поле ниже для завершения регистрации.");

            ShowVerificationField = true;
            IsVerificationCodeSent = true;
            Message = "Код подтверждения отправлен на указанный email";
        }

        private async Task ShowMessageDialog(string title, string message)
        {
            // Используем существующий метод из ViewModelBase
            await ShowMessage(title, message);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPassword(string password)
        {
            // Пароль должен содержать минимум 8 символов
            if (password.Length < 8)
                return false;

            // Должен содержать хотя бы одну цифру
            if (!password.Any(char.IsDigit))
                return false;

            // Должен содержать хотя бы один спецсимвол
            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                return false;

            // Должен содержать хотя бы одну заглавную букву
            if (!password.Any(char.IsUpper))
                return false;

            // Должен содержать хотя бы одну строчную букву
            if (!password.Any(char.IsLower))
                return false;

            return true;
        }

        private void GenerateCaptcha()
        {
            // Простой пример: 2 + 2 - 2 * 2 = ?
            CaptchaQuestion = "2 + 2 - 2 × 2 = ?";
        }

        private bool ValidateCaptcha(string userAnswer)
        {
            if (string.IsNullOrWhiteSpace(userAnswer))
                return false;

            // Правильный ответ: 0
            return userAnswer.Trim() == "0";
        }

        [RelayCommand]
        private async Task Register()
        {
            // Очищаем предыдущее сообщение
            Message = "";

            // Проверка всех полей
            if (string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword) ||
                string.IsNullOrWhiteSpace(Surname) ||
                string.IsNullOrWhiteSpace(Firstname) ||
                string.IsNullOrWhiteSpace(Patronymic))
            {
                Message = "Все поля должны быть заполнены";
                return;
            }

            // Проверка email
            if (!IsValidEmail(Email))
            {
                Message = "Введите корректный email";
                return;
            }

            // Проверка пароля
            if (!IsValidPassword(Password))
            {
                Message = "Пароль должен содержать минимум 8 символов, включая:\n- хотя бы одну цифру\n- хотя бы один спецсимвол\n- хотя бы одну заглавную букву\n- хотя бы одну строчную букву";
                return;
            }

            // Проверка совпадения паролей
            if (Password != ConfirmPassword)
            {
                Message = "Пароли не совпадают";
                return;
            }

            // Проверка кода подтверждения
            if (!IsVerificationCodeSent)
            {
                Message = "Сначала отправьте код подтверждения на email";
                return;
            }

            if (string.IsNullOrWhiteSpace(VerificationCode) || VerificationCode != GeneratedCode)
            {
                Message = "Неверный код подтверждения";
                return;
            }

            // Проверка CAPTCHA
            if (!IsCaptchaValid)
            {
                Message = "Неверный ответ на CAPTCHA";
                return;
            }

            // Проверка существующего пользователя
            var existingUser = db.Users.FirstOrDefault(x => x.Email == Email);
            if (existingUser != null)
            {
                Message = "Пользователь с таким email уже существует";
                return;
            }

            try
            {
                // Создание нового пользователя
                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = Email,
                    Password = Password,
                    Name = Firstname,
                    Family = Surname,
                    Patronymic = Patronymic,
                    IsManager = false,
                    IsEngineer = false,
                    IsOperator = false
                };

                db.Users.Add(newUser);
                await db.SaveChangesAsync();

                await ShowMessage("Успешно", "Регистрация успешно завершена!");

                // Переход на страницу входа
                MainWindowViewModel.Instance.PageSwitcher = new SignInViewModel();
            }
            catch (Exception ex)
            {
                Message = $"Ошибка при регистрации: {ex.Message}";
            }
        }

        [RelayCommand]
        private void GoToSignIn()
        {
            MainWindowViewModel.Instance.PageSwitcher = new SignInViewModel();
        }

    }
}