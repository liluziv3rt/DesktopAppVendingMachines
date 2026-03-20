using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvHelper;
using CsvHelper.Configuration;
using DesktopAppVendingMachines.Models;
using DesktopAppVendingMachines.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;


namespace DesktopAppVendingMachines.ViewModels
{
    public partial class CompaniesViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<CompanyViewModel> companies = new();

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

        private CancellationTokenSource _searchDebounceCts;

        public CompaniesViewModel()
        {
            LoadCompanies();
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
                    CurrentPage = 1;
                    LoadCompanies();
                });
            }, token);
        }

        partial void OnPageSizeChanged(int value)
        {
            CurrentPage = 1;
            LoadCompanies();
        }

        public bool CanGoPrevious => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public string PageInfo => $"Запись с {((CurrentPage - 1) * PageSize) + 1} до {Math.Min(CurrentPage * PageSize, TotalCount)} из {TotalCount} записей";

        private void LoadCompanies()
        {
            try
            {
                IsLoading = true;

                var query = db.Companies
                    .Include(c => c.IdParentCompanyNavigation)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    query = query.Where(c => c.Name.Contains(SearchText));
                }

                TotalCount = query.Count();

                var companiesList = query
                    .OrderBy(c => c.Name)
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                Companies.Clear();
                foreach (var company in companiesList)
                {
                    Companies.Add(new CompanyViewModel(company));
                }

                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(CanGoPrevious));
                OnPropertyChanged(nameof(CanGoNext));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки компаний: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void AddCompany()
        {
            NavigationService.GoToAddCompany();
        }

        [RelayCommand]
        private void PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                LoadCompanies();
            }
        }

        [RelayCommand]
        private void NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                LoadCompanies();
            }
        }

        [RelayCommand]
        private void EditCompany(int id)
        {
            NavigationService.GoToEditCompany(id);
        }

        [RelayCommand]
        private async Task DeleteCompany(int id)
        {
            var result = await ShowConfirmationDialog(
                "Подтверждение удаления",
                "Вы уверены, что хотите удалить эту компанию?\nВсе дочерние компании также будут удалены.",
                "Да", "Нет");
            if (!result) return;

            try
            {
                IsLoading = true;

                var company = db.Companies
                    .Include(c => c.InverseIdParentCompanyNavigation)
                    .FirstOrDefault(c => c.Id == id);

                if (company != null)
                {
                    db.Companies.Remove(company);
                    await db.SaveChangesAsync();

                    // Удаляем связанные записи из Dictionary
                    var dictEntries = db.Dictionaries.Where(d => d.Key == "company" && d.Value == company.Name).ToList();
                    if (dictEntries.Any())
                    {
                        db.Dictionaries.RemoveRange(dictEntries);
                        await db.SaveChangesAsync();
                    }
                }

                LoadCompanies();
                await ShowMessage("Успешно", "Компания успешно удалена");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при удалении: {ex.Message}");
                await ShowMessage("Ошибка", $"Не удалось удалить компанию: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task Export()
        {
            try
            {
                IsLoading = true;

                // Получаем все компании (без пагинации)
                var allCompanies = await db.Companies
                    .Include(c => c.IdParentCompanyNavigation)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                if (!allCompanies.Any())
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
                    InitialFileName = $"Компании_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                // Получаем главное окно через Application.Current
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
                    return; // Пользователь отменил сохранение
                }

                // Генерируем CSV
                var csv = GenerateCsv(allCompanies);

                // Сохраняем файл с BOM для корректного отображения кириллицы
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



        private string GenerateCsv(List<Company> companies)
        {
            using var writer = new StringWriter();
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                Encoding = Encoding.UTF8
            });

            csv.WriteField("ID");
            csv.WriteField("Название");
            csv.WriteField("Вышестоящая компания");
            csv.WriteField("Адрес");
            csv.WriteField("Контакты");
            csv.WriteField("Дата создания");
            csv.NextRecord();

            foreach (var company in companies)
            {
                csv.WriteField(company.Id);
                csv.WriteField(company.Name);
                csv.WriteField(company.IdParentCompanyNavigation?.Name ?? "");
                csv.WriteField(company.Adress);
                csv.WriteField(company.Contacts);
                csv.WriteField(company.CreatedDate?.ToString("dd.MM.yyyy") ?? "");
                csv.NextRecord();
            }

            return writer.ToString();
        }


        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            // Если поле содержит точку с запятой, кавычки или перевод строки, заключаем в кавычки
            if (field.Contains(";") || field.Contains("\"") || field.Contains("\n") || field.Contains(","))
            {
                // Экранируем кавычки
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }

            return field;
        }
    }

    public class CompanyViewModel
    {
        private readonly Company _company;

        public CompanyViewModel(Company company)
        {
            _company = company;
        }

        public int Id => _company.Id;
        public string Name => _company.Name ?? "—";

        public string ParentCompanyName => _company.IdParentCompanyNavigation?.Name ?? "—";

        public string Address => _company.Adress ?? "—";
        public string Contacts => _company.Contacts ?? "—";
        public string CreatedDate => _company.CreatedDate?.ToString("dd.MM.yyyy") ?? "—";
    }
}