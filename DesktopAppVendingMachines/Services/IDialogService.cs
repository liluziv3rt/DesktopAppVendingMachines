using System.Threading.Tasks;

namespace DesktopAppVendingMachines.Services
{
    public interface IDialogService
    {
        Task<bool> ShowConfirmationAsync(string title, string message, string confirm = "Да", string cancel = "Нет");
        Task ShowMessageAsync(string title, string message);
    }

}