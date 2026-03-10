using System;
using System.Collections.Generic;

namespace VendingApp.Models;

public partial class IssuesFound
{
    public int Id { get; set; }

    public string Value { get; set; } = null!;

    public virtual ICollection<Maintenance> Maintenances { get; set; } = new List<Maintenance>();
}
