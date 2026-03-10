using System;
using System.Collections.Generic;

namespace VendingApp.Models;

public partial class Model
{
    public int Id { get; set; }

    public string Model1 { get; set; } = null!;

    public int IdManufacture { get; set; }

    public virtual Manufacture IdManufactureNavigation { get; set; } = null!;

    public virtual ICollection<VendingMachine> VendingMachines { get; set; } = new List<VendingMachine>();
}
