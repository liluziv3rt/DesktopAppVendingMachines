using DesktopAppVendingMachines.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopAppVendingMachines.Services
{
    public static class NavigationService
    {
        public static MainViewModel? MainVM { get; set; }

        public static void GoToVendingMachines()
        {
            MainVM?.NavigateToVendingMachines();
        }

        public static void GoToEditMachine(Guid id)
        {
            MainVM?.NavigateToEditMachine(id);
        }

        public static void GoToAddMachine()
        {
            MainVM?.NavigateToAddMachine();
        }

        public static void GoToCompanies()
        {
            MainVM?.NavigateToCompanies();
        }

        public static void GoToAddCompany()
        {
            MainVM?.NavigateToAddCompany();
        }

        public static void GoToEditCompany(int id)
        {
            MainVM?.NavigateToEditCompany(id);
        }
    }
}
