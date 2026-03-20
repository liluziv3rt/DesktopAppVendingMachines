using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DesktopAppVendingMachines.Models;
using DesktopAppVendingMachines.Services;

namespace DesktopAppVendingMachines.ViewModels
{
    public partial class EditVendingMachineViewModel : ViewModelBase
    {
        private readonly Guid _machineId;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private Manufacture selectedManufacture;

        [ObservableProperty]
        private Model selectedModel;

        [ObservableProperty]
        private string selectedWorkMode;

        [ObservableProperty]
        private string location;

        [ObservableProperty]
        private string selectedPlace; 

        [ObservableProperty]
        private string coordinates;

        [ObservableProperty]
        private int serialNumber;

        [ObservableProperty]
        private string workingHours;

        [ObservableProperty]
        private string selectedTimezone;

        [ObservableProperty]
        private string selectedCriticalThresholdTemplate;

        [ObservableProperty]
        private string selectedNotificationTemplate;

        [ObservableProperty]
        private User selectedClient; 

        [ObservableProperty]
        private User selectedManager;  

        [ObservableProperty]
        private User selectedEngineer; 

        [ObservableProperty]
        private User selectedTechnician; 

        [ObservableProperty]
        private bool coinAcceptorEnabled;

        [ObservableProperty]
        private bool billAcceptorEnabled;

        [ObservableProperty]
        private bool cashlessModuleEnabled;

        [ObservableProperty]
        private bool qrPaymentsEnabled;

        [ObservableProperty]
        private string rfidService;

        [ObservableProperty]
        private string rfidCashCollection;

        [ObservableProperty]
        private string rfidLoading;

        [ObservableProperty]
        private string kitOnlineId;

        [ObservableProperty]
        private string selectedServicePriority;

        [ObservableProperty]
        private string selectedModem;

        [ObservableProperty]
        private string notes;

        public ObservableCollection<Manufacture> Manufactures { get; } = new();
        public ObservableCollection<Model> Models { get; } = new();
        public ObservableCollection<string> WorkModes { get; } = new();
        public ObservableCollection<string> Timezones { get; } = new();
        public ObservableCollection<string> Places { get; } = new(); 
        public ObservableCollection<string> CriticalThresholdTemplates { get; } = new();
        public ObservableCollection<string> NotificationTemplates { get; } = new();
        public ObservableCollection<User> Clients { get; } = new(); 
        public ObservableCollection<User> Managers { get; } = new(); 
        public ObservableCollection<User> Engineers { get; } = new(); 
        public ObservableCollection<User> Technicians { get; } = new(); 
        public ObservableCollection<string> ServicePriorities { get; } = new();
        public ObservableCollection<string> Modems { get; } = new();

        public EditVendingMachineViewModel(Guid machineId)
        {
            _machineId = machineId;
            LoadData();
        }

        private void LoadData()
        {
            LoadReferenceData();

            var machine = db.VendingMachines
                .Include(v => v.IdModelNavigation)
                    .ThenInclude(m => m.IdManufactureNavigation)
                .Include(v => v.User)
                .Include(v => v.IdManagerNavigation)
                .Include(v => v.IdEngineerNavigation)
                .Include(v => v.IdTechnicianNavigation)
                .FirstOrDefault(v => v.Id == _machineId);

            if (machine == null) return;

            Name = machine.Name;
            Location = machine.Location;
            Coordinates = machine.Coordinates;
            SerialNumber = machine.SerialNumber;
            WorkingHours = machine.WorkingHours;
            KitOnlineId = machine.KitOnlineId;
            RfidService = machine.RfidService;
            RfidCashCollection = machine.RfidCashCollection;
            RfidLoading = machine.RfidLoading;

            SelectedManufacture = machine.IdModelNavigation?.IdManufactureNavigation;
            SelectedModel = machine.IdModelNavigation;
            SelectedClient = machine.User;
            SelectedManager = machine.IdManagerNavigation;
            SelectedEngineer = machine.IdEngineerNavigation;
            SelectedTechnician = machine.IdTechnicianNavigation;

            var machineDicts = db.MachineDictionaries
                .Include(md => md.IdValueNavigation)
                .Where(md => md.IdMachine == _machineId)
                .ToList();



            foreach (var md in machineDicts)
            {
                if (md.IdValueNavigation == null) continue;
                switch (md.IdValueNavigation.Key.ToLower())
                {
                    case "work_mode":
                        SelectedWorkMode = md.IdValueNavigation.Value;
                        break;
                    case "place":
                        SelectedPlace = md.IdValueNavigation.Value; 
                        break;
                    case "timezone":
                        SelectedTimezone = md.IdValueNavigation.Value;
                        break;
                    case "critical_threshold_template":
                        SelectedCriticalThresholdTemplate = md.IdValueNavigation.Value;
                        break;
                    case "notification_template":
                        SelectedNotificationTemplate = md.IdValueNavigation.Value;
                        break;
                    case "service_priority":
                        SelectedServicePriority = md.IdValueNavigation.Value;
                        break;
                    case "operator":
                        SelectedModem = md.IdValueNavigation.Value;
                        break;
                }
            }

            var paymentValues = machineDicts
                .Where(md => md.IdValueNavigation?.Key?.ToLower() == "payment_type")
                .Select(md => md.IdValueNavigation.Value)
                .ToList();

            CoinAcceptorEnabled = paymentValues.Contains("Монетоприемник");
            BillAcceptorEnabled = paymentValues.Contains("Купюроприемник");
            CashlessModuleEnabled = paymentValues.Contains("Модуль б/н оплаты");
            QrPaymentsEnabled = paymentValues.Contains("QR-платежи");
        }

        private void LoadReferenceData()
        {
            Manufactures.Clear();
            foreach (var m in db.Manufactures.OrderBy(x => x.Name))
                Manufactures.Add(m);

            Models.Clear();
            foreach (var m in db.Models.Include(x => x.IdManufactureNavigation).OrderBy(x => x.Model1))
                Models.Add(m);

            var dicts = db.Dictionaries.ToList();
            foreach (var d in dicts)
            {
                switch (d.Key.ToLower())
                {
                    case "work_mode":
                        WorkModes.Add(d.Value);
                        break;
                    case "place":
                        Places.Add(d.Value); 
                        break;
                    case "timezone":
                        Timezones.Add(d.Value);
                        break;
                    case "critical_threshold_template":
                        CriticalThresholdTemplates.Add(d.Value);
                        break;
                    case "notification_template":
                        NotificationTemplates.Add(d.Value);
                        break;
                    case "service_priority":
                        ServicePriorities.Add(d.Value);
                        break;
                    case "operator":
                        Modems.Add(d.Value);
                        break;
                }
            }

            var users = db.Users.ToList();

            Clients.Clear();
            Managers.Clear();
            Engineers.Clear();
            Technicians.Clear();

            foreach (var u in users)
            {
                Clients.Add(u);

                if (u.IsManager == true)
                    Managers.Add(u);

                if (u.IsEngineer == true)
                    Engineers.Add(u);

                if (u.IsOperator == true)
                    Technicians.Add(u);
            }
        }

        private string GetFullName(User user)
        {
            if (user == null) return "";
            return $"{user.Family} {user.Name} {user.Patronymic}".Trim();
        }

        [RelayCommand]
        private void Save()
        {
            var machine = db.VendingMachines.FirstOrDefault(v => v.Id == _machineId);
            if (machine == null) return;

            machine.Name = Name;
            machine.IdModel = SelectedModel?.Id ?? 0;
            machine.Location = Location;
            machine.Coordinates = Coordinates;
            machine.SerialNumber = SerialNumber;
            machine.WorkingHours = WorkingHours;
            machine.KitOnlineId = KitOnlineId;
            machine.RfidService = RfidService;
            machine.RfidCashCollection = RfidCashCollection;
            machine.RfidLoading = RfidLoading;
            machine.UserId = SelectedClient?.Id ?? Guid.Empty;
            machine.IdManager = SelectedManager?.Id ?? Guid.Empty;
            machine.IdEngineer = SelectedEngineer?.Id ?? Guid.Empty;
            machine.IdTechnician = SelectedTechnician?.Id ?? Guid.Empty;

            var oldDicts = db.MachineDictionaries.Where(md => md.IdMachine == _machineId).ToList();
            db.MachineDictionaries.RemoveRange(oldDicts);

            void AddDictEntry(string key, string value)
            {
                if (string.IsNullOrEmpty(value)) return;
                var dictEntry = db.Dictionaries.FirstOrDefault(d => d.Key == key && d.Value == value);
                if (dictEntry != null)
                {
                    db.MachineDictionaries.Add(new MachineDictionary
                    {
                        IdMachine = machine.Id,
                        IdValue = dictEntry.Id
                    });
                }
            }

            AddDictEntry("work_mode", SelectedWorkMode);
            AddDictEntry("place", SelectedPlace); 
            AddDictEntry("timezone", SelectedTimezone);
            AddDictEntry("critical_threshold_template", SelectedCriticalThresholdTemplate);
            AddDictEntry("notification_template", SelectedNotificationTemplate);
            AddDictEntry("service_priority", SelectedServicePriority);
            AddDictEntry("operator", SelectedModem);

            if (CoinAcceptorEnabled)
                AddDictEntry("payment_type", "Монетоприемник");
            if (BillAcceptorEnabled)
                AddDictEntry("payment_type", "Купюроприемник");
            if (CashlessModuleEnabled)
                AddDictEntry("payment_type", "Модуль б/н оплаты");
            if (QrPaymentsEnabled)
                AddDictEntry("payment_type", "QR-платежи");

            db.SaveChanges();

            NavigationService.GoToVendingMachines();
        }

        [RelayCommand]
        private void Cancel()
        {
            NavigationService.GoToVendingMachines();
        }
    }
}