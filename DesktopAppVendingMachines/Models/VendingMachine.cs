using System;
using System.Collections.Generic;

namespace DesktopAppVendingMachines.Models;

public partial class VendingMachine
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid UserId { get; set; }

    public string RfidCashCollection { get; set; } = null!;

    public string Location { get; set; } = null!;

    public string RfidLoading { get; set; } = null!;

    public string KitOnlineId { get; set; } = null!;

    public Guid IdManager { get; set; }

    public string WorkingHours { get; set; } = null!;

    public Guid IdEngineer { get; set; }

    public DateTime InstallDate { get; set; }

    public Guid IdTechnician { get; set; }

    public DateTime LastMaintenanceDate { get; set; }

    public string RfidService { get; set; } = null!;

    public string Coordinates { get; set; } = null!;

    public decimal TotalIncome { get; set; }

    public int SerialNumber { get; set; }

    public int IdModel { get; set; }

    public virtual User IdEngineerNavigation { get; set; } = null!;

    public virtual User IdManagerNavigation { get; set; } = null!;

    public virtual Model IdModelNavigation { get; set; } = null!;

    public virtual User IdTechnicianNavigation { get; set; } = null!;

    public virtual ICollection<Maintenance> Maintenances { get; set; } = new List<Maintenance>();

    public virtual User User { get; set; } = null!;
}
