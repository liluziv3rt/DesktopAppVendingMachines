using CommunityToolkit.Mvvm.ComponentModel;
using DesktopAppVendingMachines.Models;
using DesktopAppVendingMachines.Services;
using System.Threading.Tasks;

namespace DesktopAppVendingMachines.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {
        public static GusevContext db = new GusevContext();
        public User? currentLogin;

        protected async Task<bool> ShowConfirmationDialog(string title, string message, string confirm, string cancel)
        {
            // Используем статическое свойство DialogService из App
            return await App.DialogService.ShowConfirmationAsync(title, message, confirm, cancel);
        }

        protected async Task ShowMessage(string title, string message)
        {
            await App.DialogService.ShowMessageAsync(title, message);
        }
    }

}
