using System;
using System.Collections.Generic;

namespace VendingApp.Models;

public partial class MachineDictionary
{
    public Guid IdMachine { get; set; }

    public int? IdValue { get; set; }

    public virtual VendingMachine IdMachineNavigation { get; set; } = null!;

    public virtual Dictionary? IdValueNavigation { get; set; }
}
