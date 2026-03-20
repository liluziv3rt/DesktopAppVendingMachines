using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopAppVendingMachines.Models;
using DesktopAppVendingMachines.Services;
using DesktopAppVendingMachines.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        // Кэши
        private Dictionary<string, Dictionary<int, string>> _dictionaryCache = new();
        private Dictionary<Guid, List<MachineDictionary>> _machineDictionaryCache = new();

        private readonly Action<Guid> _onEditRequest;
        private CancellationTokenSource _searchDebounceCts;

        public VendingMachinesViewModel(Action<Guid> onEditRequest = null)
        {
            _onEditRequest = onEditRequest;
            LoadDictionaryCache();
            LoadMachineDictionaryCache();
            LoadVendingMachines();
        }

        partial void OnSearchTextChanged(string value)
        {
            _searchDebounceCts?.Cancel();
            _searchDebounceCts = new CancellationTokenSource();
            var token = _searchDebounceCts.Token;

            Task.Delay(300, token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    CurrentPage = 1; // сбрасываем на первую страницу
                    LoadVendingMachines();
                });
            }, token);
        }

        private void LoadDictionaryCache()
        {
            try
            {
                var allDictionaries = db.Dictionaries.ToList();
                _dictionaryCache = allDictionaries
                    .GroupBy(d => d.Key)
                    .ToDictionary(g => g.Key, g => g.ToDictionary(d => d.Id, d => d.Value));
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
                    .ToDictionary(g => g.Key, g => g.ToList());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки связей машин: {ex.Message}");
            }
        }

        public bool CanGoPrevious => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;

        private void LoadVendingMachines()
        {
            try
            {
                IsLoading = true;

                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(CanGoPrevious));
                OnPropertyChanged(nameof(CanGoNext));

                var query = db.VendingMachines
                    .Include(v => v.IdModelNavigation)
                        .ThenInclude(m => m.IdManufactureNavigation)
                    .Include(v => v.User)
                    .Include(v => v.IdManagerNavigation)
                    .Include(v => v.IdEngineerNavigation)
                    .Include(v => v.IdTechnicianNavigation)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    query = query.Where(v =>
                        v.Name.Contains(SearchText));
                }

                TotalCount = query.Count();

                var machines = query
                    .OrderBy(v => v.Name)
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                VendingMachines.Clear();
                foreach (var machine in machines)
                {
                    var machineDictionaries = _machineDictionaryCache.ContainsKey(machine.Id)
                        ? _machineDictionaryCache[machine.Id]
                        : new List<MachineDictionary>();

                    VendingMachines.Add(new VendingMachineViewModel(machine, machineDictionaries, _dictionaryCache));
                }

                OnPropertyChanged(nameof(TotalPages));

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
        private void AddMachine()
        {
            NavigationService.GoToAddMachine();
        }

        partial void OnPageSizeChanged(int value)
        {
            CurrentPage = 1;
            LoadVendingMachines();
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
        private void EditMachine(Guid id)
        {
            NavigationService.GoToEditMachine(id);
        }

        [RelayCommand]
        private async Task DeleteMachine(Guid id)
        {
            var result = await ShowConfirmationDialog(
                "Подтверждение удаления",
                "Вы уверены, что хотите удалить этот торговый автомат?\nВсе связанные данные также будут удалены.",
                "Да", "Нет");
            if (!result) return;

            try
            {
                IsLoading = true;
                using var transaction = await db.Database.BeginTransactionAsync();

                var machineDicts = db.MachineDictionaries.Where(md => md.IdMachine == id).ToList();
                db.MachineDictionaries.RemoveRange(machineDicts);

                var maintenances = db.Maintenances.Where(m => m.IdVendingMachine == id).ToList();
                db.Maintenances.RemoveRange(maintenances);

                var machine = db.VendingMachines.Find(id);
                if (machine != null) db.VendingMachines.Remove(machine);

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                if (_machineDictionaryCache.ContainsKey(id))
                    _machineDictionaryCache.Remove(id);

                LoadVendingMachines();
                await ShowMessage("Успешно", "Торговый автомат успешно удалён");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при удалении: {ex.Message}");
                await ShowMessage("Ошибка", $"Не удалось удалить автомат: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
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