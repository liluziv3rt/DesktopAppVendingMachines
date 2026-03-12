using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using VendingMachineApp.ViewModels;

namespace VendingApp.ViewModels
{
    internal partial class MainViewModel : ViewModelBase
    {
        [ObservableProperty]
        private int workingMachinesCount;

        [ObservableProperty]
        private int totalMachinesCount;

        [ObservableProperty]
        private decimal totalMoneyInMachines;

        [ObservableProperty]
        private decimal? changeMoneyInMachines;

        [ObservableProperty]
        private decimal todayRevenue;

        [ObservableProperty]
        private decimal yesterdayRevenue;

        [ObservableProperty]
        private decimal todayCollected;

        [ObservableProperty]
        private decimal yesterdayCollected;

        [ObservableProperty]
        private int servicingToday;

        [ObservableProperty]
        private int servicingYesterday;

        [ObservableProperty]
        private string networkEfficiency;

        [ObservableProperty]
        private string networkStatus;

        [ObservableProperty]
        private string userName; 

        public MainViewModel()
        {
            LoadUserInfo(); // Сначала загружаем информацию о пользователе
            LoadData();
        }

        private void LoadUserInfo()
        {
            // Используем currentLogin из базового класса
            if (currentLogin != null)
            {
                // Если есть поле Login или Email
                if (!string.IsNullOrEmpty(currentLogin.Email))
                {
                    UserName = currentLogin.Email;
                }
                else if (!string.IsNullOrEmpty(currentLogin.Email))
                {
                    UserName = currentLogin.Email;
                }
                else
                {
                    UserName = $"Пользователь #{currentLogin.Id}";
                }
            }
            else
            {
                // Если пользователь не найден, пробуем загрузить из БД
                // Например, последнего залогинившегося или первого
                var defaultUser = db.Users.FirstOrDefault();
                if (defaultUser != null)
                {
                    currentLogin = defaultUser;
                    UserName = defaultUser.Email ?? defaultUser.Email ?? "Пользователь";
                }
                else
                {
                    UserName = "Гость";
                }
            }
        }

        private void LoadData()
        {
            try
            {
                LoadMachineStats();
                LoadFinancialStats();
                LoadServiceStats();
                CalculateEfficiency();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void LoadMachineStats()
        {
            // Получаем все автоматы
            var machines = db.VendingMachines.ToList();
            totalMachinesCount = machines.Count;

            // Получаем ID статуса "Работает" из словаря
            var workingStatusId = db.Dictionaries
                .FirstOrDefault(d => d.Key == "status" && d.Value == "Работает")?.Id;

            if (workingStatusId.HasValue)
            {
                // Находим автоматы, у которых есть запись в machine_dictionary со статусом "Работает"
                var workingMachineIds = db.MachineDictionaries
                    .Where(md => md.IdValue == workingStatusId.Value)
                    .Select(md => md.IdMachine)
                    .ToList();

                workingMachinesCount = workingMachineIds.Count;
            }
            else
            {
                workingMachinesCount = 0;
            }
        }

        private void LoadFinancialStats()
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);

            // Выручка из таблицы Sales
            todayRevenue = db.Sales
                .Where(s => s.TimeSale.Date == today)
                .Sum(s => s.TotalPrice);

            yesterdayRevenue = db.Sales
                .Where(s => s.TimeSale.Date == yesterday)
                .Sum(s => s.TotalPrice);

            // Деньги в автоматах: сумма (цена * остаток) из Products
            totalMoneyInMachines = db.Products
                .Sum(p => p.Price * p.QuantityAvailable);

            // Сдача - пока не подсчитываем
            changeMoneyInMachines = null;

            // Инкассация (временные значения)
            todayCollected = 111;
            yesterdayCollected = 111;
        }

        private void LoadServiceStats()
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);

            // Подсчет обслуживания по дате последнего обслуживания
            servicingToday = db.VendingMachines
                .Count(m => m.LastMaintenanceDate.Date == today);

            servicingYesterday = db.VendingMachines
                .Count(m => m.LastMaintenanceDate.Date == yesterday);
        }

        private void CalculateEfficiency()
        {
            if (totalMachinesCount > 0)
            {
                var efficiency = (double)workingMachinesCount / totalMachinesCount * 100;
                NetworkEfficiency = $"Работающих автоматов - {efficiency:F0}%";
            }
            else
            {
                NetworkEfficiency = "Работающих автоматов - 0%";
            }

            // Вычисляем статус на основе процента работающих
            if (workingMachinesCount == totalMachinesCount)
                NetworkStatus = "Отлично";
            else if (workingMachinesCount >= totalMachinesCount * 0.9)
                NetworkStatus = "Хорошо";
            else if (workingMachinesCount >= totalMachinesCount * 0.7)
                NetworkStatus = "Удовлетворительно";
            else
                NetworkStatus = "Требует внимания";
        }
    }
}