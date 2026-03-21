using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesktopAppVendingMachines.Models;
using DesktopAppVendingMachines.ViewModels;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;


namespace Tests
{
    public class AuthTests : IDisposable
    {
        private readonly GusevContext _context;
        private readonly SignInViewModel _signInViewModel;
        private readonly SignUpViewModel _signUpViewModel;

        public AuthTests()
        {
            // Используем InMemory базу данных для тестов
            var options = new DbContextOptionsBuilder<GusevContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GusevContext(options);

            _signInViewModel = new SignInViewModel();
            _signUpViewModel = new SignUpViewModel();

            // Подменяем контекст базы данных
            typeof(ViewModelBase).GetProperty("db")?.SetValue(_signInViewModel, _context);
            typeof(ViewModelBase).GetProperty("db")?.SetValue(_signUpViewModel, _context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region SignIn Tests (5 тестов)

        [Fact]
        public void SignIn_ValidCredentials_ShouldSucceed()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                Password = "Password123!",
                Name = "Test",
                Family = "User",
                Patronymic = "Testovich"
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            _signInViewModel.Email = "test@test.com";
            _signInViewModel.Password = "Password123!";

            // Act
            _signInViewModel.Enter();

            // Assert
            Assert.Null(_signInViewModel.Message);
        }

        [Fact]
        public void SignIn_InvalidEmail_ShouldShowError()
        {
            // Arrange
            _signInViewModel.Email = "wrong@test.com";
            _signInViewModel.Password = "Password123!";

            // Act
            _signInViewModel.Enter();

            // Assert
            Assert.Equal("Неверный логин или пароль", _signInViewModel.Message);
        }

        [Fact]
        public void SignIn_InvalidPassword_ShouldShowError()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                Password = "Password123!",
                Name = "Test",
                Family = "User",
                Patronymic = "Testovich"
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            _signInViewModel.Email = "test@test.com";
            _signInViewModel.Password = "WrongPassword";

            // Act
            _signInViewModel.Enter();

            // Assert
            Assert.Equal("Неверный логин или пароль", _signInViewModel.Message);
        }

        [Fact]
        public void SignIn_EmptyEmail_ShouldShowError()
        {
            // Arrange
            _signInViewModel.Email = "";
            _signInViewModel.Password = "Password123!";

            // Act
            _signInViewModel.Enter();

            // Assert
            Assert.Equal("Неверный логин или пароль", _signInViewModel.Message);
        }

        [Fact]
        public void SignIn_EmptyPassword_ShouldShowError()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                Password = "Password123!",
                Name = "Test",
                Family = "User",
                Patronymic = "Testovich"
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            _signInViewModel.Email = "test@test.com";
            _signInViewModel.Password = "";

            // Act
            _signInViewModel.Enter();

            // Assert
            Assert.Equal("Неверный логин или пароль", _signInViewModel.Message);
        }

        #endregion

        #region SignUp Tests (10 тестов)

        [Fact]
        public async Task Register_ValidData_ShouldSucceed()
        {
            // Arrange
            _signUpViewModel.Email = "newuser@test.com";
            _signUpViewModel.Password = "Password123!";
            _signUpViewModel.ConfirmPassword = "Password123!";
            _signUpViewModel.Surname = "Иванов";
            _signUpViewModel.Firstname = "Иван";
            _signUpViewModel.Patronymic = "Иванович";

            // Отправляем код и устанавливаем правильный код
            _signUpViewModel.CaptchaAnswer = "0";
            await _signUpViewModel.SendVerificationCode();
            _signUpViewModel.VerificationCode = _signUpViewModel.GeneratedCode;

            // Act
            await _signUpViewModel.Register();

            // Assert
            Assert.Empty(_signUpViewModel.Message);
            var user = _context.Users.FirstOrDefault(u => u.Email == "newuser@test.com");
            Assert.NotNull(user);
            Assert.Equal("Иван", user.Name);
            Assert.Equal("Иванов", user.Family);
        }

        [Fact]
        public async Task Register_EmailAlreadyExists_ShouldShowError()
        {
            // Arrange
            var existingUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "existing@test.com",
                Password = "Password123!",
                Name = "Existing",
                Family = "User",
                Patronymic = "Testovich"
            };
            _context.Users.Add(existingUser);
            _context.SaveChanges();

            _signUpViewModel.Email = "existing@test.com";
            _signUpViewModel.Password = "Password123!";
            _signUpViewModel.ConfirmPassword = "Password123!";
            _signUpViewModel.Surname = "Иванов";
            _signUpViewModel.Firstname = "Иван";
            _signUpViewModel.Patronymic = "Иванович";

            _signUpViewModel.CaptchaAnswer = "0";
            await _signUpViewModel.SendVerificationCode();
            _signUpViewModel.VerificationCode = _signUpViewModel.GeneratedCode;

            // Act
            await _signUpViewModel.Register();

            // Assert
            Assert.Equal("Пользователь с таким email уже существует", _signUpViewModel.Message);
        }

        [Fact]
        public async Task Register_PasswordTooShort_ShouldShowError()
        {
            // Arrange
            _signUpViewModel.Email = "test@test.com";
            _signUpViewModel.Password = "short";
            _signUpViewModel.ConfirmPassword = "short";
            _signUpViewModel.Surname = "Иванов";
            _signUpViewModel.Firstname = "Иван";
            _signUpViewModel.Patronymic = "Иванович";

            // Act
            await _signUpViewModel.Register();

            // Assert
            Assert.Contains("минимум 8 символов", _signUpViewModel.Message);
        }

        [Fact]
        public async Task Register_PasswordNoDigit_ShouldShowError()
        {
            // Arrange
            _signUpViewModel.Email = "test@test.com";
            _signUpViewModel.Password = "Password!";
            _signUpViewModel.ConfirmPassword = "Password!";
            _signUpViewModel.Surname = "Иванов";
            _signUpViewModel.Firstname = "Иван";
            _signUpViewModel.Patronymic = "Иванович";

            // Act
            await _signUpViewModel.Register();

            // Assert
            Assert.Contains("хотя бы одну цифру", _signUpViewModel.Message);
        }

        [Fact]
        public async Task Register_PasswordNoSpecialChar_ShouldShowError()
        {
            // Arrange
            _signUpViewModel.Email = "test@test.com";
            _signUpViewModel.Password = "Password123";
            _signUpViewModel.ConfirmPassword = "Password123";
            _signUpViewModel.Surname = "Иванов";
            _signUpViewModel.Firstname = "Иван";
            _signUpViewModel.Patronymic = "Иванович";

            // Act
            await _signUpViewModel.Register();

            // Assert
            Assert.Contains("спецсимвол", _signUpViewModel.Message);
        }

        [Fact]
        public async Task Register_PasswordNoUppercase_ShouldShowError()
        {
            // Arrange
            _signUpViewModel.Email = "test@test.com";
            _signUpViewModel.Password = "password123!";
            _signUpViewModel.ConfirmPassword = "password123!";
            _signUpViewModel.Surname = "Иванов";
            _signUpViewModel.Firstname = "Иван";
            _signUpViewModel.Patronymic = "Иванович";

            // Act
            await _signUpViewModel.Register();

            // Assert
            Assert.Contains("заглавную букву", _signUpViewModel.Message);
        }

        [Fact]
        public async Task Register_PasswordNoLowercase_ShouldShowError()
        {
            // Arrange
            _signUpViewModel.Email = "test@test.com";
            _signUpViewModel.Password = "PASSWORD123!";
            _signUpViewModel.ConfirmPassword = "PASSWORD123!";
            _signUpViewModel.Surname = "Иванов";
            _signUpViewModel.Firstname = "Иван";
            _signUpViewModel.Patronymic = "Иванович";

            // Act
            await _signUpViewModel.Register();

            // Assert
            Assert.Contains("строчную букву", _signUpViewModel.Message);
        }

        [Fact]
        public async Task Register_PasswordsDoNotMatch_ShouldShowError()
        {
            // Arrange
            _signUpViewModel.Email = "test@test.com";
            _signUpViewModel.Password = "Password123!";
            _signUpViewModel.ConfirmPassword = "Different123!";
            _signUpViewModel.Surname = "Иванов";
            _signUpViewModel.Firstname = "Иван";
            _signUpViewModel.Patronymic = "Иванович";

            // Act
            await _signUpViewModel.Register();

            // Assert
            Assert.Equal("Пароли не совпадают", _signUpViewModel.Message);
        }

        [Fact]
        public async Task Register_InvalidEmail_ShouldShowError()
        {
            // Arrange
            _signUpViewModel.Email = "invalid-email";
            _signUpViewModel.Password = "Password123!";
            _signUpViewModel.ConfirmPassword = "Password123!";
            _signUpViewModel.Surname = "Иванов";
            _signUpViewModel.Firstname = "Иван";
            _signUpViewModel.Patronymic = "Иванович";

            // Act
            await _signUpViewModel.Register();

            // Assert
            Assert.Equal("Введите корректный email", _signUpViewModel.Message);
        }

        [Fact]
        public async Task Register_WrongVerificationCode_ShouldShowError()
        {
            // Arrange
            _signUpViewModel.Email = "test@test.com";
            _signUpViewModel.Password = "Password123!";
            _signUpViewModel.ConfirmPassword = "Password123!";
            _signUpViewModel.Surname = "Иванов";
            _signUpViewModel.Firstname = "Иван";
            _signUpViewModel.Patronymic = "Иванович";

            _signUpViewModel.CaptchaAnswer = "0";
            await _signUpViewModel.SendVerificationCode();
            _signUpViewModel.VerificationCode = "000000"; // Неправильный код

            // Act
            await _signUpViewModel.Register();

            // Assert
            Assert.Equal("Неверный код подтверждения", _signUpViewModel.Message);
        }

        [Fact]
        public async Task Register_MissingFields_ShouldShowError()
        {
            // Arrange
            _signUpViewModel.Email = "test@test.com";
            _signUpViewModel.Password = "Password123!";
            _signUpViewModel.ConfirmPassword = "Password123!";
            _signUpViewModel.Surname = ""; // Пустое поле
            _signUpViewModel.Firstname = "Иван";
            _signUpViewModel.Patronymic = "Иванович";

            // Act
            await _signUpViewModel.Register();

            // Assert
            Assert.Equal("Все поля должны быть заполнены", _signUpViewModel.Message);
        }

        #endregion

        #region CAPTCHA Tests (3 теста)

        [Fact]
        public void Captcha_CorrectAnswer_ShouldBeValid()
        {
            // Arrange
            _signUpViewModel.CaptchaAnswer = "0";

            // Assert
            Assert.True(_signUpViewModel.IsCaptchaValid);
        }

        [Fact]
        public void Captcha_WrongAnswer_ShouldBeInvalid()
        {
            // Arrange
            _signUpViewModel.CaptchaAnswer = "5";

            // Assert
            Assert.False(_signUpViewModel.IsCaptchaValid);
        }

        [Fact]
        public void Captcha_EmptyAnswer_ShouldBeInvalid()
        {
            // Arrange
            _signUpViewModel.CaptchaAnswer = "";

            // Assert
            Assert.False(_signUpViewModel.IsCaptchaValid);
        }

        #endregion

        #region Verification Code Tests (2 теста)

        [Fact]
        public async Task SendVerificationCode_WithoutCaptcha_ShouldShowError()
        {
            // Arrange
            _signUpViewModel.Email = "test@test.com";
            _signUpViewModel.CaptchaAnswer = "wrong";

            // Act
            await _signUpViewModel.SendVerificationCode();

            // Assert
            Assert.Equal("Сначала решите математический пример", _signUpViewModel.Message);
            Assert.False(_signUpViewModel.IsVerificationCodeSent);
        }

        [Fact]
        public async Task SendVerificationCode_WithValidCaptcha_ShouldGenerateCode()
        {
            // Arrange
            _signUpViewModel.Email = "test@test.com";
            _signUpViewModel.CaptchaAnswer = "0";

            // Act
            await _signUpViewModel.SendVerificationCode();

            // Assert
            Assert.True(_signUpViewModel.IsVerificationCodeSent);
            Assert.NotNull(_signUpViewModel.GeneratedCode);
            Assert.Equal(6, _signUpViewModel.GeneratedCode.Length);
        }

        #endregion
    }
}

