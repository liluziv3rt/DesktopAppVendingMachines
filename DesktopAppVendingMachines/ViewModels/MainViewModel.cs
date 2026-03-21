using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopAppVendingMachines.Models;
using DesktopAppVendingMachines.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DesktopAppVendingMachines.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private object? currentPage;

        [ObservableProperty]
        private string currentPageTitle = "Главная";

        [ObservableProperty]
        private bool isAdminMenuExpanded = false;

        [ObservableProperty]
        private string userRole;

        [ObservableProperty]
        private string userPhotoPath = "avares://DesktopAppVendingMachines/Assets/Flag.png";

        [ObservableProperty]
        private string efficiencyPercent;

        [ObservableProperty]
        private string salesDateRange;

        [ObservableProperty]
        private int totalMachinesCount;

        [ObservableProperty]
        private IEnumerable<ISeries> efficiencySeries;

        [ObservableProperty]
        private IEnumerable<ISeries> stateSeries;

        [ObservableProperty]
        private bool isSumMode = true;

        [ObservableProperty]
        private ISeries[] salesSeries;

        [ObservableProperty]
        private Axis[] xAxes;

        private int _workingCount;
        private int _servicingCount;
        private int _brokenCount;

        [ObservableProperty]
        private string moneyInMachines;

        [ObservableProperty]
        private string changeInMachines = "12 729 ₽";

        [ObservableProperty]
        private string revenueToday;

        [ObservableProperty]
        private string revenueYesterday;

        [ObservableProperty]
        private string collectedToday = "8 145 ₽";

        [ObservableProperty]
        private string collectedYesterday = "9 690 ₽";

        [ObservableProperty]
        private string serviceInfo = "2/2";

        public MainViewModel()
        {
            NavigationService.MainVM = this;

            LoadUserInfo();
            LoadMachineStats();
            LoadSalesChart();
            LoadSummaryData();

            NavigateTo("Main");
        }

        private void LoadUserInfo()
        {
            var currentUser = SessionManager.CurrentUser;

            if (currentUser != null)
            {
                var fullName = $"{currentUser.Family} {currentUser.Name} {currentUser.Patronymic}".Trim();
                if (string.IsNullOrWhiteSpace(fullName))
                    fullName = currentUser.Email ?? "Пользователь";

                UserName = fullName;

                if (currentUser.IdRoleNavigation != null)
                {
                    UserRole = currentUser.IdRoleNavigation.Name;
                }
                else if (currentUser.IsManager == true)
                {
                    UserRole = "Менеджер";
                }
                else if (currentUser.IsEngineer == true)
                {
                    UserRole = "Инженер";
                }
                else if (currentUser.IsOperator == true)
                {
                    UserRole = "Оператор";
                }
                else
                {
                    UserRole = "Пользователь";
                }

                if (!string.IsNullOrEmpty(currentUser.Images))
                {
                    UserPhotoPath = currentUser.Images;
                }
            }
            else
            {
                UserName = "Гость";
                UserRole = "Администратор";
            }
        }

        private void LoadMachineStats()
        {
            try
            {
                totalMachinesCount = db.VendingMachines.Count();

                var statusEntries = db.Dictionaries
                    .Where(d => d.Key == "status")
                    .Select(d => new { d.Id, d.Value })
                    .ToList();

                if (statusEntries.Count == 0)
                {
                    EfficiencySeries = Array.Empty<ISeries>();
                    StateSeries = Array.Empty<ISeries>();
                    return;
                }

                var statusIdMap = statusEntries.ToDictionary(s => s.Value, s => s.Id);
                var statusIds = statusIdMap.Values.Select(id => (int?)id).ToList();

                var allLinks = db.MachineDictionaries
                    .Where(md => statusIds.Contains(md.IdValue))
                    .ToList();

                var machineStatusCounts = allLinks
                    .GroupBy(md => md.IdValue)
                    .ToDictionary(g => g.Key, g => g.Count());

                int GetCount(string statusName)
                {
                    if (statusIdMap.TryGetValue(statusName, out int id))
                    {
                        int? nullableId = id;
                        if (machineStatusCounts.TryGetValue(nullableId, out int count))
                            return count;
                    }
                    return 0;
                }

                _workingCount = GetCount("Работает");
                _servicingCount = GetCount("Обслуживается");
                _brokenCount = GetCount("Сломан");

                int nonWorking = _servicingCount + _brokenCount;

                double percent = totalMachinesCount == 0
                    ? 0
                    : (double)_workingCount / totalMachinesCount * 100;

                EfficiencyPercent = $"{percent:F0}%";

                EfficiencySeries = new ISeries[]
                {
                    new PieSeries<int>
                    {
                        Values = new[]{ _workingCount },
                        Fill = new SolidColorPaint(SKColor.Parse("#2ECC71")),
                        InnerRadius = 70,
                        MaxRadialColumnWidth = 40,
                        TooltipLabelFormatter = chartPoint =>
                            $"{chartPoint.PrimaryValue} автоматов"
                    },
                    new PieSeries<int>
                    {
                        Values = new[]{ nonWorking },
                        Fill = new SolidColorPaint(SKColor.Parse("#E74C3C")),
                        InnerRadius = 70,
                        MaxRadialColumnWidth = 40,
                        TooltipLabelFormatter = chartPoint =>
                            $"{chartPoint.PrimaryValue} автоматов"
                    }
                };

                StateSeries = new ISeries[]
                {
                    new PieSeries<int>
                    {
                        Values = new[]{ _workingCount },
                        Fill = new SolidColorPaint(SKColor.Parse("#2ECC71")),
                        InnerRadius = 70,
                        TooltipLabelFormatter = chartPoint =>
                            $"Работают: {chartPoint.PrimaryValue}"
                    },
                    new PieSeries<int>
                    {
                        Values = new[]{ _servicingCount },
                        Fill = new SolidColorPaint(SKColor.Parse("#F39C12")),
                        InnerRadius = 70,
                        TooltipLabelFormatter = chartPoint =>
                            $"Обслуживаются: {chartPoint.PrimaryValue}"
                    },
                    new PieSeries<int>
                    {
                        Values = new[]{ _brokenCount },
                        Fill = new SolidColorPaint(SKColor.Parse("#E74C3C")),
                        InnerRadius = 70,
                        TooltipLabelFormatter = chartPoint =>
                            $"Сломаны: {chartPoint.PrimaryValue}"
                    }
                };

                OnPropertyChanged(nameof(EfficiencySeries));
                OnPropertyChanged(nameof(StateSeries));
                OnPropertyChanged(nameof(EfficiencyPercent));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ОШИБКА LoadMachineStats: {ex.Message}");
                EfficiencySeries = Array.Empty<ISeries>();
                StateSeries = Array.Empty<ISeries>();
            }
        }

        private void LoadSalesChart()
        {
            try
            {
                var endDate = DateTime.Today;
                var startDate = endDate.AddDays(-9);

                SalesDateRange = $"Данные по продажам с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}";

                var salesData = db.Sales
                    .Where(s => s.TimeSale.Date >= startDate && s.TimeSale.Date <= endDate)
                    .GroupBy(s => s.TimeSale.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TotalSum = g.Sum(s => s.TotalPrice),
                        TotalQuantity = g.Sum(s => s.Quantity)
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                var allDates = Enumerable.Range(0, 10)
                    .Select(offset => startDate.AddDays(offset))
                    .ToList();

                var sumValues = new List<double>();
                var quantityValues = new List<double>();
                var labels = new List<string>();

                foreach (var date in allDates)
                {
                    var found = salesData.FirstOrDefault(x => x.Date == date);
                    sumValues.Add(found != null ? (double)found.TotalSum : 0);
                    quantityValues.Add(found != null ? found.TotalQuantity : 0);

                    string dayLabel = date.Day.ToString();
                    string weekDay = date.DayOfWeek switch
                    {
                        DayOfWeek.Sunday => "Вс",
                        DayOfWeek.Monday => "Пн",
                        DayOfWeek.Tuesday => "Вт",
                        DayOfWeek.Wednesday => "Ср",
                        DayOfWeek.Thursday => "Чт",
                        DayOfWeek.Friday => "Пт",
                        DayOfWeek.Saturday => "Сб",
                        _ => ""
                    };
                    labels.Add($"{dayLabel} {weekDay}");
                }

                XAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = labels.ToArray(),
                        LabelsRotation = 0,
                        TextSize = 12,
                    }
                };

                SalesSeries = new ISeries[]
                {
                    new ColumnSeries<double>
                    {
                        Name = "Сумма",
                        Values = sumValues,
                        Fill = new SolidColorPaint(SKColor.Parse("#3498DB")),
                        Stroke = null,
                        IsVisible = isSumMode,
                    },
                    new ColumnSeries<double>
                    {
                        Name = "Количество",
                        Values = quantityValues,
                        Fill = new SolidColorPaint(SKColor.Parse("#E67E22")),
                        Stroke = null,
                        IsVisible = !isSumMode,
                    }
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка LoadSalesChart: {ex.Message}");
                SetDummyChartData();
            }
        }

        private void LoadSummaryData()
        {
            try
            {
                decimal totalIncome = db.VendingMachines.Sum(v => v.TotalIncome);
                MoneyInMachines = $"{totalIncome:N0} ₽";

                ChangeInMachines = "12 729 ₽";

                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                decimal revenueTodayValue = db.Sales
                    .Where(s => s.TimeSale >= today && s.TimeSale < tomorrow)
                    .Sum(s => s.TotalPrice);
                RevenueToday = $"{revenueTodayValue:N0} ₽";

                var yesterday = today.AddDays(-1);
                decimal revenueYesterdayValue = db.Sales
                    .Where(s => s.TimeSale >= yesterday && s.TimeSale < today)
                    .Sum(s => s.TotalPrice);
                RevenueYesterday = $"{revenueYesterdayValue:N0} ₽";

                CollectedToday = "8 145 ₽";
                CollectedYesterday = "9 690 ₽";

                ServiceInfo = $"{_servicingCount}/{totalMachinesCount}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка LoadSummaryData: {ex.Message}");
            }
        }
        private void SetDummyChartData()
        {
            var dates = Enumerable.Range(0, 10).Select(i => DateTime.Today.AddDays(-9 + i)).ToArray();
            var sumValues = new double[] { 15000, 10000, 5000, 12000, 8000, 14000, 11000, 9000, 13000, 7000 };
            var quantityValues = new double[] { 1, 2, 3, 4, 3, 5, 4, 3, 6, 4 };

            XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = dates.Select(d => d.Day.ToString() + "\n" +
                        (d.DayOfWeek == DayOfWeek.Sunday ? "Вс" :
                         d.DayOfWeek == DayOfWeek.Monday ? "Пн" :
                         d.DayOfWeek == DayOfWeek.Tuesday ? "Вт" :
                         d.DayOfWeek == DayOfWeek.Wednesday ? "Ср" :
                         d.DayOfWeek == DayOfWeek.Thursday ? "Чт" :
                         d.DayOfWeek == DayOfWeek.Friday ? "Пт" : "Сб")).ToArray(),
                    LabelsRotation = 0,
                    TextSize = 12,
                }
            };

            SalesSeries = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name = "Сумма",
                    Values = sumValues,
                    Fill = new SolidColorPaint(SKColor.Parse("#3498DB")),
                    Stroke = null,
                    IsVisible = isSumMode,
                },
                new ColumnSeries<double>
                {
                    Name = "Количество",
                    Values = quantityValues,
                    Fill = new SolidColorPaint(SKColor.Parse("#E67E22")),
                    Stroke = null,
                    IsVisible = !isSumMode,
                }
            };
        }

        [RelayCommand]
        private void ShowSum()
        {
            IsSumMode = true;
            if (SalesSeries?.Length >= 2)
            {
                SalesSeries[0].IsVisible = true;
                SalesSeries[1].IsVisible = false;
            }
        }

        [RelayCommand]
        private void ShowQuantity()
        {
            IsSumMode = false;
            if (SalesSeries?.Length >= 2)
            {
                SalesSeries[0].IsVisible = false;
                SalesSeries[1].IsVisible = true;
            }
        }

        public void NavigateToAddCompany()
        {
            CurrentPage = new AddCompanyViewModel();
            CurrentPageTitle = "Добавление компании";
        }

        public void NavigateToEditCompany(int id)
        {
            CurrentPage = new EditCompanyViewModel(id);
            CurrentPageTitle = "Редактирование компании";
        }


        public void NavigateToVendingMachines()
        {
            CurrentPage = new VendingMachinesViewModel(id => NavigateToEditMachine(id)); 
            CurrentPageTitle = "Торговые автоматы";
        }

        public void NavigateToCompanies()
        {
            CurrentPage = new CompaniesViewModel();
        }


        public void NavigateToEditMachine(Guid id)
        {
            CurrentPage = new EditVendingMachineViewModel(id);
            CurrentPageTitle = "Редактирование торгового автомата";
        }

        public void NavigateToAddMachine()
        {
            CurrentPage = new AddVendingMachineViewModel();
            CurrentPageTitle = "Создание торгового автомата";
        }

        [RelayCommand]
        private void NavigateTo(string page)
        {
            switch (page)
            {
                case "Main":
                    CurrentPage = null;
                    CurrentPageTitle = "Главная";
                    IsAdminMenuExpanded = false;
                    break;

                case "Monitor":
                    CurrentPageTitle = "Монитор ТА";
                    IsAdminMenuExpanded = false;
                    break;

                case "Reports":
                    CurrentPageTitle = "Детальные отчеты";
                    IsAdminMenuExpanded = false;
                    break;

                case "Inventory":
                    CurrentPageTitle = "Учет ТМЦ";
                    IsAdminMenuExpanded = false;
                    break;

                case "VendingMachines":
                    NavigateToVendingMachines();
                    IsAdminMenuExpanded = true;
                    break;

                case "Companies":
                    NavigateToCompanies();
                    IsAdminMenuExpanded = true;
                    break;

                case "Users":
                    CurrentPageTitle = "Пользователи";
                    IsAdminMenuExpanded = true;
                    break;

                case "Modems":
                    CurrentPageTitle = "Модемы";
                    IsAdminMenuExpanded = true;
                    break;

                case "Additional":
                    CurrentPageTitle = "Дополнительные";
                    IsAdminMenuExpanded = true;
                    break;
            }
        }

        [RelayCommand]
        private void Logout()
        {
            SessionManager.ClearSession();
            MainWindowViewModel.Instance.PageSwitcher = new SignInViewModel();
        }
    }
}
