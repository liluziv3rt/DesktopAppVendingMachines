using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopAppVendingMachines.Models;
using DesktopAppVendingMachines.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Tmds.DBus.Protocol;

namespace DesktopAppVendingMachines.ViewModels
{
    public partial class AddCompanyViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private Company selectedParentCompany;

        [ObservableProperty]
        private string address;

        [ObservableProperty]
        private string contacts;

        public ObservableCollection<Company> ParentCompanies { get; } = new();

        public AddCompanyViewModel()
        {
            LoadParentCompanies();
        }

        private void LoadParentCompanies()
        {
            ParentCompanies.Clear();
            // Добавляем пустой вариант для отсутствия родительской компании
            ParentCompanies.Add(null);

            foreach (var company in db.Companies.OrderBy(c => c.Name))
            {
                ParentCompanies.Add(company);
            }
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ShowMessage("Ошибка", "Название компании обязательно");
                return false;
            }

            // Проверяем уникальность названия
            if (db.Companies.Any(c => c.Name == Name.Trim()))
            {
                ShowMessage("Ошибка", $"Компания с названием '{Name}' уже существует");
                return false;
            }

            return true;
        }

        [RelayCommand]
        private async Task Save()
        {
            if (!ValidateFields()) return;

            try
            {
                var company = new Company
                {
                    Name = Name.Trim(),
                    IdParentCompany = SelectedParentCompany?.Id,
                    Adress = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
                    Contacts = string.IsNullOrWhiteSpace(Contacts) ? null : Contacts.Trim(),
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now)
                };

                db.Companies.Add(company);
                await db.SaveChangesAsync();

                // Добавляем компанию в Dictionary
                var dictEntry = new Dictionary
                {
                    Key = "company",
                    Value = company.Name
                };
                db.Dictionaries.Add(dictEntry);
                await db.SaveChangesAsync();

                await ShowMessage("Успешно", "Компания успешно создана");
                NavigationService.GoToCompanies();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при создании компании: {ex.Message}");
                await ShowMessage("Ошибка", $"Не удалось создать компанию: {ex.Message}");
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            NavigationService.GoToCompanies();
        }
    }
}