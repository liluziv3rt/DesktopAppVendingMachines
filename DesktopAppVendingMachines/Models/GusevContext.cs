using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DesktopAppVendingMachines.Models;

public partial class GusevContext : DbContext
{
    public GusevContext()
    {
    }

    public GusevContext(DbContextOptions<GusevContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Dictionary> Dictionaries { get; set; }

    public virtual DbSet<IssuesFound> IssuesFounds { get; set; }

    public virtual DbSet<MachineDictionary> MachineDictionaries { get; set; }

    public virtual DbSet<Maintenance> Maintenances { get; set; }

    public virtual DbSet<Manufacture> Manufactures { get; set; }

    public virtual DbSet<Model> Models { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VendingMachine> VendingMachines { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=ngknn.ru;Port=5442;Database=Gusev;Username=21P;Password=123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("C");

        modelBuilder.Entity<Dictionary>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("dictionary_pkey");

            entity.ToTable("dictionary", "praktika");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Key)
                .HasColumnType("character varying")
                .HasColumnName("key");
            entity.Property(e => e.Value)
                .HasColumnType("character varying")
                .HasColumnName("value");
        });

        modelBuilder.Entity<IssuesFound>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("issues_found_pkey");

            entity.ToTable("issues_found", "praktika");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Value)
                .HasColumnType("character varying")
                .HasColumnName("value");
        });

        modelBuilder.Entity<MachineDictionary>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("machine_dictionary", "praktika");

            entity.Property(e => e.IdMachine).HasColumnName("id_machine");
            entity.Property(e => e.IdValue).HasColumnName("id_value");

            entity.HasOne(d => d.IdMachineNavigation).WithMany()
                .HasForeignKey(d => d.IdMachine)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("machine_dictionary_id_machine_fkey");

            entity.HasOne(d => d.IdValueNavigation).WithMany()
                .HasForeignKey(d => d.IdValue)
                .HasConstraintName("machine_dictionary_id_value_fkey");
        });

        modelBuilder.Entity<Maintenance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("maintenance_pkey");

            entity.ToTable("maintenance", "praktika");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Date)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date");
            entity.Property(e => e.FullName).HasColumnName("full_name");
            entity.Property(e => e.IdIssuesFound).HasColumnName("id_issues_found");
            entity.Property(e => e.IdVendingMachine).HasColumnName("id_vending_machine");
            entity.Property(e => e.WorkDescription)
                .HasColumnType("character varying")
                .HasColumnName("work_description");

            entity.HasOne(d => d.FullNameNavigation).WithMany(p => p.Maintenances)
                .HasForeignKey(d => d.FullName)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("maintenance_full_name_fkey");

            entity.HasOne(d => d.IdIssuesFoundNavigation).WithMany(p => p.Maintenances)
                .HasForeignKey(d => d.IdIssuesFound)
                .HasConstraintName("maintenance_id_issues_found_fkey");

            entity.HasOne(d => d.IdVendingMachineNavigation).WithMany(p => p.Maintenances)
                .HasForeignKey(d => d.IdVendingMachine)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("maintenance_id_vending_machine_fkey");
        });

        modelBuilder.Entity<Manufacture>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("manufactures_pkey");

            entity.ToTable("manufactures", "praktika");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<Model>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("models_pkey");

            entity.ToTable("models", "praktika");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdManufacture).HasColumnName("id_manufacture");
            entity.Property(e => e.Model1)
                .HasColumnType("character varying")
                .HasColumnName("model");

            entity.HasOne(d => d.IdManufactureNavigation).WithMany(p => p.Models)
                .HasForeignKey(d => d.IdManufacture)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("models_id_manufacture_fkey");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payment_methods_pkey");

            entity.ToTable("payment_methods", "praktika");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title)
                .HasColumnType("character varying")
                .HasColumnName("title");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("products_pkey");

            entity.ToTable("products", "praktika");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Description)
                .HasColumnType("character varying")
                .HasColumnName("description");
            entity.Property(e => e.IdVendingMachine).HasColumnName("id_vending_machine");
            entity.Property(e => e.MinStock).HasColumnName("min_stock");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.QuantityAvailable).HasColumnName("quantity_available");
            entity.Property(e => e.SalesTrend).HasColumnName("sales_trend");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles", "praktika");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sales_pkey");

            entity.ToTable("sales", "praktika");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdPaymentMethod).HasColumnName("id_payment_method");
            entity.Property(e => e.IdProduct).HasColumnName("id_product");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.TimeSale)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("time_sale");
            entity.Property(e => e.TotalPrice).HasColumnName("total_price");

            entity.HasOne(d => d.IdPaymentMethodNavigation).WithMany(p => p.Sales)
                .HasForeignKey(d => d.IdPaymentMethod)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sales_id_payment_method_fkey");

            entity.HasOne(d => d.IdProductNavigation).WithMany(p => p.Sales)
                .HasForeignKey(d => d.IdProduct)
                .HasConstraintName("sales_id_product_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users", "praktika");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Email)
                .HasColumnType("character varying")
                .HasColumnName("email");
            entity.Property(e => e.Family)
                .HasColumnType("character varying")
                .HasColumnName("family");
            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.Images)
                .HasColumnType("character varying")
                .HasColumnName("images");
            entity.Property(e => e.IsEngineer).HasColumnName("is_engineer");
            entity.Property(e => e.IsManager).HasColumnName("is_manager");
            entity.Property(e => e.IsOperator).HasColumnName("is_operator");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasColumnType("character varying")
                .HasColumnName("password");
            entity.Property(e => e.Patronymic)
                .HasColumnType("character varying")
                .HasColumnName("patronymic");
            entity.Property(e => e.Phone)
                .HasColumnType("character varying")
                .HasColumnName("phone");

            entity.HasOne(d => d.IdRoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdRole)
                .HasConstraintName("users_id_role_fkey");
        });

        modelBuilder.Entity<VendingMachine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("vending_machines_pkey");

            entity.ToTable("vending_machines", "praktika");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Coordinates)
                .HasColumnType("character varying")
                .HasColumnName("coordinates");
            entity.Property(e => e.IdEngineer).HasColumnName("id_engineer");
            entity.Property(e => e.IdManager).HasColumnName("id_manager");
            entity.Property(e => e.IdModel).HasColumnName("id_model");
            entity.Property(e => e.IdTechnician).HasColumnName("id_technician");
            entity.Property(e => e.InstallDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("install_date");
            entity.Property(e => e.KitOnlineId)
                .HasColumnType("character varying")
                .HasColumnName("kit_online_id");
            entity.Property(e => e.LastMaintenanceDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_maintenance_date");
            entity.Property(e => e.Location)
                .HasColumnType("character varying")
                .HasColumnName("location");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.RfidCashCollection)
                .HasColumnType("character varying")
                .HasColumnName("rfid_cash_collection");
            entity.Property(e => e.RfidLoading)
                .HasColumnType("character varying")
                .HasColumnName("rfid_loading");
            entity.Property(e => e.RfidService)
                .HasColumnType("character varying")
                .HasColumnName("rfid_service");
            entity.Property(e => e.SerialNumber).HasColumnName("serial_number");
            entity.Property(e => e.TotalIncome).HasColumnName("total_income");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WorkingHours)
                .HasColumnType("character varying")
                .HasColumnName("working_hours");

            entity.HasOne(d => d.IdEngineerNavigation).WithMany(p => p.VendingMachineIdEngineerNavigations)
                .HasForeignKey(d => d.IdEngineer)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("vending_machines_id_engineer_fkey");

            entity.HasOne(d => d.IdManagerNavigation).WithMany(p => p.VendingMachineIdManagerNavigations)
                .HasForeignKey(d => d.IdManager)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("vending_machines_id_manager_fkey");

            entity.HasOne(d => d.IdModelNavigation).WithMany(p => p.VendingMachines)
                .HasForeignKey(d => d.IdModel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("vending_machines_id_model_fkey");

            entity.HasOne(d => d.IdTechnicianNavigation).WithMany(p => p.VendingMachineIdTechnicianNavigations)
                .HasForeignKey(d => d.IdTechnician)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("vending_machines_id_technician_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.VendingMachineUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("vending_machines_user_id_fkey");
        });

        modelBuilder.Entity<MachineDictionary>()
        .HasKey(md => new { md.IdMachine, md.IdValue });

        OnModelCreatingPartial(modelBuilder);
        base.OnModelCreating(modelBuilder);

    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
