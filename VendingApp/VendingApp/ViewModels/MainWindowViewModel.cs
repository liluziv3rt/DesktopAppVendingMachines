using CommunityToolkit.Mvvm.ComponentModel;

namespace VendingMachineApp.ViewModels
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
