using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using System.Threading.Tasks;

namespace DesktopAppVendingMachines.Services
{
    public class DialogService : IDialogService
    {
        private Window? GetMainWindow()
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                return desktop.MainWindow;
            return null;
        }

        public async Task<bool> ShowConfirmationAsync(string title, string message, string confirm = "Да", string cancel = "Нет")
        {
            var window = GetMainWindow();
            if (window == null) return false;

            var result = false;
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                SystemDecorations = SystemDecorations.BorderOnly,
                Content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(20),
                    Spacing = 20,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                        },
                        new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            Spacing = 10,
                            Children =
                            {
                                new Button
                                {
                                    Content = confirm,
                                    Width = 80,
                                    Tag = true
                                },
                                new Button
                                {
                                    Content = cancel,
                                    Width = 80,
                                    Tag = false
                                }
                            }
                        }
                    }
                }
            };

            // Подписываемся на события кнопок
            foreach (var child in ((StackPanel)((StackPanel)dialog.Content).Children[1]).Children)
            {
                if (child is Button button)
                {
                    button.Click += (s, e) =>
                    {
                        result = (bool)((Button)s!).Tag!;
                        dialog.Close();
                    };
                }
            }

            await dialog.ShowDialog(window);
            return result;
        }

        public async Task ShowMessageAsync(string title, string message)
        {
            var window = GetMainWindow();
            if (window == null) return;

            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                SystemDecorations = SystemDecorations.BorderOnly,
                Content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(20),
                    Spacing = 20,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                        },
                        new Button
                        {
                            Content = "OK",
                            Width = 80,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                        }
                    }
                }
            };

            // Находим кнопку и подписываемся
            var okButton = ((StackPanel)dialog.Content).Children[1] as Button;
            if (okButton != null)
            {
                okButton.Click += (s, e) => dialog.Close();
            }

            await dialog.ShowDialog(window);
        }
    }
}