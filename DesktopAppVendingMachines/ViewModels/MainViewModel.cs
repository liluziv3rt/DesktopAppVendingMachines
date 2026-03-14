using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using DesktopAppVendingMachines.Models; 

namespace DesktopAppVendingMachines.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string userPhotoPath = "avares://DesktopAppVendingMachines/Assets/Flag.png";

        [ObservableProperty]
        private int totalMachinesCount;

        [ObservableProperty]
        private int workingMachinesPercent;

        [ObservableProperty]
        private int servicingMachinesPercent;

        [ObservableProperty]
        private int brokenMachinesPercent;

        [ObservableProperty]
        private bool isSumMode = true;

        [ObservableProperty]
        private ISeries[] series;

        [ObservableProperty]
        private Axis[] xAxes;

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
                // Собираем ФИО
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
                TotalMachinesCount = db.VendingMachines.Count();
                if (TotalMachinesCount == 0) return;

                // Получаем статусы из словаря
                var statusEntries = db.Dictionaries
                    .Where(d => d.Key == "status")
                    .Select(d => new { d.Id, d.Value })
                    .ToList();

                var statusIdMap = statusEntries.ToDictionary(s => s.Value, s => s.Id);
                var statusIds = statusIdMap.Values.Select(id => (int?)id).ToList();

                var machineStatusCounts = db.MachineDictionaries
                    .Where(md => statusIds.Contains(md.IdValue))
                    .GroupBy(md => md.IdValue)
                    .ToDictionary(g => g.Key, g => g.Count());

                int GetCount(string statusName)
                {
                    if (statusIdMap.TryGetValue(statusName, out int id))
                        return machineStatusCounts.TryGetValue(id, out int count) ? count : 0;
                    return 0;
                }

                int working = GetCount("Работает");
                int servicing = GetCount("Обслуживается");
                int broken = GetCount("Сломан");

                WorkingMachinesPercent = (int)Math.Round((double)working / TotalMachinesCount * 100);
                ServicingMachinesPercent = (int)Math.Round((double)servicing / TotalMachinesCount * 100);
                BrokenMachinesPercent = (int)Math.Round((double)broken / TotalMachinesCount * 100);
            }
            catch (Exception ex)
            {
                // Логирование
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void LoadSalesChart()
        {
            // Заглушка для графика – замените на реальные данные из БД
            var dates = new[]
            {
                DateTime.Today.AddDays(-9), DateTime.Today.AddDays(-8),
                DateTime.Today.AddDays(-7), DateTime.Today.AddDays(-6),
                DateTime.Today.AddDays(-5), DateTime.Today.AddDays(-4),
                DateTime.Today.AddDays(-3), DateTime.Today.AddDays(-2),
                DateTime.Today.AddDays(-1), DateTime.Today
            };

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

            Series = new ISeries[]
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
            Series[0].IsVisible = true;
            Series[1].IsVisible = false;
        }

        [RelayCommand]
        private void ShowQuantity()
        {
            IsSumMode = false;
            Series[0].IsVisible = false;
            Series[1].IsVisible = true;
        }
    }
}