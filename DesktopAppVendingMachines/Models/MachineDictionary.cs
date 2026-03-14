using System;
using System.Collections.Generic;

namespace DesktopAppVendingMachines.Models;

public partial class MachineDictionary
{
    public Guid IdMachine { get; set; }

    public int? IdValue { get; set; }

    public virtual VendingMachine IdMachineNavigation { get; set; } = null!;

    public virtual Dictionary? IdValueNavigation { get; set; }
}
