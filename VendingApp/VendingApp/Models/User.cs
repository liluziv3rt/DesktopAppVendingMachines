using System;
using System.Collections.Generic;

namespace VendingApp.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Family { get; set; } = null!;

    public string Patronymic { get; set; } = null!;

    public bool IsManager { get; set; }

    public bool IsEngineer { get; set; }

    public string Phone { get; set; } = null!;

    public bool IsOperator { get; set; }

    public int IdRole { get; set; }

    public string Images { get; set; } = null!;

    public string? Password { get; set; }

    public virtual Role IdRoleNavigation { get; set; } = null!;

    public virtual ICollection<Maintenance> Maintenances { get; set; } = new List<Maintenance>();

    public virtual ICollection<VendingMachine> VendingMachineIdEngineerNavigations { get; set; } = new List<VendingMachine>();

    public virtual ICollection<VendingMachine> VendingMachineIdManagerNavigations { get; set; } = new List<VendingMachine>();

    public virtual ICollection<VendingMachine> VendingMachineIdTechnicianNavigations { get; set; } = new List<VendingMachine>();

    public virtual ICollection<VendingMachine> VendingMachineUsers { get; set; } = new List<VendingMachine>();
}
