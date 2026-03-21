using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopAppVendingMachines.Models;
using DesktopAppVendingMachines.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

using Avalonia.Controls.ApplicationLifetimes;
using System.Globalization;
using System.IO;
using System.Text;

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

                // Удаляем через SQL запросы
                await db.Database.ExecuteSqlRawAsync(
                    "DELETE FROM praktika.machine_dictionary WHERE id_machine = {0}", id);

                await db.Database.ExecuteSqlRawAsync(
                    "DELETE FROM praktika.maintenance WHERE id_vending_machine = {0}", id);

                // Удаляем сам автомат
                await db.Database.ExecuteSqlRawAsync(
                    "DELETE FROM praktika.vending_machines WHERE id = {0}", id);

                // Обновляем кэш
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

        [RelayCommand]
        private async Task Export()
        {
            try
            {
                IsLoading = true;

                // Получаем все автоматы с нужными включениями
                var allMachines = await db.VendingMachines
                    .Include(v => v.IdModelNavigation)
                        .ThenInclude(m => m.IdManufactureNavigation)
                    .Include(v => v.User)
                    .Include(v => v.IdManagerNavigation)
                    .Include(v => v.IdEngineerNavigation)
                    .Include(v => v.IdTechnicianNavigation)
                    .OrderBy(v => v.Name)
                    .ToListAsync();

                if (!allMachines.Any())
                {
                    await ShowMessage("Экспорт", "Нет данных для экспорта");
                    return;
                }

                // Создаем диалог выбора места сохранения
                var saveFileDialog = new Avalonia.Controls.SaveFileDialog
                {
                    Title = "Сохранить CSV файл",
                    DefaultExtension = "csv",
                    Filters = new List<Avalonia.Controls.FileDialogFilter>
            {
                new Avalonia.Controls.FileDialogFilter
                {
                    Name = "CSV файлы",
                    Extensions = new List<string> { "csv" }
                }
            },
                    InitialFileName = $"Торговые_автоматы_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                // Получаем главное окно
                Avalonia.Controls.Window? mainWindow = null;

                if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    mainWindow = desktop.MainWindow;
                }

                if (mainWindow == null)
                {
                    await ShowMessage("Ошибка", "Не удалось открыть диалог сохранения");
                    return;
                }

                var filePath = await saveFileDialog.ShowAsync(mainWindow);

                if (string.IsNullOrEmpty(filePath))
                {
                    return;
                }

                // Генерируем CSV
                var csv = GenerateMachinesCsv(allMachines);

                // Сохраняем файл
                var encoding = new UTF8Encoding(true);
                await File.WriteAllTextAsync(filePath, csv, encoding);

                await ShowMessage("Успешно", $"Данные успешно экспортированы в файл:\n{filePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при экспорте: {ex.Message}");
                await ShowMessage("Ошибка", $"Не удалось экспортировать данные: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string GenerateMachinesCsv(List<VendingMachine> machines)
        {
            using var writer = new StringWriter();
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                Encoding = Encoding.UTF8
            });

            // Заголовки
            csv.WriteField("ID");
            csv.WriteField("Название");
            csv.WriteField("Модель");
            csv.WriteField("Производитель");
            csv.WriteField("Адрес");
            csv.WriteField("Компания");
            csv.WriteField("Модем");
            csv.WriteField("Дата установки");
            csv.WriteField("Менеджер");
            csv.WriteField("Инженер");
            csv.WriteField("Техник");
            csv.NextRecord();

            // Данные
            foreach (var machine in machines)
            {
                var modelName = machine.IdModelNavigation?.Model1 ?? "";
                var manufactureName = machine.IdModelNavigation?.IdManufactureNavigation?.Name ?? "";

                csv.WriteField(machine.SerialNumber);
                csv.WriteField(machine.Name);
                csv.WriteField(modelName);
                csv.WriteField(manufactureName);
                csv.WriteField(machine.Location);
                csv.WriteField(""); // Компания - нужно получить из MachineDictionary
                csv.WriteField(machine.KitOnlineId);
                csv.WriteField(machine.InstallDate.ToString("dd.MM.yyyy"));
                csv.WriteField(machine.IdManagerNavigation?.Family ?? "");
                csv.WriteField(machine.IdEngineerNavigation?.Family ?? "");
                csv.WriteField(machine.IdTechnicianNavigation?.Family ?? "");
                csv.NextRecord();
            }

            return writer.ToString();
        }


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