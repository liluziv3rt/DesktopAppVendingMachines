using System;
using System.Collections.Generic;

namespace VendingApp.Models;

public partial class PaymentMethod
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
