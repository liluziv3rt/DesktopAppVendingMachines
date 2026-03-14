using CommunityToolkit.Mvvm.ComponentModel;
using DesktopAppVendingMachines.Models;

namespace DesktopAppVendingMachines.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {
        public static GusevContext db = new GusevContext();

        public User? currentLogin;
    }
}
