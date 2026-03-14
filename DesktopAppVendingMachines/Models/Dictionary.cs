using System;
using System.Collections.Generic;

namespace DesktopAppVendingMachines.Models;

public partial class Dictionary
{
    public int Id { get; set; }

    public string Key { get; set; } = null!;

    public string Value { get; set; } = null!;
}
