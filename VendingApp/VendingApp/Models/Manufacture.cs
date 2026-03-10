using System;
using System.Collections.Generic;

namespace VendingApp.Models;

public partial class Manufacture
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Model> Models { get; set; } = new List<Model>();
}
