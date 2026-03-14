using System;
using System.Collections.Generic;

namespace DesktopAppVendingMachines.Models;

public partial class Sale
{
    public int Id { get; set; }

    public DateTime TimeSale { get; set; }

    public Guid? IdProduct { get; set; }

    public decimal TotalPrice { get; set; }

    public int Quantity { get; set; }

    public int IdPaymentMethod { get; set; }

    public virtual PaymentMethod IdPaymentMethodNavigation { get; set; } = null!;

    public virtual Product? IdProductNavigation { get; set; }
}
