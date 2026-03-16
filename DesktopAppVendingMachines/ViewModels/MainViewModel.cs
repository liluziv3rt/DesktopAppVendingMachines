using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using DesktopAppVendingMachines.Models; // ваши модели

namespace DesktopAppVendingMachines.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string efficiencyPercent;

        [ObservableProperty]
        private string salesDateRange;

        [ObservableProperty]
        private string userPhotoPath = "avares://DesktopAppVendingMachines/Assets/default-avatar.png";

        [ObservableProperty]
        private int totalMachinesCount;

        // --- Серии для круговых диаграмм ---
        [ObservableProperty]
        private IEnumerable<ISeries> efficiencySeries;

        [ObservableProperty]
        private IEnumerable<ISeries> stateSeries;

        // --- Для столбчатого графика продаж ---
        [ObservableProperty]
        private bool isSumMode = true;

        [ObservableProperty]
        private ISeries[] salesSeries;

        [ObservableProperty]
        private Axis[] xAxes;

        // Поля для хранения количеств, чтобы использовать в DataLabelsFormatter
        private int _workingCount;
        private int _servicingCount;
        private int _brokenCount;
        private int _totalMachines;

        public MainViewModel()
        {
            LoadUserInfo();
            LoadMachineStats();
            LoadSalesChart();
        }

        private void LoadUserInfo()
        {
            if (currentLogin != null)
            {
                // Собираем ФИО - подставьте свои названия полей
                var fullName = $"{currentLogin.Family} {currentLogin.Name} {currentLogin.Patronymic}".Trim();
                if (string.IsNullOrWhiteSpace(fullName))
                    fullName = currentLogin.Email ?? "Пользователь";
                UserName = fullName;
            }
            else
            {
                UserName = "Гость";
            }
        }

        private void LoadMachineStats()
        {
            try
            {
                // Общее количество автоматов
                totalMachinesCount = db.VendingMachines.Count();
                System.Diagnostics.Debug.WriteLine($"=== Загрузка статистики ===");
                System.Diagnostics.Debug.WriteLine($"Всего автоматов в БД: {totalMachinesCount}");

                // Получаем все записи из dictionary с ключом "status"
                var statusEntries = db.Dictionaries
                    .Where(d => d.Key == "status")
                    .Select(d => new { d.Id, d.Value })
                    .ToList();
                System.Diagnostics.Debug.WriteLine($"Найдено записей в dictionary с key='status': {statusEntries.Count}");
                foreach (var se in statusEntries)
                    System.Diagnostics.Debug.WriteLine($"  ID: {se.Id}, Value: '{se.Value}'");

                // Если нет статусов, выходим
                if (statusEntries.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("ВНИМАНИЕ: нет статусов в dictionary!");
                    EfficiencySeries = Array.Empty<ISeries>();
                    StateSeries = Array.Empty<ISeries>();
                    return;
                }

                // Строим словарь ID -> значение
                var statusIdMap = statusEntries.ToDictionary(s => s.Value, s => s.Id);
                var statusIds = statusIdMap.Values.Select(id => (int?)id).ToList();

                // Получаем все связи из machine_dictionary
                var allLinks = db.MachineDictionaries
                    .Where(md => statusIds.Contains(md.IdValue))
                    .ToList();
                System.Diagnostics.Debug.WriteLine($"Всего связей в machine_dictionary для этих статусов: {allLinks.Count}");

                // Группируем по IdValue
                var machineStatusCounts = allLinks
                    .GroupBy(md => md.IdValue)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Вспомогательная функция
                int GetCount(string statusName)
                {
                    if (statusIdMap.TryGetValue(statusName, out int id))
                    {
                        int? nullableId = id;
                        if (machineStatusCounts.TryGetValue(nullableId, out int count))
                            return count;
                        else
                            return 0;
                    }
                    return 0;
                }

                _workingCount = GetCount("Работает");
                _servicingCount = GetCount("Обслуживается");
                _brokenCount = GetCount("Сломан");
                _totalMachines = totalMachinesCount;

                System.Diagnostics.Debug.WriteLine($"Подсчитано: Работает={_workingCount}, Обслуживается={_servicingCount}, Сломано={_brokenCount}");
                System.Diagnostics.Debug.WriteLine($"Сумма статусов: {_workingCount + _servicingCount + _brokenCount} (должна равняться общему количеству автоматов, если у всех есть статус)");

                // Создаём серии
                int nonWorking = _servicingCount + _brokenCount;

                double percent = _totalMachines == 0
                    ? 0
                    : (double)_workingCount / _totalMachines * 100;

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
                OnPropertyChanged(nameof(EfficiencySeries));
                OnPropertyChanged(nameof(StateSeries));

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
                    labels.Add($"{dayLabel}\n{weekDay}");
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
    }
}