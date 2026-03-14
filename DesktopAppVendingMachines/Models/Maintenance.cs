using System;
using System.Collections.Generic;

namespace DesktopAppVendingMachines.Models;

public partial class Maintenance
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public int? IdIssuesFound { get; set; }

    public Guid IdVendingMachine { get; set; }

    public Guid FullName { get; set; }

    public string WorkDescription { get; set; } = null!;

    public virtual User FullNameNavigation { get; set; } = null!;

    public virtual IssuesFound? IdIssuesFoundNavigation { get; set; }

    public virtual VendingMachine IdVendingMachineNavigation { get; set; } = null!;
}
