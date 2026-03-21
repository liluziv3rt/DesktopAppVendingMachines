using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopAppVendingMachines.Models;
using DesktopAppVendingMachines.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DesktopAppVendingMachines.ViewModels
{
    public partial class AddVendingMachineViewModel : ViewModelBase
    {
        // Основные поля (все обязательные)
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
        private string coordinates = ""; // по умолчанию пустая строка

        [ObservableProperty]
        private int serialNumber;

        [ObservableProperty]
        private string workingHours = ""; // по умолчанию пустая строка

        [ObservableProperty]
        private string selectedTimezone;

        [ObservableProperty]
        private string selectedCriticalThresholdTemplate;

        [ObservableProperty]
        private string selectedNotificationTemplate;

        // Пользователи (обязательны к выбору)
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
        private string rfidService = "";

        [ObservableProperty]
        private string rfidCashCollection = "";

        [ObservableProperty]
        private string rfidLoading = "";

        [ObservableProperty]
        private string kitOnlineId = "";

        [ObservableProperty]
        private string selectedServicePriority;

        [ObservableProperty]
        private string selectedModem;

        [ObservableProperty]
        private string notes = "";

        // Коллекции
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

        public AddVendingMachineViewModel()
        {
            LoadReferenceData();
        }

        private void LoadReferenceData()
        {
            // Производители
            Manufactures.Clear();
            foreach (var m in db.Manufactures.OrderBy(x => x.Name))
                Manufactures.Add(m);

            // Модели
            Models.Clear();
            foreach (var m in db.Models.Include(x => x.IdManufactureNavigation).OrderBy(x => x.Model1))
                Models.Add(m);

            // Словари
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

            // Пользователи
            var users = db.Users.ToList();
            Clients.Clear();
            Managers.Clear();
            Engineers.Clear();
            Technicians.Clear();

            foreach (var u in users)
            {
                Clients.Add(u);
                if (u.IsManager == true) Managers.Add(u);
                if (u.IsEngineer == true) Engineers.Add(u);
                if (u.IsOperator == true) Technicians.Add(u);
            }
        }

        private async Task AddMachineStatus(Guid machineId)
        {
            try
            {
                // Добавляем статус "Работает" через SQL
                await db.Database.ExecuteSqlRawAsync(@"
            INSERT INTO praktika.machine_dictionary (id_machine, id_value)
            SELECT {0}, id 
            FROM praktika.dictionary 
            WHERE key = 'status' AND value = 'Работает'",
                    machineId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при добавлении статуса: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (!await ValidateAllFields()) return;

            try
            {
                if (db.VendingMachines.Any(v => v.SerialNumber == SerialNumber))
                {
                    await ShowMessage("Ошибка", $"Автомат с номером {SerialNumber} уже существует");
                    return;
                }

                var machine = new VendingMachine
                {
                    Id = Guid.NewGuid(),
                    Name = Name.Trim(),
                    IdModel = SelectedModel!.Id,
                    Location = Location.Trim(),
                    Coordinates = string.IsNullOrWhiteSpace(Coordinates) ? "" : Coordinates.Trim(),
                    SerialNumber = SerialNumber,
                    WorkingHours = string.IsNullOrWhiteSpace(WorkingHours) ? "" : WorkingHours.Trim(),
                    KitOnlineId = string.IsNullOrWhiteSpace(KitOnlineId) ? "" : KitOnlineId.Trim(),
                    RfidService = string.IsNullOrWhiteSpace(RfidService) ? "" : RfidService.Trim(),
                    RfidCashCollection = string.IsNullOrWhiteSpace(RfidCashCollection) ? "" : RfidCashCollection.Trim(),
                    RfidLoading = string.IsNullOrWhiteSpace(RfidLoading) ? "" : RfidLoading.Trim(),
                    InstallDate = DateTime.Now,
                    LastMaintenanceDate = DateTime.Now,
                    TotalIncome = 0,
                    UserId = SelectedClient!.Id,
                    IdManager = SelectedManager!.Id,
                    IdEngineer = SelectedEngineer!.Id,
                    IdTechnician = SelectedTechnician!.Id
                };

                var existingUserIds = db.Users.Select(u => u.Id).ToHashSet();
                if (!existingUserIds.Contains(machine.UserId))
                    throw new Exception($"UserId {machine.UserId} не существует");
                if (!existingUserIds.Contains(machine.IdManager))
                    throw new Exception($"IdManager {machine.IdManager} не существует");
                if (!existingUserIds.Contains(machine.IdEngineer))
                    throw new Exception($"IdEngineer {machine.IdEngineer} не существует");
                if (!existingUserIds.Contains(machine.IdTechnician))
                    throw new Exception($"IdTechnician {machine.IdTechnician} не существует");

                // Сохраняем машину
                db.VendingMachines.Add(machine);
                await db.SaveChangesAsync();

                // Добавляем статус "Работает"
                await AddMachineStatus(machine.Id);

                // Добавляем остальные параметры
                await AddMachineDictionaries(machine.Id);

                await ShowMessage("Успешно", "Торговый автомат создан");
                NavigationService.GoToVendingMachines();
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex}");
                await ShowMessage("Ошибка", $"Не удалось создать автомат: {inner}");
            }
        }


        private async Task AddMachineDictionaries(Guid machineId)
        {
            try
            {
                var entries = new List<(string key, string value)>();

                void AddDictEntry(string key, string value)
                {
                    if (string.IsNullOrEmpty(value)) return;
                    entries.Add((key, value));
                }

                AddDictEntry("work_mode", SelectedWorkMode);
                AddDictEntry("place", SelectedPlace);
                AddDictEntry("timezone", SelectedTimezone);
                AddDictEntry("critical_threshold_template", SelectedCriticalThresholdTemplate);
                AddDictEntry("notification_template", SelectedNotificationTemplate);
                AddDictEntry("service_priority", SelectedServicePriority);
                AddDictEntry("operator", SelectedModem);

                if (CoinAcceptorEnabled) AddDictEntry("payment_type", "Монетоприемник");
                if (BillAcceptorEnabled) AddDictEntry("payment_type", "Купюроприемник");
                if (CashlessModuleEnabled) AddDictEntry("payment_type", "Модуль б/н оплаты");
                if (QrPaymentsEnabled) AddDictEntry("payment_type", "QR-платежи");

                foreach (var entry in entries)
                {
                    await db.Database.ExecuteSqlRawAsync(@"
                INSERT INTO praktika.machine_dictionary (id_machine, id_value)
                SELECT {0}, id 
                FROM praktika.dictionary 
                WHERE key = {1} AND value = {2}",
                        machineId, entry.key, entry.value);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при добавлении MachineDictionary: {ex.Message}");
                throw;
            }
        }


        private async Task<bool> ValidateAllFields()
        {
            // Основные поля
            if (string.IsNullOrWhiteSpace(Name))
            {
                await ShowMessage("Ошибка", "Поле 'Название ТА' обязательно");
                return false;
            }
            if (SelectedManufacture == null)
            {
                await ShowMessage("Ошибка", "Поле 'Производитель ТА' обязательно");
                return false;
            }
            if (SelectedModel == null)
            {
                await ShowMessage("Ошибка", "Поле 'Модель ТА' обязательно");
                return false;
            }
            if (string.IsNullOrWhiteSpace(SelectedWorkMode))
            {
                await ShowMessage("Ошибка", "Поле 'Режим работы' обязательно");
                return false;
            }
            if (string.IsNullOrWhiteSpace(Location))
            {
                await ShowMessage("Ошибка", "Поле 'Адрес' обязательно");
                return false;
            }
            if (string.IsNullOrWhiteSpace(SelectedPlace))
            {
                await ShowMessage("Ошибка", "Поле 'Место' обязательно");
                return false;
            }
            if (SerialNumber <= 0)  // <-- проверка номера автомата
            {
                await ShowMessage("Ошибка", "Поле 'Номер автомата' обязательно и должно быть положительным числом");
                return false;
            }
            if (string.IsNullOrWhiteSpace(SelectedTimezone))
            {
                await ShowMessage("Ошибка", "Поле 'Часовой пояс' обязательно");
                return false;
            }

            // Пользователи
            if (SelectedClient == null)
            {
                await ShowMessage("Ошибка", "Необходимо выбрать Клиента");
                return false;
            }
            if (SelectedManager == null)
            {
                await ShowMessage("Ошибка", "Необходимо выбрать Менеджера");
                return false;
            }
            if (SelectedEngineer == null)
            {
                await ShowMessage("Ошибка", "Необходимо выбрать Инженера");
                return false;
            }
            if (SelectedTechnician == null)
            {
                await ShowMessage("Ошибка", "Необходимо выбрать Техника-оператора");
                return false;
            }

            // Платежные системы (хотя бы одна)
            if (!CoinAcceptorEnabled && !BillAcceptorEnabled && !CashlessModuleEnabled && !QrPaymentsEnabled)
            {
                await ShowMessage("Ошибка", "Необходимо выбрать хотя бы одну платежную систему");
                return false;
            }

            // Приоритет обслуживания
            if (string.IsNullOrWhiteSpace(SelectedServicePriority))
            {
                await ShowMessage("Ошибка", "Необходимо выбрать приоритет обслуживания");
                return false;
            }

            // Модем
            if (string.IsNullOrWhiteSpace(SelectedModem))
            {
                await ShowMessage("Ошибка", "Необходимо выбрать модем");
                return false;
            }

            // Проверка существования пользователей в БД
            var existingIds = db.Users.Select(u => u.Id).ToHashSet();
            if (!existingIds.Contains(SelectedClient.Id))
            {
                await ShowMessage("Ошибка", "Выбранный клиент не найден в базе данных");
                return false;
            }
            if (!existingIds.Contains(SelectedManager.Id))
            {
                await ShowMessage("Ошибка", "Выбранный менеджер не найден в базе данных");
                return false;
            }
            if (!existingIds.Contains(SelectedEngineer.Id))
            {
                await ShowMessage("Ошибка", "Выбранный инженер не найден в базе данных");
                return false;
            }
            if (!existingIds.Contains(SelectedTechnician.Id))
            {
                await ShowMessage("Ошибка", "Выбранный техник не найден в базе данных");
                return false;
            }

            return true;
        }

        [RelayCommand]
        private void Cancel()
        {
            NavigationService.GoToVendingMachines();
        }
    }
}