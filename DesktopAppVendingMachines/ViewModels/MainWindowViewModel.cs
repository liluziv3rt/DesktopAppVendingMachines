using CommunityToolkit.Mvvm.ComponentModel;

namespace DesktopAppVendingMachines.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty] ViewModelBase pageSwitcher = new SignInViewModel();
        public static MainWindowViewModel Instance { get; set; }

        public MainWindowViewModel()
        {
            Instance = this;
        }
    }
}
