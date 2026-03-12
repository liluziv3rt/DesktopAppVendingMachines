using CommunityToolkit.Mvvm.ComponentModel;
using VendingApp.Models;

namespace VendingMachineApp.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {
       public static GusevContext db = new GusevContext();

        public User? currentLogin;
    }
}
