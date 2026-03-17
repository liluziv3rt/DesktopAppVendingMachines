using DesktopAppVendingMachines.Models;

namespace DesktopAppVendingMachines.Models
{
    public static class SessionManager
    {
        public static User? CurrentUser { get; set; }

        public static void ClearSession()
        {
            CurrentUser = null;
        }

        public static bool IsAuthenticated => CurrentUser != null;
    }

}