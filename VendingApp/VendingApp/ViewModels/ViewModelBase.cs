using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using VendingApp.Models;

namespace VendingApp.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {
        public static GusevContext db = new GusevContext();

        public User? currentUser;
    }
}
