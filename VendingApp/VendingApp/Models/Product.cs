using System;
using System.Collections.Generic;

namespace VendingApp.Models;

public partial class Product
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public int MinStock { get; set; }

    public Guid IdVendingMachine { get; set; }

    public string Description { get; set; } = null!;

    public int QuantityAvailable { get; set; }

    public decimal SalesTrend { get; set; }

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
