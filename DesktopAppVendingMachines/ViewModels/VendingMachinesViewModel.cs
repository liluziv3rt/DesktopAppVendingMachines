using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopAppVendingMachines.Models;
using DesktopAppVendingMachines.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DesktopAppVendingMachines.ViewModels
{
    public partial class VendingMachinesViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<VendingMachineViewModel> vendingMachines = new();

        [ObservableProperty]
        private int totalCount;

        [ObservableProperty]
        private int pageSize = 50;

        [ObservableProperty]
        private int currentPage = 1;

        [ObservableProperty]
        private string searchText = "";

        [ObservableProperty]
        private bool isLoading;

        public List<int> PageSizes { get; } = new() { 10, 25, 50, 100 };

        // Кэш для словарей
        private Dictionary<string, Dictionary<int, string>> _dictionaryCache = new();

        // Кэш для связей машин со словарями
        private Dictionary<Guid, List<MachineDictionary>> _machineDictionaryCache = new();

        private readonly Action<Guid> _onEditRequest;

        public VendingMachinesViewModel(Action<Guid> onEditRequest = null)
        {
            _onEditRequest = onEditRequest;
            System.Diagnostics.Debug.WriteLine($"VendingMachinesViewModel created: _onEditRequest is {(_onEditRequest == null ? "null" : "not null")}");

            LoadDictionaryCache();
            LoadMachineDictionaryCache();
            LoadVendingMachines();
        }

        private void LoadDictionaryCache()
        {
            try
            {
                var allDictionaries = db.Dictionaries.ToList();

                _dictionaryCache = allDictionaries
                    .GroupBy(d => d.Key)
                    .ToDictionary(
                        g => g.Key,
                        g => g.ToDictionary(d => d.Id, d => d.Value)
                    );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки словарей: {ex.Message}");
            }
        }

        private void LoadMachineDictionaryCache()
        {
            try
            {
                var allMachineDictionaries = db.MachineDictionaries
                    .Include(md => md.IdValueNavigation)
                    .ToList();

                _machineDictionaryCache = allMachineDictionaries
                    .GroupBy(md => md.IdMachine)
                    .ToDictionary(
                        g => g.Key,
                        g => g.ToList()
                    );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки связей машин: {ex.Message}");
            }
        }

        private void LoadVendingMachines()
        {
            try
            {
                IsLoading = true;

                var query = db.VendingMachines
                    .Include(v => v.IdModelNavigation)
                        .ThenInclude(m => m.IdManufactureNavigation)
                    .Include(v => v.User)
                    .Include(v => v.IdManagerNavigation)
                    .Include(v => v.IdEngineerNavigation)
                    .Include(v => v.IdTechnicianNavigation)
                    .AsQueryable();

                // Поиск по названию или адресу
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    query = query.Where(v =>
                        v.Name.Contains(SearchText) ||
                        v.Location.Contains(SearchText));
                }

                TotalCount = query.Count();

                // Пагинация
                var machines = query
                    .OrderBy(v => v.Name)
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                VendingMachines.Clear();
                foreach (var machine in machines)
                {
                    // Получаем словари для этой машины из кэша
                    var machineDictionaries = _machineDictionaryCache.ContainsKey(machine.Id)
                        ? _machineDictionaryCache[machine.Id]
                        : new List<MachineDictionary>();

                    VendingMachines.Add(new VendingMachineViewModel(machine, machineDictionaries, _dictionaryCache));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки автоматов: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void Search()
        {
            CurrentPage = 1;
            LoadVendingMachines();
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = "";
            CurrentPage = 1;
            LoadVendingMachines();
        }

        [RelayCommand]
        private void NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                LoadVendingMachines();
            }
        }

        [RelayCommand]
        private void PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                LoadVendingMachines();
            }
        }
        partial void OnPageSizeChanged(int value)
        {
            CurrentPage = 1;          // сброс на первую страницу
            LoadVendingMachines();    // перезагрузка данных
        }

        [RelayCommand]
        private void EditMachine(Guid id)
        {
            NavigationService.GoToEditMachine(id);
        }

        [RelayCommand]
        private void DeleteMachine(Guid id)
        {
            // TODO: Подтверждение и удаление
        }

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public string PageInfo => $"Запись с {((CurrentPage - 1) * PageSize) + 1} до {Math.Min(CurrentPage * PageSize, TotalCount)} из {TotalCount} записей";
    }

    public class VendingMachineViewModel
    {
        private readonly VendingMachine _machine;
        private readonly List<MachineDictionary> _machineDictionaries;
        private readonly Dictionary<string, Dictionary<int, string>> _dictionaryCache;

        public VendingMachineViewModel(
            VendingMachine machine,
            List<MachineDictionary> machineDictionaries,
            Dictionary<string, Dictionary<int, string>> dictionaryCache)
        {
            _machine = machine;
            _machineDictionaries = machineDictionaries;
            _dictionaryCache = dictionaryCache;
        }

        public Guid Id => _machine.Id;

        // ID для отображения (SerialNumber)
        public string DisplayId => _machine.SerialNumber.ToString();

        public string Name => _machine.Name;

        public string ModelName
        {
            get
            {
                if (_machine.IdModelNavigation != null)
                {
                    var manufactureName = _machine.IdModelNavigation.IdManufactureNavigation?.Name ?? "";
                    var modelName = _machine.IdModelNavigation.Model1 ?? "";
                    return $"{manufactureName} {modelName}".Trim();
                }
                return "Неизвестно";
            }
        }

        public string Company
        {
            get
            {
                var company = GetDictionaryValue("company");
                return string.IsNullOrEmpty(company) || company == "—"
                    ? "ООО Торговые Автоматы"
                    : company;
            }
        }


        // Получаем модем из MachineDictionary
        public string Modem
        {
            get
            {
                var modemEntry = _machineDictionaries
                    .FirstOrDefault(md => md.IdValueNavigation?.Key?.ToLower() == "operator");

                if (modemEntry?.IdValueNavigation != null)
                    return modemEntry.IdValueNavigation.Value;

                return _machine.KitOnlineId ?? "—";
            }
        }

        public string Address => _machine.Location;

        public string WorkingSince => _machine.InstallDate.ToString("dd.MM.yyyy");

        // Дополнительные свойства из Dictionary
        public string Status => GetDictionaryValue("status");
        public string WorkMode => GetDictionaryValue("work_mode");
        public string PaymentType => GetDictionaryValue("payment_type");
        public string Place => GetDictionaryValue("place");
        public string Operator => GetDictionaryValue("operator");
        public string Timezone => GetDictionaryValue("timezone");
        public string Notes => GetDictionaryValue("notes");

        private string GetDictionaryValue(string key)
        {
            var entry = _machineDictionaries
                .FirstOrDefault(md => md.IdValueNavigation?.Key?.ToLower() == key.ToLower());

            if (entry?.IdValueNavigation != null)
            {
                return entry.IdValueNavigation.Value;
            }

            return "—";
        }

        // Информация о менеджере, инженере, технике
        public string Manager => _machine.IdManagerNavigation?.Family ?? "";
        public string Engineer => _machine.IdEngineerNavigation?.Family ?? "";
        public string Technician => _machine.IdTechnicianNavigation?.Family ?? "";
    }
}