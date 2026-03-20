using System;
using System.Collections.Generic;

namespace DesktopAppVendingMachines.Models;

public partial class Company
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? IdParentCompany { get; set; }

    public string? Adress { get; set; }

    public string? Contacts { get; set; }

    public DateOnly? CreatedDate { get; set; }

    public virtual Company? IdParentCompanyNavigation { get; set; }

    public virtual ICollection<Company> InverseIdParentCompanyNavigation { get; set; } = new List<Company>();
}
