using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopAppVendingMachines.Models;
using DesktopAppVendingMachines.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using Tmds.DBus.Protocol;

namespace DesktopAppVendingMachines.ViewModels
{
    public partial class EditCompanyViewModel : ViewModelBase
    {
        private readonly int _companyId;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private Company selectedParentCompany;

        [ObservableProperty]
        private string address;

        [ObservableProperty]
        private string contacts;

        public ObservableCollection<Company> ParentCompanies { get; } = new();

        public EditCompanyViewModel(int companyId)
        {
            _companyId = companyId;
            LoadData();
        }

        private void LoadData()
        {
            LoadParentCompanies();

            var company = db.Companies
                .Include(c => c.IdParentCompanyNavigation)
                .FirstOrDefault(c => c.Id == _companyId);

            if (company == null) return;

            Name = company.Name;
            SelectedParentCompany = company.IdParentCompanyNavigation;
            Address = company.Adress;
            Contacts = company.Contacts;
        }

        private void LoadParentCompanies()
        {
            ParentCompanies.Clear();
            ParentCompanies.Add(null); 

            foreach (var company in db.Companies
                .Where(c => c.Id != _companyId) 
                .OrderBy(c => c.Name))
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

            if (db.Companies.Any(c => c.Name == Name.Trim() && c.Id != _companyId))
            {
                ShowMessage("Ошибка", $"Компания с названием '{Name}' уже существует");
                return false;
            }

            return true;
        }

        [RelayCommand]
        private void Save()
        {
            if (!ValidateFields()) return;

            try
            {
                var company = db.Companies.Find(_companyId);
                if (company == null) return;

                var oldName = company.Name;
                company.Name = Name.Trim();
                company.IdParentCompany = SelectedParentCompany?.Id;
                company.Adress = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim();
                company.Contacts = string.IsNullOrWhiteSpace(Contacts) ? null : Contacts.Trim();

                db.SaveChanges();

                if (oldName != company.Name)
                {
                    var dictEntry = db.Dictionaries.FirstOrDefault(d => d.Key == "company" && d.Value == oldName);
                    if (dictEntry != null)
                    {
                        dictEntry.Value = company.Name;
                        db.SaveChanges();
                    }
                }

                ShowMessage("Успешно", "Компания успешно обновлена");
                NavigationService.GoToCompanies();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при обновлении компании: {ex.Message}");
                ShowMessage("Ошибка", $"Не удалось обновить компанию: {ex.Message}");
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            NavigationService.GoToCompanies();
        }
    }
}