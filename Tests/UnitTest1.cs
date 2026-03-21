using DesktopAppVendingMachines.Models;
using DesktopAppVendingMachines.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    public class UnitTest1
    {
        private readonly SignUpViewModel _viewModel;

        public UnitTest1()
        {
            _viewModel = new SignUpViewModel();
        }

        [Fact]
        public void Constructor_SetsCaptchaQuestion()
        {
            // Assert
            Assert.NotNull(_viewModel.CaptchaQuestion);
            Assert.NotEmpty(_viewModel.CaptchaQuestion);
        }

        [Fact]
        public void Constructor_ShowVerificationFieldIsFalse()
        {
            // Assert
            Assert.False(_viewModel.ShowVerificationField);
        }

        [Fact]
        public void Constructor_IsVerificationCodeSentIsFalse()
        {
            // Assert
            Assert.False(_viewModel.IsVerificationCodeSent);
        }

        [Fact]
        public void Constructor_IsCaptchaValidIsFalse()
        {
            // Assert
            Assert.False(_viewModel.IsCaptchaValid);
        }

        [Fact]
        public void ToggleShowPassword_TogglesValue()
        {
            // Arrange
            bool initialValue = _viewModel.ShowPassword;

            // Act
            _viewModel.ToggleShowPasswordCommand.Execute(null);

            // Assert
            Assert.Equal(!initialValue, _viewModel.ShowPassword);
        }

        [Fact]
        public void CaptchaAnswer_CorrectAnswer_IsCaptchaValidTrue()
        {
            // Act
            _viewModel.CaptchaAnswer = "0";

            // Assert
            Assert.True(_viewModel.IsCaptchaValid);
        }

        [Fact]
        public void CaptchaAnswer_WrongAnswer_IsCaptchaValidFalse()
        {
            // Act
            _viewModel.CaptchaAnswer = "5";

            // Assert
            Assert.False(_viewModel.IsCaptchaValid);
        }

        [Fact]
        public void CaptchaAnswer_EmptyAnswer_IsCaptchaValidFalse()
        {
            // Act
            _viewModel.CaptchaAnswer = "";

            // Assert
            Assert.False(_viewModel.IsCaptchaValid);
        }

        [Fact]
        public async Task SendVerificationCode_EmptyEmail_ShowsMessage()
        {
            // Arrange
            _viewModel.Email = "";
            _viewModel.CaptchaAnswer = "0";

            // Act
            await _viewModel.SendVerificationCodeCommand.ExecuteAsync(null);

            // Assert
            Assert.Contains("email", _viewModel.Message);
        }

        [Fact]
        public async Task SendVerificationCode_InvalidEmail_ShowsMessage()
        {
            // Arrange
            _viewModel.Email = "invalid";
            _viewModel.CaptchaAnswer = "0";

            // Act
            await _viewModel.SendVerificationCodeCommand.ExecuteAsync(null);

            // Assert
            Assert.Contains("корректный email", _viewModel.Message);
        }

        [Fact]
        public async Task SendVerificationCode_WithoutCaptcha_ShowsMessage()
        {
            // Arrange
            _viewModel.Email = "test@test.com";
            _viewModel.CaptchaAnswer = "";

            // Act
            await _viewModel.SendVerificationCodeCommand.ExecuteAsync(null);

            // Assert
            Assert.Contains("решите", _viewModel.Message);
        }

        [Fact]
        public async Task Register_EmptyFields_ShowsMessage()
        {
            // Arrange
            _viewModel.Email = "";
            _viewModel.Password = "";
            _viewModel.ConfirmPassword = "";
            _viewModel.Surname = "";
            _viewModel.Firstname = "";
            _viewModel.Patronymic = "";

            // Act
            await _viewModel.RegisterCommand.ExecuteAsync(null);

            // Assert
            Assert.Contains("поля должны быть заполнены", _viewModel.Message);
        }

        [Fact]
        public async Task Register_PasswordsDoNotMatch_ShowsMessage()
        {
            // Arrange
            _viewModel.Password = "Password123!";
            _viewModel.ConfirmPassword = "Different123!";

            // Act
            await _viewModel.RegisterCommand.ExecuteAsync(null);

            // Assert
            Assert.Contains("Все поля должны быть заполнены", _viewModel.Message);
        }

        [Fact]
        public async Task Register_WithoutVerificationCode_ShowsMessage()
        {
            // Arrange
            _viewModel.Email = "test@test.com";
            _viewModel.Password = "Password123!";
            _viewModel.ConfirmPassword = "Password123!";
            _viewModel.Surname = "Иванов";
            _viewModel.Firstname = "Иван";
            _viewModel.Patronymic = "Иванович";
            _viewModel.IsVerificationCodeSent = false;

            // Act
            await _viewModel.RegisterCommand.ExecuteAsync(null);

            // Assert
            Assert.Contains("отправьте код", _viewModel.Message);
        }

        [Fact]
        public void Patronymic_ShouldBeSettable()
        {
            // Act
            _viewModel.Patronymic = "Иванович";

            // Assert
            Assert.Equal("Иванович", _viewModel.Patronymic);
        }

        [Fact]
        public void Surname_ShouldBeSettable()
        {
            // Act
            _viewModel.Surname = "Иванов";

            // Assert
            Assert.Equal("Иванов", _viewModel.Surname);
        }
    }
}

